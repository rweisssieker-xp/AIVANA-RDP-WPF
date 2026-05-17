use std::io::{Read, Write};
use std::net::{TcpStream, ToSocketAddrs};
use std::sync::mpsc::{Receiver, Sender, TryRecvError};
use std::time::{Duration, Instant};

use anyhow::{Context, Result};
use ironrdp::connector::{self, ConnectionResult, Credentials};
use ironrdp::pdu::gcc::KeyboardType;
use ironrdp::pdu::rdp::capability_sets::MajorPlatformType;
use ironrdp::session::image::DecodedImage;
use ironrdp::session::{ActiveStage, ActiveStageOutput};
use ironrdp_pdu::rdp::client_info::{PerformanceFlags, TimezoneInfo};
use sspi::network_client::reqwest_network_client::ReqwestNetworkClient;
use tokio_rustls::rustls;

use crate::legacy_rdp::{LegacySecurityMode, detect_server_security};
use crate::models::ConnectionProfile;
use crate::models::{DirtyRegion, EngineEvent, FrameUpdate, InputAction, MouseButton};

type UpgradedFramed = ironrdp_blocking::Framed<RdpTransport>;

enum RdpTransport {
    Tls(rustls::StreamOwned<rustls::ClientConnection, TcpStream>),
    Plain(TcpStream),
}

impl Read for RdpTransport {
    fn read(&mut self, buf: &mut [u8]) -> std::io::Result<usize> {
        match self {
            Self::Tls(stream) => stream.read(buf),
            Self::Plain(stream) => stream.read(buf),
        }
    }
}

impl Write for RdpTransport {
    fn write(&mut self, buf: &[u8]) -> std::io::Result<usize> {
        match self {
            Self::Tls(stream) => stream.write(buf),
            Self::Plain(stream) => stream.write(buf),
        }
    }

    fn flush(&mut self) -> std::io::Result<()> {
        match self {
            Self::Tls(stream) => stream.flush(),
            Self::Plain(stream) => stream.flush(),
        }
    }
}

#[derive(Clone, Copy, Debug, PartialEq, Eq)]
enum RdpSecurityMode {
    NlaCredSsp,
    TlsGraphicalLogin,
    StandardRdp,
}

pub struct IronRdpRuntime {
    pub profile: ConnectionProfile,
    pub session_id: uuid::Uuid,
    pub events: Sender<EngineEvent>,
    pub input: Receiver<InputAction>,
}

pub fn probe_server_fingerprint(profile: &ConnectionProfile) -> Result<String> {
    block_unsupported_legacy_standard(profile)?;
    let config = build_config(profile);
    probe_server_fingerprint_with_config(profile, config).or_else(|err| {
        if should_try_tls_fallback(&err) {
            probe_server_fingerprint_with_config(profile, build_tls_config(profile))
                .context("legacy TLS graphical-login fallback")
        } else {
            Err(err)
        }
    })
}

fn probe_server_fingerprint_with_config(
    profile: &ConnectionProfile,
    config: connector::Config,
) -> Result<String> {
    let server_name = profile.host.clone();
    let server_addr = lookup_addr(&server_name, profile.port).context("lookup address")?;
    let tcp_stream =
        TcpStream::connect_timeout(&server_addr, Duration::from_secs(8)).context("TCP connect")?;
    let client_addr = tcp_stream
        .local_addr()
        .context("get local socket address")?;
    let mut framed = ironrdp_blocking::Framed::new(tcp_stream);
    let mut connector = connector::ClientConnector::new(config, client_addr);

    let should_upgrade =
        ironrdp_blocking::connect_begin(&mut framed, &mut connector).context("connection begin")?;
    let initial_stream = framed.into_inner_no_leftover();
    let (_upgraded_stream, server_public_key) =
        tls_upgrade(initial_stream, server_name).context("TLS upgrade")?;
    let _ = should_upgrade;
    Ok(fingerprint_bytes(&server_public_key))
}

pub fn run_session(runtime: IronRdpRuntime) {
    let session_id = runtime.session_id;
    let events = runtime.events.clone();
    let result = run_session_inner(runtime);
    if let Err(err) = result {
        let detail = format!("{err:#}");
        events
            .send(EngineEvent::Error {
                session_id,
                class: crate::diagnostics::classify_error(&detail),
                message: detail,
            })
            .ok();
    }
}

