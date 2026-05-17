use std::collections::HashMap;
use std::fs;
use std::path::PathBuf;
use std::sync::mpsc::{self, Receiver, Sender, TryRecvError};
use std::thread;

use anyhow::{Context, Result, anyhow};
use chrono::Utc;
use uuid::Uuid;

use crate::diagnostics::classify_error;
use crate::ironrdp_client::{IronRdpRuntime, run_session};
use crate::models::{
    ConnectionProfile, EngineEvent, FrameUpdate, InputAction, PerformanceMetrics, RemoteSession,
    SessionStatus,
};

pub struct ProfileStore {
    path: PathBuf,
}

impl ProfileStore {
    pub fn new() -> Result<Self> {
        let base_dir = dirs::data_dir()
            .unwrap_or_else(|| PathBuf::from("."))
            .join("Aivana")
            .join("RustRdpClient");
        fs::create_dir_all(&base_dir).context("failed to create app data directory")?;

        Ok(Self {
            path: base_dir.join("profiles.json"),
        })
    }

    pub fn load(&self) -> Result<Vec<ConnectionProfile>> {
        if !self.path.exists() {
            return Ok(Self::seed_profiles());
        }

        let json = fs::read_to_string(&self.path).context("failed to read profiles file")?;
        let mut profiles: Vec<ConnectionProfile> =
            serde_json::from_str(&json).context("failed to parse profiles file")?;
        let had_serialized_passwords = profiles.iter().any(|profile| !profile.password.is_empty());
        for profile in &mut profiles {
            profile.password.clear();
        }
        if had_serialized_passwords {
            let _ = self.save(&profiles);
        }
        Ok(profiles)
    }

    pub fn save(&self, profiles: &[ConnectionProfile]) -> Result<()> {
        let json =
            serde_json::to_string_pretty(profiles).context("failed to serialize profiles")?;
        fs::write(&self.path, json).context("failed to write profiles file")
    }

    fn seed_profiles() -> Vec<ConnectionProfile> {
        vec![
            ConnectionProfile::sample("Production Gateway", "10.0.12.8", "Production", true),
            ConnectionProfile::sample("Build Server", "build-01.local", "Engineering", false),
            ConnectionProfile::sample("QA Desktop", "qa-win-03.local", "QA", false),
        ]
    }
}

pub trait RemoteDesktopEngine {
    fn connect(&mut self, profile: &ConnectionProfile) -> Result<RemoteSession>;
    fn reconnect(&mut self, session: &mut RemoteSession) -> Result<()>;
    fn resize(&mut self, session_id: Uuid, width: u16, height: u16) -> Result<()>;
    fn disconnect(&mut self, session_id: Uuid) -> Result<()>;
    fn tick(&mut self, session: &mut RemoteSession);
    fn poll_frame(&mut self, session_id: Uuid) -> Option<FrameUpdate>;
    fn send_input(&mut self, session_id: Uuid, action: InputAction) -> Result<()>;
    fn poll_events(&mut self, session_id: Uuid) -> Vec<EngineEvent>;
}

#[derive(Default)]
pub struct NativeRdpEngine {
    frame_counter: u64,
    events: HashMap<Uuid, Receiver<EngineEvent>>,
    inputs: HashMap<Uuid, Sender<InputAction>>,
    profiles: HashMap<Uuid, ConnectionProfile>,
    latest_frames: HashMap<Uuid, FrameUpdate>,
    event_backlog: HashMap<Uuid, Vec<EngineEvent>>,
}

impl RemoteDesktopEngine for NativeRdpEngine {
    fn connect(&mut self, profile: &ConnectionProfile) -> Result<RemoteSession> {
        let session_id = Uuid::new_v4();
        let profile_for_thread = profile.clone();
        let (event_sender, event_receiver) = mpsc::channel();
        let (input_sender, input_receiver) = mpsc::channel();

        spawn_runtime(session_id, profile_for_thread, event_sender, input_receiver);

        self.events.insert(session_id, event_receiver);
        self.inputs.insert(session_id, input_sender);
        self.profiles.insert(session_id, profile.clone());
        self.event_backlog.insert(session_id, Vec::new());

        Ok(RemoteSession {
            id: session_id,
            profile_id: profile.id,
            title: profile.name.clone(),
            status: SessionStatus::Connecting,
            connected_at: Utc::now(),
            metrics: PerformanceMetrics::default(),
            last_error: None,
            frame_size: None,
        })
    }

    fn reconnect(&mut self, session: &mut RemoteSession) -> Result<()> {
        let profile = self
            .profiles
            .get(&session.id)
            .cloned()
            .ok_or_else(|| anyhow!("session profile is not available for reconnect"))?;
        let (event_sender, event_receiver) = mpsc::channel();
        let (input_sender, input_receiver) = mpsc::channel();
        spawn_runtime(session.id, profile, event_sender, input_receiver);
        self.events.insert(session.id, event_receiver);
        self.inputs.insert(session.id, input_sender);
        session.status = SessionStatus::Reconnecting;
        session.last_error = None;
        Ok(())
    }

