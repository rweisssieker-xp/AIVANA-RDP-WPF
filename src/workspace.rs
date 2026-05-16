use std::collections::HashMap;

use chrono::Utc;
use uuid::Uuid;

use crate::models::Workspace;

#[derive(Default)]
pub struct WorkspaceStore {
    workspaces: HashMap<Uuid, Workspace>,
}

impl WorkspaceStore {
    pub fn ensure_default(&mut self) -> Uuid {
        if let Some(id) = self.workspaces.keys().next().copied() {
            return id;
        }

        let now = Utc::now();
        let id = Uuid::new_v4();
        self.workspaces.insert(
            id,
            Workspace {
                id,
                name: "Default Workspace".to_owned(),
                notes: "Local-first Aivana workspace".to_owned(),
                created_at: now,
                updated_at: now,
            },
        );
        id
    }

    pub fn all(&self) -> Vec<Workspace> {
        self.workspaces.values().cloned().collect()
    }
}