fn run_session_inner(runtime: IronRdpRuntime) -> Result<()> {
    let detection = detect_server_security(&runtime.profile).ok();
    let (connection_result, mut framed) = if detection
        .as_ref()
        .is_some_and(|detection| detection.mode == LegacySecurityMode::StandardRdp)
    {
        connect_standard(&runtime.profile).context("legacy Standard RDP Security connect")?
    } else {
        let config = build_config(&runtime.profile);
        connect(config, runtime.profile.host.clone(), runtime.profile.port).or_else(|err| {
            if should_try_tls_fallback(&err) {
                connect(
                    build_tls_config(&runtime.profile),
                    runtime.profile.host.clone(),
                    runtime.profile.port,
                )
                .context("legacy TLS graphical-login fallback")
            } else {
                Err(err)
            }
        })?
    };

    let mut image = DecodedImage::new(
        ironrdp_graphics::image_processing::PixelFormat::RgbA32,
        connection_result.desktop_size.width,
        connection_result.desktop_size.height,
    );
    let desktop_width = connection_result.desktop_size.width;
    let desktop_height = connection_result.desktop_size.height;
    let mut active_stage = ActiveStage::new(connection_result);
    let mut stats = RdpLoopStats::default();
    let mut last_stats_event = Instant::now();

    runtime
        .events
        .send(EngineEvent::StatusChanged {
            session_id: runtime.session_id,
            status: crate::models::SessionStatus::Connected,
        })
        .ok();
    runtime
        .events
        .send(EngineEvent::Diagnostic {
            session_id: runtime.session_id,
            message: format!(
                "ActiveStage ready, negotiated desktop {desktop_width}x{desktop_height}"
            ),
        })
        .ok();

    loop {
        if !drain_input(&runtime.input, &mut active_stage, &mut image, &mut framed)? {
            runtime
                .events
                .send(EngineEvent::Disconnected {
                    session_id: runtime.session_id,
                    reason: "Input channel closed by Aivana".to_owned(),
                })
                .ok();
            break;
        }

        match framed.read_pdu() {
            Ok((action, payload)) => {
                stats.pdus += 1;
                stats.last_payload_len = payload.len();
                match action {
                    ironrdp_pdu::Action::FastPath => stats.fast_path_pdus += 1,
                    ironrdp_pdu::Action::X224 => stats.x224_pdus += 1,
                }
                let outputs = active_stage.process(&mut image, action, &payload)?;
                let output_stats = process_outputs(
                    runtime.session_id,
                    outputs,
                    &mut framed,
                    &image,
                    &runtime.events,
                )?;
                stats.response_frames += output_stats.response_frames;
                stats.graphics_updates += output_stats.graphics_updates;
                stats.terminations += output_stats.terminations;
                stats.other_outputs += output_stats.other_outputs;
            }
            Err(e)
                if matches!(
                    e.kind(),
                    std::io::ErrorKind::WouldBlock | std::io::ErrorKind::TimedOut
                ) => {}
            Err(e) => {
                runtime
                    .events
                    .send(EngineEvent::Error {
                        session_id: runtime.session_id,
                        class: crate::diagnostics::classify_error(&e.to_string()),
                        message: e.to_string(),
                    })
                    .ok();
                break;
            }
        }

        if stats.pdus <= 5
            || stats.graphics_updates == 0 && last_stats_event.elapsed() >= Duration::from_secs(3)
            || last_stats_event.elapsed() >= Duration::from_secs(10)
        {
            runtime
                .events
                .send(EngineEvent::Diagnostic {
                    session_id: runtime.session_id,
                    message: stats.describe(),
                })
                .ok();
            last_stats_event = Instant::now();
        }
    }

    Ok(())
}

fn block_unsupported_legacy_standard(profile: &ConnectionProfile) -> Result<()> {
    if let Ok(detection) = detect_server_security(profile) {
        if detection.mode == LegacySecurityMode::StandardRdp {
            anyhow::bail!(
                "standard rdp security detected on {}:{}: {}. Native RC4 security exchange is not implemented yet.",
                profile.host,
                profile.port,
                detection.detail
            );
        }
    }
    Ok(())
}