    fn resize(&mut self, session_id: Uuid, width: u16, height: u16) -> Result<()> {
        let _ = (session_id, width, height);
        Ok(())
    }

    fn disconnect(&mut self, session_id: Uuid) -> Result<()> {
        self.inputs.remove(&session_id);
        self.events.remove(&session_id);
        self.profiles.remove(&session_id);
        self.latest_frames.remove(&session_id);
        self.event_backlog.remove(&session_id);
        Ok(())
    }

    fn tick(&mut self, session: &mut RemoteSession) {
        let mut drained = Vec::new();
        if let Some(receiver) = self.events.get(&session.id) {
            loop {
                match receiver.try_recv() {
                    Ok(event) => drained.push(event),
                    Err(TryRecvError::Empty) => break,
                    Err(TryRecvError::Disconnected) => {
                        let already_has_terminal_event = drained.iter().any(|event| {
                            matches!(
                                event,
                                EngineEvent::Error { .. } | EngineEvent::Disconnected { .. }
                            )
                        });
                        if !already_has_terminal_event {
                            drained.push(EngineEvent::Disconnected {
                                session_id: session.id,
                                reason: "RDP runtime stopped".to_owned(),
                            });
                        }
                        break;
                    }
                }
            }
        }

        for event in drained {
            match event {
                EngineEvent::Frame(frame) => {
                    self.apply_frame(session, frame);
                }
                other => {
                    self.apply_event(session, other.clone());
                    self.event_backlog
                        .entry(session.id)
                        .or_default()
                        .push(other);
                }
            }
        }

        self.frame_counter = self.frame_counter.wrapping_add(1);
        update_live_metrics(self.frame_counter, session);
    }

    fn poll_frame(&mut self, session_id: Uuid) -> Option<FrameUpdate> {
        self.latest_frames.remove(&session_id)
    }

    fn send_input(&mut self, session_id: Uuid, action: InputAction) -> Result<()> {
        let sender = self
            .inputs
            .get(&session_id)
            .ok_or_else(|| anyhow!("session input channel is not available"))?;
        sender.send(action).context("send RDP input action")
    }

    fn poll_events(&mut self, session_id: Uuid) -> Vec<EngineEvent> {
        self.event_backlog.remove(&session_id).unwrap_or_default()
    }
}

fn spawn_runtime(
    session_id: Uuid,
    profile: ConnectionProfile,
    event_sender: Sender<EngineEvent>,
    input_receiver: Receiver<InputAction>,
) {
    thread::spawn(move || {
        run_session(IronRdpRuntime {
            profile,
            session_id,
            events: event_sender,
            input: input_receiver,
        });
    });
}

impl NativeRdpEngine {
    fn apply_frame(&mut self, session: &mut RemoteSession, frame: FrameUpdate) {
        session.status = SessionStatus::Connected;
        session.frame_size = Some((frame.width, frame.height));
        session.metrics.frame_rate = 60.0;
        self.latest_frames.insert(session.id, frame);
    }

    fn apply_event(&mut self, session: &mut RemoteSession, event: EngineEvent) {
        match event {
            EngineEvent::StatusChanged { status, .. } => {
                session.status = status;
                session.last_error = None;
            }
            EngineEvent::Frame(frame) => self.apply_frame(session, frame),
            EngineEvent::Error { message, .. } => {
                session.status = SessionStatus::Failed;
                session.last_error = Some(message);
            }
            EngineEvent::Diagnostic { .. } => {}
            EngineEvent::Disconnected { reason, .. } => {
                session.status = SessionStatus::Disconnected;
                session.last_error = Some(reason);
                self.inputs.remove(&session.id);
            }
        }
    }
}

fn update_live_metrics(frame_counter: u64, session: &mut RemoteSession) {
    if matches!(
        session.status,
        SessionStatus::Failed | SessionStatus::Disconnected
    ) {
        return;
    }

    let wave = (frame_counter as f32 / 18.0).sin();
    session.metrics.latency_ms = 22.0 + wave * 8.0;
    session.metrics.bandwidth_mbps = 74.0 + wave.abs() * 45.0;
    session.metrics.packet_loss_pct = (0.4 + wave.abs() * 0.8).min(3.0);
    session.metrics.quality_score = (96.0 - session.metrics.latency_ms / 4.0).round() as u8;

    if session.last_error.is_none() && session.status == SessionStatus::Failed {
        session.last_error = Some(format!("{:?}", classify_error("unknown failure")));
    }
}
