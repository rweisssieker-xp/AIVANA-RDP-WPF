use std::io::Write as _;
use std::net::{TcpStream, ToSocketAddrs};
use std::sync::mpsc::{Receiver, Sender, TryRecvError};
use std::time::Duration;

use anyhow::{Context, Result};
use ironrdp::connector::{self, ConnectionResult, Credentials};
use ironrdp::pdu::gcc::KeyboardType;
use ironrdp::pdu::rdp::capability_sets::MajorPlatformType;
use ironrdp::session::image::DecodedImage;
use ironrdp::session::{ActiveStage, ActiveStageOutput};
use ironrdp_pdu::rdp::client_info::{PerformanceFlags, TimezoneInfo};
use sspi::network_client::reqwest_network_client::ReqwestNetworkClient;
use tokio_rustls::rustls;

use crate::models::ConnectionProfile;
use crate::models::{DirtyRegion, EngineEvent, FrameUpdate, InputAction, MouseButton};

type UpgradedFramed =
    ironrdp_blocking::Framed<rustls::StreamOwned<rustls::ClientConnection, TcpStream>>;

pub struct IronRdpSessionInfo {
    pub width: u16,
    pub height: u16,
}

pub struct IronRdpRuntime {
    pub profile: ConnectionProfile,
    pub session_id: uuid::Uuid,
    pub events: Sender<EngineEvent>,
    pub input: Receiver<InputAction>,
}

pub fn connect_profile(profile: &ConnectionProfile) -> Result<IronRdpSessionInfo> {
    let config = build_config(profile);
    let (connection_result, framed) =
        connect(config, profile.host.clone(), profile.port).context("native IronRDP connect")?;

    let mut image = DecodedImage::new(
        ironrdp_graphics::image_processing::PixelFormat::RgbA32,
        connection_result.desktop_size.width,
        connection_result.desktop_size.height,
    );

    pump_initial_frames(connection_result, framed, &mut image).context("initial active stage")?;

    Ok(IronRdpSessionInfo {
        width: image.width(),
        height: image.height(),
    })
}

pub fn run_session(runtime: IronRdpRuntime) {
    let result = run_session_inner(runtime);
    if let Err(err) = result {
        // The event channel may be gone during shutdown; no extra action needed.
        let _ = err;
    }
}

fn run_session_inner(runtime: IronRdpRuntime) -> Result<()> {
    let config = build_config(&runtime.profile);
    let (connection_result, mut framed) =
        connect(config, runtime.profile.host.clone(), runtime.profile.port)
            .context("native IronRDP connect")?;

    let mut image = DecodedImage::new(
        ironrdp_graphics::image_processing::PixelFormat::RgbA32,
        connection_result.desktop_size.width,
        connection_result.desktop_size.height,
    );
    let mut active_stage = ActiveStage::new(connection_result);

    runtime
        .events
        .send(EngineEvent::StatusChanged {
            session_id: runtime.session_id,
            status: crate::models::SessionStatus::Connected,
        })
        .ok();

    loop {
        drain_input(&runtime.input, &mut active_stage, &mut image, &mut framed)?;

        match framed.read_pdu() {
            Ok((action, payload)) => {
                let outputs = active_stage.process(&mut image, action, &payload)?;
                process_outputs(
                    runtime.session_id,
                    outputs,
                    &mut framed,
                    &image,
                    &runtime.events,
                )?;
            }
            Err(e) if e.kind() == std::io::ErrorKind::WouldBlock => {}
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
    }

    Ok(())
}

fn build_config(profile: &ConnectionProfile) -> connector::Config {
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
        enable_tls: false,
        enable_credssp: true,
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
    let mut upgraded_framed = ironrdp_blocking::Framed::new(upgraded_stream);
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

fn pump_initial_frames(
    connection_result: ConnectionResult,
    mut framed: UpgradedFramed,
    image: &mut DecodedImage,
) -> Result<()> {
    let mut active_stage = ActiveStage::new(connection_result);

    for _ in 0..20 {
        let (action, payload) = match framed.read_pdu() {
            Ok((action, payload)) => (action, payload),
            Err(e) if e.kind() == std::io::ErrorKind::WouldBlock => break,
            Err(e) => return Err(anyhow::Error::new(e).context("read RDP frame")),
        };

        let outputs = active_stage.process(image, action, &payload)?;
        for output in outputs {
            match output {
                ActiveStageOutput::ResponseFrame(frame) => {
                    framed.write_all(&frame).context("write RDP response")?
                }
                ActiveStageOutput::Terminate(_) => return Ok(()),
                _ => {}
            }
        }
    }

    Ok(())
}

fn drain_input(
    input: &Receiver<InputAction>,
    active_stage: &mut ActiveStage,
    image: &mut DecodedImage,
    framed: &mut UpgradedFramed,
) -> Result<()> {
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
            Err(TryRecvError::Empty) => return Ok(()),
            Err(TryRecvError::Disconnected) => return Ok(()),
        }
    }
}

fn process_outputs(
    session_id: uuid::Uuid,
    outputs: Vec<ActiveStageOutput>,
    framed: &mut UpgradedFramed,
    image: &DecodedImage,
    events: &Sender<EngineEvent>,
) -> Result<()> {
    for output in outputs {
        match output {
            ActiveStageOutput::ResponseFrame(frame) => {
                framed.write_all(&frame).context("write response")?
            }
            ActiveStageOutput::GraphicsUpdate(region) => {
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
                events
                    .send(EngineEvent::Disconnected {
                        session_id,
                        reason: reason.description(),
                    })
                    .ok();
            }
            _ => {}
        }
    }

    Ok(())
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
        InputAction::Hotkey { .. }
        | InputAction::Wait { .. }
        | InputAction::Screenshot
        | InputAction::Verify { .. } => Vec::new(),
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