fn build_config(profile: &ConnectionProfile) -> connector::Config {
    build_config_for_security(profile, RdpSecurityMode::NlaCredSsp)
}

fn build_tls_config(profile: &ConnectionProfile) -> connector::Config {
    build_config_for_security(profile, RdpSecurityMode::TlsGraphicalLogin)
}

fn build_config_for_security(
    profile: &ConnectionProfile,
    security_mode: RdpSecurityMode,
) -> connector::Config {
    let (enable_tls, enable_credssp) = match security_mode {
        RdpSecurityMode::NlaCredSsp => (false, true),
        RdpSecurityMode::TlsGraphicalLogin => (true, false),
        RdpSecurityMode::StandardRdp => (false, false),
    };

    connector::Config {
        credentials: Credentials::UsernamePassword {
            username: profile.username.clone(),
            password: profile.password.clone(),
        },
        domain: if profile.domain.trim().is_empty() {
            None
        } else {
            Some(profile.domain.clone())
        },
        enable_tls,
        enable_credssp,
        keyboard_type: KeyboardType::IbmEnhanced,
        keyboard_subtype: 0,
        keyboard_layout: 0,
        keyboard_functional_keys_count: 12,
        ime_file_name: String::new(),
        dig_product_id: String::new(),
        desktop_size: connector::DesktopSize {
            width: 1280,
            height: 800,
        },
        bitmap: None,
        client_build: 0,
        client_name: "aivana-rust-rdp-client".to_owned(),
        client_dir: "native-rust".to_owned(),

        #[cfg(windows)]
        platform: MajorPlatformType::WINDOWS,
        #[cfg(target_os = "macos")]
        platform: MajorPlatformType::MACINTOSH,
        #[cfg(target_os = "ios")]
        platform: MajorPlatformType::IOS,
        #[cfg(target_os = "linux")]
        platform: MajorPlatformType::UNIX,
        #[cfg(target_os = "android")]
        platform: MajorPlatformType::ANDROID,
        #[cfg(target_os = "freebsd")]
        platform: MajorPlatformType::UNIX,
        #[cfg(target_os = "dragonfly")]
        platform: MajorPlatformType::UNIX,
        #[cfg(target_os = "openbsd")]
        platform: MajorPlatformType::UNIX,
        #[cfg(target_os = "netbsd")]
        platform: MajorPlatformType::UNIX,

        enable_server_pointer: false,
        request_data: None,
        autologon: false,
        enable_audio_playback: false,
        pointer_software_rendering: true,
        performance_flags: PerformanceFlags::default(),
        desktop_scale_factor: 0,
        hardware_id: None,
        license_cache: None,
        timezone_info: TimezoneInfo::default(),
    }
}

fn connect(
    config: connector::Config,
    server_name: String,
    port: u16,
) -> Result<(ConnectionResult, UpgradedFramed)> {
    let server_addr = lookup_addr(&server_name, port).context("lookup address")?;
    let tcp_stream =
        TcpStream::connect_timeout(&server_addr, Duration::from_secs(8)).context("TCP connect")?;
    tcp_stream
        .set_read_timeout(Some(Duration::from_secs(2)))
        .context("set read timeout")?;

    let client_addr = tcp_stream
        .local_addr()
        .context("get local socket address")?;
    let mut framed = ironrdp_blocking::Framed::new(tcp_stream);
    let mut connector = connector::ClientConnector::new(config, client_addr);

    let should_upgrade =
        ironrdp_blocking::connect_begin(&mut framed, &mut connector).context("connection begin")?;

    let initial_stream = framed.into_inner_no_leftover();
    let (upgraded_stream, server_public_key) =
        tls_upgrade(initial_stream, server_name.clone()).context("TLS upgrade")?;

    let upgraded = ironrdp_blocking::mark_as_upgraded(should_upgrade, &mut connector);
    let mut upgraded_framed = ironrdp_blocking::Framed::new(RdpTransport::Tls(upgraded_stream));
    let mut network_client = ReqwestNetworkClient;

    let connection_result = ironrdp_blocking::connect_finalize(
        upgraded,
        connector,
        &mut upgraded_framed,
        &mut network_client,
        server_name.into(),
        server_public_key,
        None,
    )
    .context("connection finalize")?;

    Ok((connection_result, upgraded_framed))
}

