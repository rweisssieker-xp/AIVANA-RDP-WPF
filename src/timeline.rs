use std::collections::HashMap;

use chrono::Utc;
use uuid::Uuid;

use crate::models::{BlackboxSnapshot, FrameUpdate, SessionEvent, SessionEventKind};
use crate::security::redact_secret_text;

pub trait TimelineStore {
    fn append_event(
        &mut self,
        session_id: Uuid,
        workspace_id: Option<Uuid>,
        kind: SessionEventKind,
        message: String,
    );
    fn events_for_session(&self, session_id: Uuid) -> Vec<SessionEvent>;
    fn append_snapshot(&mut self, reason: String, frame: &FrameUpdate);
    fn snapshots_for_session(&self, session_id: Uuid) -> Vec<BlackboxSnapshot>;
    fn export_incident_markdown(&self, session_id: Uuid) -> String;
    fn export_evidence_json(&self, session_id: Uuid) -> String;
}

#[derive(Default)]
pub struct InMemoryTimelineStore {
    events: HashMap<Uuid, Vec<SessionEvent>>,
    snapshots: HashMap<Uuid, Vec<BlackboxSnapshot>>,
}

impl TimelineStore for InMemoryTimelineStore {
    fn append_event(
        &mut self,
        session_id: Uuid,
        workspace_id: Option<Uuid>,
        kind: SessionEventKind,
        message: String,
    ) {
        let redacted_message = redact_secret_text(&message);
        self.events
            .entry(session_id)
            .or_default()
            .push(SessionEvent {
                id: Uuid::new_v4(),
                session_id,
                workspace_id,
                kind,
                redacted: redacted_message != message,
                message: redacted_message,
                created_at: Utc::now(),
            });
    }

    fn events_for_session(&self, session_id: Uuid) -> Vec<SessionEvent> {
        self.events.get(&session_id).cloned().unwrap_or_default()
    }

    fn append_snapshot(&mut self, reason: String, frame: &FrameUpdate) {
        self.snapshots
            .entry(frame.session_id)
            .or_default()
            .push(BlackboxSnapshot {
                id: Uuid::new_v4(),
                session_id: frame.session_id,
                reason: redact_secret_text(&reason),
                frame_hash: frame.frame_hash,
                width: frame.width,
                height: frame.height,
                captured_at: Utc::now(),
            });
    }

    fn snapshots_for_session(&self, session_id: Uuid) -> Vec<BlackboxSnapshot> {
        self.snapshots.get(&session_id).cloned().unwrap_or_default()
    }

    fn export_incident_markdown(&self, session_id: Uuid) -> String {
        let mut out = format!("# Aivana Incident Report\n\nSession: `{session_id}`\n\n");
        for event in self.events_for_session(session_id) {
            out.push_str(&format!(
                "- {} [{:?}] {}\n",
                event.created_at.format("%Y-%m-%d %H:%M:%S"),
                event.kind,
                event.message
            ));
        }
        let snapshots = self.snapshots_for_session(session_id);
        if !snapshots.is_empty() {
            out.push_str("\n## Blackbox Snapshots\n\n");
            for snapshot in snapshots {
                out.push_str(&format!(
                    "- {} frame={} {}x{} reason={}\n",
                    snapshot.captured_at.format("%Y-%m-%d %H:%M:%S"),
                    snapshot.frame_hash,
                    snapshot.width,
                    snapshot.height,
                    snapshot.reason
                ));
            }
        }
        out
    }

    fn export_evidence_json(&self, session_id: Uuid) -> String {
        serde_json::json!({
            "session_id": session_id,
            "events": self.events_for_session(session_id),
            "snapshots": self.snapshots_for_session(session_id),
        })
        .to_string()
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use chrono::Utc;

    use crate::models::DirtyRegion;

    #[test]
    fn evidence_json_contains_snapshots() {
        let mut store = InMemoryTimelineStore::default();
        let session_id = Uuid::new_v4();
        let frame = FrameUpdate {
            session_id,
            width: 100,
            height: 80,
            pixels_rgba: Vec::new(),
            dirty_regions: vec![DirtyRegion {
                left: 0,
                top: 0,
                right: 10,
                bottom: 10,
            }],
            frame_hash: 7,
            captured_at: Utc::now(),
        };

        store.append_snapshot("password=hidden".to_owned(), &frame);
        let json = store.export_evidence_json(session_id);
        assert!(json.contains("snapshots"));
        assert!(json.contains("[REDACTED]"));
    }
}
