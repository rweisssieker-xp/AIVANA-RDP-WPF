use std::fs;
use std::path::PathBuf;
#[cfg(target_os = "windows")]
use std::process::Command;
use std::{collections::HashMap, process::Child};

use anyhow::{Context, Result};
use chrono::Utc;
use uuid::Uuid;

use crate::models::{ConnectionProfile, PerformanceMetrics, RemoteSession, SessionStatus};

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
        let profiles = serde_json::from_str(&json).context("failed to parse profiles file")?;
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
    fn disconnect(&mut self, session_id: Uuid) -> Result<()>;
    fn tick(&mut self, session: &mut RemoteSession);
}

#[derive(Default)]
pub struct NativeRdpEngine {
    frame: u64,
    processes: HashMap<Uuid, Child>,
}

impl RemoteDesktopEngine for NativeRdpEngine {
    fn connect(&mut self, profile: &ConnectionProfile) -> Result<RemoteSession> {
        let child = spawn_rdp_process(profile)?;
        let session_id = Uuid::new_v4();
        if let Some(child) = child {
            self.processes.insert(session_id, child);
        }

        Ok(RemoteSession {
            id: session_id,
            profile_id: profile.id,
            title: profile.name.clone(),
            status: SessionStatus::Connected,
            connected_at: Utc::now(),
            metrics: PerformanceMetrics::default(),
        })
    }

    fn disconnect(&mut self, session_id: Uuid) -> Result<()> {
        if let Some(mut child) = self.processes.remove(&session_id) {
            let _ = child.kill();
        }
        Ok(())
    }

    fn tick(&mut self, session: &mut RemoteSession) {
        self.frame = self.frame.wrapping_add(1);
        let wave = (self.frame as f32 / 18.0).sin();
        session.metrics.latency_ms = 22.0 + wave * 8.0;
        session.metrics.bandwidth_mbps = 74.0 + wave.abs() * 45.0;
        session.metrics.frame_rate = 56.0 + wave.max(0.0) * 8.0;
        session.metrics.packet_loss_pct = (0.4 + wave.abs() * 0.8).min(3.0);
        session.metrics.quality_score = (96.0 - session.metrics.latency_ms / 4.0).round() as u8;
    }
}

#[cfg(target_os = "windows")]
fn spawn_rdp_process(profile: &ConnectionProfile) -> Result<Option<Child>> {
    let child = Command::new("mstsc.exe")
        .arg(format!("/v:{}:{}", profile.host, profile.port))
        .spawn()
        .with_context(|| format!("failed to launch mstsc.exe for {}", profile.host))?;

    Ok(Some(child))
}

#[cfg(not(target_os = "windows"))]
fn spawn_rdp_process(_profile: &ConnectionProfile) -> Result<Option<Child>> {
    Ok(None)
}