fn connect_standard(profile: &ConnectionProfile) -> Result<(ConnectionResult, UpgradedFramed)> {
    let server_addr = lookup_addr(&profile.host, profile.port).context("lookup address")?;
    let tcp_stream =
        TcpStream::connect_timeout(&server_addr, Duration::from_secs(8)).context("TCP connect")?;
    tcp_stream
        .set_read_timeout(Some(Duration::from_secs(2)))
        .context("set read timeout")?;

    let client_addr = tcp_stream
        .local_addr()
        .context("get local socket address")?;
    let mut framed = ironrdp_blocking::Framed::new(tcp_stream);
    let mut connector = connector::ClientConnector::new(
        build_config_for_security(profile, RdpSecurityMode::StandardRdp),
        client_addr,
    );

    let should_upgrade =
        ironrdp_blocking::connect_begin(&mut framed, &mut connector).context("connection begin")?;
    let plain_stream = framed.into_inner_no_leftover();
    let upgraded = ironrdp_blocking::mark_as_upgraded(should_upgrade, &mut connector);
    let mut upgraded_framed = ironrdp_blocking::Framed::new(RdpTransport::Plain(plain_stream));
    let mut network_client = ReqwestNetworkClient;

    let connection_result = ironrdp_blocking::connect_finalize(
        upgraded,
        connector,
        &mut upgraded_framed,
        &mut network_client,
        profile.host.clone().into(),
        Vec::new(),
        None,
    )
    .context("connection finalize")?;

    Ok((connection_result, upgraded_framed))
}

fn drain_input(
    input: &Receiver<InputAction>,
    active_stage: &mut ActiveStage,
    image: &mut DecodedImage,
    framed: &mut UpgradedFramed,
) -> Result<bool> {
    loop {
        match input.try_recv() {
            Ok(action) => {
                let events = input_action_to_fastpath(action);
                if events.is_empty() {
                    continue;
                }

                let outputs = active_stage.process_fastpath_input(image, &events)?;
                for output in outputs {
                    if let ActiveStageOutput::ResponseFrame(frame) = output {
                        framed.write_all(&frame).context("write input frame")?;
                    }
                }
            }
            Err(TryRecvError::Empty) => return Ok(true),
            Err(TryRecvError::Disconnected) => return Ok(false),
        }
    }
}

fn process_outputs(
    session_id: uuid::Uuid,
    outputs: Vec<ActiveStageOutput>,
    framed: &mut UpgradedFramed,
    image: &DecodedImage,
    events: &Sender<EngineEvent>,
) -> Result<OutputStats> {
    let mut stats = OutputStats::default();
    for output in outputs {
        match output {
            ActiveStageOutput::ResponseFrame(frame) => {
                stats.response_frames += 1;
                framed.write_all(&frame).context("write response")?
            }
            ActiveStageOutput::GraphicsUpdate(region) => {
                stats.graphics_updates += 1;
                events
                    .send(EngineEvent::Frame(FrameUpdate {
                        session_id,
                        width: image.width(),
                        height: image.height(),
                        pixels_rgba: image.data().to_vec(),
                        dirty_regions: vec![DirtyRegion {
                            left: region.left,
                            top: region.top,
                            right: region.right,
                            bottom: region.bottom,
                        }],
                        frame_hash: frame_hash(image.data()),
                        captured_at: chrono::Utc::now(),
                    }))
                    .ok();
            }
            ActiveStageOutput::Terminate(reason) => {
                stats.terminations += 1;
                events
                    .send(EngineEvent::Disconnected {
                        session_id,
                        reason: reason.description(),
                    })
                    .ok();
            }
            _ => stats.other_outputs += 1,
        }
    }

    Ok(stats)
}

#[derive(Default)]
struct RdpLoopStats {
    pdus: u64,
    fast_path_pdus: u64,
    x224_pdus: u64,
    response_frames: u64,
    graphics_updates: u64,
    terminations: u64,
    other_outputs: u64,
    last_payload_len: usize,
}

