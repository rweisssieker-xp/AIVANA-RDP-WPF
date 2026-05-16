use std::collections::HashMap;

use chrono::Utc;
use uuid::Uuid;

use crate::models::{HostMemory, WorkspaceMemory};
use crate::security::redact_secret_text;

#[derive(Default)]
pub struct MemoryStore {
    hosts: HashMap<String, HostMemory>,
    workspaces: HashMap<Uuid, WorkspaceMemory>,
}

impl MemoryStore {
    pub fn record_known_issue(&mut self, host: &str, workspace_id: Option<Uuid>, issue: &str) {
        self.host_memory_mut(host, workspace_id)
            .known_issues
            .push(redact_secret_text(issue));
    }

    pub fn record_successful_fix(&mut self, host: &str, workspace_id: Option<Uuid>, fix: &str) {
        self.host_memory_mut(host, workspace_id)
            .successful_fixes
            .push(redact_secret_text(fix));
    }

    pub fn record_runbook_result(&mut self, host: &str, workspace_id: Option<Uuid>, result: &str) {
        self.host_memory_mut(host, workspace_id)
            .runbook_results
            .push(redact_secret_text(result));
    }

    pub fn host_memory(&self, host: &str) -> Option<HostMemory> {
        self.hosts.get(&host.to_lowercase()).cloned()
    }

    pub fn recommended_next_step(&self, host: &str) -> String {
        match self.host_memory(host) {
            Some(memory) if !memory.successful_fixes.is_empty() => {
                format!(
                    "Letzten erfolgreichen Fix pruefen: {}",
                    memory.successful_fixes.last().unwrap()
                )
            }
            Some(memory) if !memory.known_issues.is_empty() => {
                format!(
                    "Known Issue pruefen: {}",
                    memory.known_issues.last().unwrap()
                )
            }
            _ => "Evidence Mode starten und sichere Diagnosebeweise sammeln.".to_owned(),
        }
    }

    pub fn workspace_memory_mut(&mut self, workspace_id: Uuid) -> &mut WorkspaceMemory {
        self.workspaces
            .entry(workspace_id)
            .or_insert_with(|| WorkspaceMemory {
                workspace_id,
                notes: Vec::new(),
                updated_at: Utc::now(),
            })
    }

    fn host_memory_mut(&mut self, host: &str, workspace_id: Option<Uuid>) -> &mut HostMemory {
        let now = Utc::now();
        self.hosts
            .entry(host.to_lowercase())
            .or_insert_with(|| HostMemory {
                id: Uuid::new_v4(),
                host: host.to_owned(),
                workspace_id,
                known_issues: Vec::new(),
                successful_fixes: Vec::new(),
                certificate_changes: Vec::new(),
                login_notes: Vec::new(),
                disconnect_patterns: Vec::new(),
                maintenance_notes: Vec::new(),
                runbook_results: Vec::new(),
                updated_at: now,
            })
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn memory_redacts_secrets() {
        let mut store = MemoryStore::default();
        store.record_known_issue("host", None, "password=secret failed");
        let memory = store.host_memory("host").unwrap();
        assert_eq!(memory.known_issues[0], "password=[REDACTED] failed");
    }
}
