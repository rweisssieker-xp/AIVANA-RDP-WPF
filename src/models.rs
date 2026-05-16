use chrono::{DateTime, Utc};
use serde::{Deserialize, Serialize};
use uuid::Uuid;

#[derive(Clone, Debug, Deserialize, Serialize, PartialEq, Eq)]
pub enum Protocol {
    Rdp,
    Ssh,
    Vnc,
}

impl Protocol {
    pub fn label(&self) -> &'static str {
        match self {
            Self::Rdp => "RDP",
            Self::Ssh => "SSH",
            Self::Vnc => "VNC",
        }
    }

    pub fn default_port(&self) -> u16 {
        match self {
            Self::Rdp => 3389,
            Self::Ssh => 22,
            Self::Vnc => 5900,
        }
    }
}

#[derive(Clone, Debug, Deserialize, Serialize)]
pub struct ConnectionProfile {
    pub id: Uuid,
    pub name: String,
    pub host: String,
    pub port: u16,
    pub username: String,
    pub domain: String,
    pub protocol: Protocol,
    pub group: String,
    pub tags: Vec<String>,
    pub favorite: bool,
    pub created_at: DateTime<Utc>,
    pub updated_at: DateTime<Utc>,
}

impl ConnectionProfile {
    pub fn sample(name: &str, host: &str, group: &str, favorite: bool) -> Self {
        let now = Utc::now();
        Self {
            id: Uuid::new_v4(),
            name: name.to_owned(),
            host: host.to_owned(),
            port: Protocol::Rdp.default_port(),
            username: "Administrator".to_owned(),
            domain: String::new(),
            protocol: Protocol::Rdp,
            group: group.to_owned(),
            tags: vec!["windows".to_owned(), "rdp".to_owned()],
            favorite,
            created_at: now,
            updated_at: now,
        }
    }
}

#[derive(Clone, Debug)]
pub struct RemoteSession {
    pub id: Uuid,
    pub profile_id: Uuid,
    pub title: String,
    pub status: SessionStatus,
    pub connected_at: DateTime<Utc>,
    pub metrics: PerformanceMetrics,
}

#[derive(Clone, Copy, Debug, PartialEq, Eq)]
#[allow(dead_code)]
pub enum SessionStatus {
    Connecting,
    Connected,
    Suspended,
    Disconnected,
    Failed,
}

impl SessionStatus {
    pub fn label(self) -> &'static str {
        match self {
            Self::Connecting => "Connecting",
            Self::Connected => "Connected",
            Self::Suspended => "Suspended",
            Self::Disconnected => "Disconnected",
            Self::Failed => "Failed",
        }
    }
}

#[derive(Clone, Copy, Debug)]
pub struct PerformanceMetrics {
    pub latency_ms: f32,
    pub bandwidth_mbps: f32,
    pub frame_rate: f32,
    pub packet_loss_pct: f32,
    pub quality_score: u8,
}

impl Default for PerformanceMetrics {
    fn default() -> Self {
        Self {
            latency_ms: 18.0,
            bandwidth_mbps: 85.0,
            frame_rate: 60.0,
            packet_loss_pct: 0.2,
            quality_score: 92,
        }
    }
}

#[derive(Clone, Debug, Default)]
pub struct DraftProfile {
    pub name: String,
    pub host: String,
    pub port: String,
    pub username: String,
    pub domain: String,
    pub group: String,
    pub tags: String,
    pub favorite: bool,
}

impl From<&ConnectionProfile> for DraftProfile {
    fn from(profile: &ConnectionProfile) -> Self {
        Self {
            name: profile.name.clone(),
            host: profile.host.clone(),
            port: profile.port.to_string(),
            username: profile.username.clone(),
            domain: profile.domain.clone(),
            group: profile.group.clone(),
            tags: profile.tags.join(", "),
            favorite: profile.favorite,
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn protocol_default_ports_match_common_remote_desktop_ports() {
        assert_eq!(Protocol::Rdp.default_port(), 3389);
        assert_eq!(Protocol::Ssh.default_port(), 22);
        assert_eq!(Protocol::Vnc.default_port(), 5900);
    }

    #[test]
    fn draft_profile_preserves_editable_profile_fields() {
        let mut profile = ConnectionProfile::sample("Prod", "10.0.0.5", "Ops", true);
        profile.username = "admin".to_owned();
        profile.domain = "corp".to_owned();
        profile.tags = vec!["windows".to_owned(), "critical".to_owned()];

        let draft = DraftProfile::from(&profile);

        assert_eq!(draft.name, "Prod");
        assert_eq!(draft.host, "10.0.0.5");
        assert_eq!(draft.port, "3389");
        assert_eq!(draft.username, "admin");
        assert_eq!(draft.domain, "corp");
        assert_eq!(draft.group, "Ops");
        assert_eq!(draft.tags, "windows, critical");
        assert!(draft.favorite);
    }
}