impl RdpLoopStats {
    fn describe(&self) -> String {
        format!(
            "pdus={} fast_path={} x224={} responses={} graphics_updates={} other_outputs={} terminations={} last_payload={} bytes",
            self.pdus,
            self.fast_path_pdus,
            self.x224_pdus,
            self.response_frames,
            self.graphics_updates,
            self.other_outputs,
            self.terminations,
            self.last_payload_len
        )
    }
}

#[derive(Default)]
struct OutputStats {
    response_frames: u64,
    graphics_updates: u64,
    terminations: u64,
    other_outputs: u64,
}

fn input_action_to_fastpath(
    action: InputAction,
) -> Vec<ironrdp_pdu::input::fast_path::FastPathInputEvent> {
    use ironrdp_pdu::input::fast_path::{FastPathInputEvent, KeyboardFlags};
    use ironrdp_pdu::input::mouse::{MousePdu, PointerFlags};

    match action {
        InputAction::MovePointer { x, y } => vec![FastPathInputEvent::MouseEvent(MousePdu {
            flags: PointerFlags::MOVE,
            number_of_wheel_rotation_units: 0,
            x_position: x,
            y_position: y,
        })],
        InputAction::Click { x, y, button } => {
            let button_flag = match button {
                MouseButton::Left => PointerFlags::LEFT_BUTTON,
                MouseButton::Right => PointerFlags::RIGHT_BUTTON,
                MouseButton::Middle => PointerFlags::MIDDLE_BUTTON_OR_WHEEL,
            };
            vec![
                FastPathInputEvent::MouseEvent(MousePdu {
                    flags: PointerFlags::DOWN | button_flag,
                    number_of_wheel_rotation_units: 0,
                    x_position: x,
                    y_position: y,
                }),
                FastPathInputEvent::MouseEvent(MousePdu {
                    flags: button_flag,
                    number_of_wheel_rotation_units: 0,
                    x_position: x,
                    y_position: y,
                }),
            ]
        }
        InputAction::DoubleClick { x, y, button } => {
            let mut first = input_action_to_fastpath(InputAction::Click { x, y, button });
            first.extend(input_action_to_fastpath(InputAction::Click {
                x,
                y,
                button,
            }));
            first
        }
        InputAction::Scroll { x, y, delta } => vec![FastPathInputEvent::MouseEvent(MousePdu {
            flags: PointerFlags::VERTICAL_WHEEL
                | if delta < 0 {
                    PointerFlags::WHEEL_NEGATIVE
                } else {
                    PointerFlags::empty()
                },
            number_of_wheel_rotation_units: delta,
            x_position: x,
            y_position: y,
        })],
        InputAction::TypeText { text } => text
            .encode_utf16()
            .flat_map(|code| {
                [
                    FastPathInputEvent::UnicodeKeyboardEvent(KeyboardFlags::empty(), code),
                    FastPathInputEvent::UnicodeKeyboardEvent(KeyboardFlags::RELEASE, code),
                ]
            })
            .collect(),
        InputAction::Hotkey { keys } => {
            let mut pressed = Vec::new();
            let mut events = Vec::new();
            for key in keys {
                if let Some((scan_code, extended)) = key_to_scancode(&key) {
                    let flags = if extended {
                        KeyboardFlags::EXTENDED
                    } else {
                        KeyboardFlags::empty()
                    };
                    events.push(FastPathInputEvent::KeyboardEvent(flags, scan_code));
                    pressed.push((scan_code, flags));
                }
            }
            for (scan_code, flags) in pressed.into_iter().rev() {
                events.push(FastPathInputEvent::KeyboardEvent(
                    flags | KeyboardFlags::RELEASE,
                    scan_code,
                ));
            }
            events
        }
        InputAction::Wait { .. } | InputAction::Screenshot | InputAction::Verify { .. } => {
            Vec::new()
        }
    }
}

fn key_to_scancode(key: &str) -> Option<(u8, bool)> {
    match key.to_ascii_lowercase().as_str() {
        "a" => Some((0x1e, false)),
        "b" => Some((0x30, false)),
        "c" => Some((0x2e, false)),
        "d" => Some((0x20, false)),
        "e" => Some((0x12, false)),
        "f" => Some((0x21, false)),
        "g" => Some((0x22, false)),
        "h" => Some((0x23, false)),
        "i" => Some((0x17, false)),
        "j" => Some((0x24, false)),
        "k" => Some((0x25, false)),
        "l" => Some((0x26, false)),
        "m" => Some((0x32, false)),
        "n" => Some((0x31, false)),
        "o" => Some((0x18, false)),
        "p" => Some((0x19, false)),
        "q" => Some((0x10, false)),
        "r" => Some((0x13, false)),
        "s" => Some((0x1f, false)),
        "t" => Some((0x14, false)),
        "u" => Some((0x16, false)),
        "v" => Some((0x2f, false)),
        "w" => Some((0x11, false)),
        "x" => Some((0x2d, false)),
        "y" => Some((0x15, false)),
        "z" => Some((0x2c, false)),
        "enter" => Some((0x1c, false)),
        "tab" => Some((0x0f, false)),
        "esc" | "escape" => Some((0x01, false)),
        "space" => Some((0x39, false)),
        "ctrl" | "control" => Some((0x1d, false)),
        "alt" => Some((0x38, false)),
        "shift" => Some((0x2a, false)),
        "win" | "meta" | "windows" => Some((0x5b, true)),
        "del" | "delete" => Some((0x53, true)),
        "home" => Some((0x47, true)),
        "end" => Some((0x4f, true)),
        "pageup" => Some((0x49, true)),
        "pagedown" => Some((0x51, true)),
        "left" => Some((0x4b, true)),
        "right" => Some((0x4d, true)),
        "up" => Some((0x48, true)),
        "down" => Some((0x50, true)),
        _ => None,
    }
}

fn frame_hash(bytes: &[u8]) -> u64 {
    let mut hash = 0xcbf29ce484222325u64;
    for byte in bytes.iter().step_by(257) {
        hash ^= u64::from(*byte);
        hash = hash.wrapping_mul(0x100000001b3);
    }
    hash
}

fn fingerprint_bytes(bytes: &[u8]) -> String {
    let hash = frame_hash(bytes);
    format!("rdp-server-key-{hash:016x}")
}

fn should_try_tls_fallback(err: &anyhow::Error) -> bool {
    let message = format!("{err:#}").to_lowercase();
    message.contains("negotiation failure")
        && !message.contains("standard rdp security")
        && !message.contains("auth")
        && !message.contains("password")
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::models::ConnectionProfile;

    #[test]
    #[ignore = "requires AIVANA_RDP_TEST_HOST and a reachable RDP endpoint"]
    fn manual_probe_server_fingerprint_or_security_mode() {
        let host = std::env::var("AIVANA_RDP_TEST_HOST").expect("AIVANA_RDP_TEST_HOST");
        let port = std::env::var("AIVANA_RDP_TEST_PORT")
            .ok()
            .and_then(|value| value.parse::<u16>().ok())
            .unwrap_or(3389);
        let mut profile = ConnectionProfile::sample("manual-rdp-probe", &host, "Manual", false);
        profile.port = port;
        match probe_server_fingerprint(&profile) {
            Ok(fingerprint) => assert!(
                fingerprint.starts_with("rdp-server-key-"),
                "unexpected fingerprint format: {fingerprint}"
            ),
            Err(err) => {
                let message = format!("{err:#}").to_lowercase();
                assert!(
                    message.contains("standard rdp security"),
                    "unexpected RDP probe error: {err:#}"
                );
            }
        }
    }

    #[test]
    fn standard_rdp_detection_error_is_not_tls_fallback_candidate() {
        let err =
            anyhow::anyhow!("standard rdp security detected on legacy:3389: Standard RDP Security");

        assert!(!should_try_tls_fallback(&err));
    }

    #[test]
    fn runtime_failure_sends_error_event() {
        let (events, receiver) = std::sync::mpsc::channel();
        let (_input, input_receiver) = std::sync::mpsc::channel();
        let mut profile =
            ConnectionProfile::sample("invalid-rdp-host", "203.0.113.10", "Manual", false);
        profile.port = 9;
        let session_id = uuid::Uuid::new_v4();

        run_session(IronRdpRuntime {
            profile,
            session_id,
            events,
            input: input_receiver,
        });

        let event = receiver.try_recv().expect("runtime error event");
        assert!(matches!(event, EngineEvent::Error { .. }));
    }

    #[test]
    #[ignore = "requires AIVANA_RDP_TEST_HOST pointing at a Standard RDP Security endpoint"]
    fn manual_standard_rdp_connect_reaches_connector() {
        let host = std::env::var("AIVANA_RDP_TEST_HOST").expect("AIVANA_RDP_TEST_HOST");
        let port = std::env::var("AIVANA_RDP_TEST_PORT")
            .ok()
            .and_then(|value| value.parse::<u16>().ok())
            .unwrap_or(3389);
        let mut profile = ConnectionProfile::sample("manual-standard-rdp", &host, "Manual", false);
        profile.port = port;

        let result = connect_standard(&profile);
        match &result {
            Ok((connection, _)) => println!(
                "manual standard RDP connect reached desktop {}x{}",
                connection.desktop_size.width, connection.desktop_size.height
            ),
            Err(err) => println!("manual standard RDP connect error: {err:#}"),
        }
        if let Err(err) = &result {
            let message = format!("{err:#}").to_lowercase();
            assert!(
                !message.contains("standard rdp security is not supported")
                    && !message.contains("server only supports standard rdp security"),
                "legacy connector still blocks Standard RDP Security: {err:#}"
            );
        }
    }

    #[test]
    #[ignore = "requires AIVANA_RDP_TEST_HOST, AIVANA_RDP_TEST_USER and AIVANA_RDP_TEST_PASSWORD"]
    fn manual_rdp_session_receives_framebuffer() {
        let host = std::env::var("AIVANA_RDP_TEST_HOST").expect("AIVANA_RDP_TEST_HOST");
        let username = std::env::var("AIVANA_RDP_TEST_USER").expect("AIVANA_RDP_TEST_USER");
        let password = std::env::var("AIVANA_RDP_TEST_PASSWORD").expect("AIVANA_RDP_TEST_PASSWORD");
        let domain = std::env::var("AIVANA_RDP_TEST_DOMAIN").unwrap_or_default();
        let port = std::env::var("AIVANA_RDP_TEST_PORT")
            .ok()
            .and_then(|value| value.parse::<u16>().ok())
            .unwrap_or(3389);

        let (events, receiver) = std::sync::mpsc::channel();
        let (input, input_receiver) = std::sync::mpsc::channel();
        let mut profile = ConnectionProfile::sample("manual-rdp-session", &host, "Manual", false);
        profile.port = port;
        profile.username = username;
        profile.password = password;
        profile.domain = domain;
        let session_id = uuid::Uuid::new_v4();

        std::thread::spawn(move || {
            run_session(IronRdpRuntime {
                profile,
                session_id,
                events,
                input: input_receiver,
            });
        });

        let deadline = std::time::Instant::now() + std::time::Duration::from_secs(30);
        let mut saw_connected = false;
        let mut saw_frame = false;
        let mut last_diagnostic = String::new();

        while std::time::Instant::now() < deadline {
            match receiver.recv_timeout(std::time::Duration::from_millis(500)) {
                Ok(EngineEvent::StatusChanged { status, .. }) => {
                    println!("session status: {}", status.label());
                    saw_connected |= status == crate::models::SessionStatus::Connected;
                }
                Ok(EngineEvent::Diagnostic { message, .. }) => {
                    println!("session diagnostic: {message}");
                    last_diagnostic = message;
                }
                Ok(EngineEvent::Frame(frame)) => {
                    println!(
                        "session framebuffer: {}x{} hash={} dirty_regions={}",
                        frame.width,
                        frame.height,
                        frame.frame_hash,
                        frame.dirty_regions.len()
                    );
                    saw_frame = true;
                    break;
                }
                Ok(EngineEvent::Error { class, message, .. }) => {
                    panic!("session error {class:?}: {message}");
                }
                Ok(EngineEvent::Disconnected { reason, .. }) => {
                    panic!("session disconnected before framebuffer: {reason}");
                }
                Err(std::sync::mpsc::RecvTimeoutError::Timeout) => {}
                Err(std::sync::mpsc::RecvTimeoutError::Disconnected) => {
                    panic!("session event channel closed before framebuffer");
                }
            }
        }

        drop(input);
        assert!(saw_connected, "session never reached Connected");
        assert!(
            saw_frame,
            "session reached Connected but no framebuffer arrived; last diagnostic: {last_diagnostic}"
        );
    }
}

fn lookup_addr(hostname: &str, port: u16) -> Result<std::net::SocketAddr> {
    (hostname, port)
        .to_socket_addrs()?
        .next()
        .context("socket address not found")
}

fn tls_upgrade(
    stream: TcpStream,
    server_name: String,
) -> Result<(
    rustls::StreamOwned<rustls::ClientConnection, TcpStream>,
    Vec<u8>,
)> {
    let mut config = rustls::client::ClientConfig::builder()
        .dangerous()
        .with_custom_certificate_verifier(std::sync::Arc::new(danger::NoCertificateVerification))
        .with_no_client_auth();

    config.key_log = std::sync::Arc::new(rustls::KeyLogFile::new());
    config.resumption = rustls::client::Resumption::disabled();

    let config = std::sync::Arc::new(config);
    let server_name = server_name.try_into()?;
    let client = rustls::ClientConnection::new(config, server_name)?;
    let mut tls_stream = rustls::StreamOwned::new(client, stream);

    tls_stream.flush()?;

    let cert = tls_stream
        .conn
        .peer_certificates()
        .and_then(|certificates| certificates.first())
        .context("peer certificate is missing")?;

    let server_public_key = extract_tls_server_public_key(cert)?;
    Ok((tls_stream, server_public_key))
}

fn extract_tls_server_public_key(cert: &[u8]) -> Result<Vec<u8>> {
    use x509_cert::der::Decode as _;

    let cert = x509_cert::Certificate::from_der(cert)?;
    let server_public_key = cert
        .tbs_certificate
        .subject_public_key_info
        .subject_public_key
        .as_bytes()
        .context("subject public key BIT STRING is not aligned")?
        .to_owned();

    Ok(server_public_key)
}

mod danger {
    use tokio_rustls::rustls::client::danger::{
        HandshakeSignatureValid, ServerCertVerified, ServerCertVerifier,
    };
    use tokio_rustls::rustls::{DigitallySignedStruct, Error, SignatureScheme, pki_types};

    #[derive(Debug)]
    pub(super) struct NoCertificateVerification;

    impl ServerCertVerifier for NoCertificateVerification {
        fn verify_server_cert(
            &self,
            _: &pki_types::CertificateDer<'_>,
            _: &[pki_types::CertificateDer<'_>],
            _: &pki_types::ServerName<'_>,
            _: &[u8],
            _: pki_types::UnixTime,
        ) -> Result<ServerCertVerified, Error> {
            Ok(ServerCertVerified::assertion())
        }

        fn verify_tls12_signature(
            &self,
            _: &[u8],
            _: &pki_types::CertificateDer<'_>,
            _: &DigitallySignedStruct,
        ) -> Result<HandshakeSignatureValid, Error> {
            Ok(HandshakeSignatureValid::assertion())
        }

        fn verify_tls13_signature(
            &self,
            _: &[u8],
            _: &pki_types::CertificateDer<'_>,
            _: &DigitallySignedStruct,
        ) -> Result<HandshakeSignatureValid, Error> {
            Ok(HandshakeSignatureValid::assertion())
        }

        fn supported_verify_schemes(&self) -> Vec<SignatureScheme> {
            vec![
                SignatureScheme::RSA_PKCS1_SHA1,
                SignatureScheme::ECDSA_SHA1_Legacy,
                SignatureScheme::RSA_PKCS1_SHA256,
                SignatureScheme::ECDSA_NISTP256_SHA256,
                SignatureScheme::RSA_PKCS1_SHA384,
                SignatureScheme::ECDSA_NISTP384_SHA384,
                SignatureScheme::RSA_PKCS1_SHA512,
                SignatureScheme::ECDSA_NISTP521_SHA512,
                SignatureScheme::RSA_PSS_SHA256,
                SignatureScheme::RSA_PSS_SHA384,
                SignatureScheme::RSA_PSS_SHA512,
                SignatureScheme::ED25519,
                SignatureScheme::ED448,
            ]
        }
    }
}
