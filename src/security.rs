use std::collections::HashMap;

use anyhow::{Context, Result};
use chrono::Utc;
use uuid::Uuid;

use crate::models::{ConnectionProfile, CredentialRef, SecretCredential};

pub trait CredentialStore {
    fn save(
        &mut self,
        profile: &mut ConnectionProfile,
        secret: SecretCredential,
    ) -> Result<CredentialRef>;
    fn get(&self, credential_id: Uuid) -> Result<Option<SecretCredential>>;
    fn delete(&mut self, credential_id: Uuid) -> Result<()>;
    fn has_credential(&self, credential_id: Uuid) -> bool;
    fn credential_ref_for_profile(&self, profile_id: Uuid) -> Option<CredentialRef>;
    fn test(&self, credential_id: Uuid) -> Result<CredentialHealth>;
}

#[derive(Clone, Debug, PartialEq, Eq)]
pub enum CredentialHealth {
    Available,
    Missing,
    EmptyPassword,
}

#[derive(Default)]
pub struct LocalCredentialStore {
    refs: HashMap<Uuid, CredentialRef>,
    secrets: HashMap<Uuid, SecretCredential>,
}

impl CredentialStore for LocalCredentialStore {
    fn save(
        &mut self,
        profile: &mut ConnectionProfile,
        secret: SecretCredential,
    ) -> Result<CredentialRef> {
        let now = Utc::now();
        let id = profile.credential_id.unwrap_or_else(Uuid::new_v4);
        let credential_ref = CredentialRef {
            id,
            profile_id: profile.id,
            username: secret.username.clone(),
            label: format!("{}@{}", secret.username, profile.host),
            created_at: self.refs.get(&id).map(|r| r.created_at).unwrap_or(now),
            updated_at: now,
        };

        profile.credential_id = Some(id);
        profile.password.clear();
        self.refs.insert(id, credential_ref.clone());
        self.secrets.insert(id, secret);
        Ok(credential_ref)
    }

    fn get(&self, credential_id: Uuid) -> Result<Option<SecretCredential>> {
        Ok(self.secrets.get(&credential_id).cloned())
    }

    fn delete(&mut self, credential_id: Uuid) -> Result<()> {
        self.refs
            .remove(&credential_id)
            .context("credential ref missing")?;
        self.secrets.remove(&credential_id);
        Ok(())
    }

    fn has_credential(&self, credential_id: Uuid) -> bool {
        self.secrets.contains_key(&credential_id)
    }

    fn credential_ref_for_profile(&self, profile_id: Uuid) -> Option<CredentialRef> {
        self.refs
            .values()
            .find(|r| r.profile_id == profile_id)
            .cloned()
    }

    fn test(&self, credential_id: Uuid) -> Result<CredentialHealth> {
        match self.secrets.get(&credential_id) {
            None => Ok(CredentialHealth::Missing),
            Some(secret) if secret.password.is_empty() => Ok(CredentialHealth::EmptyPassword),
            Some(_) => Ok(CredentialHealth::Available),
        }
    }
}

pub fn redact_secret_text(input: &str) -> String {
    input
        .split_whitespace()
        .map(|part| {
            let lower = part.to_lowercase();
            for marker in ["password=", "pwd=", "token=", "secret="] {
                if lower.starts_with(marker) {
                    return format!("{marker}[REDACTED]");
                }
            }
            part.to_owned()
        })
        .collect::<Vec<_>>()
        .join(" ")
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn credential_save_clears_profile_password() {
        let mut store = LocalCredentialStore::default();
        let mut profile = ConnectionProfile::sample("Test", "host", "Default", false);
        profile.password = "secret".to_owned();

        let saved = store
            .save(
                &mut profile,
                SecretCredential {
                    username: "admin".to_owned(),
                    password: "secret".to_owned(),
                    domain: String::new(),
                },
            )
            .unwrap();

        assert_eq!(profile.credential_id, Some(saved.id));
        assert!(profile.password.is_empty());
        assert!(store.has_credential(saved.id));
        assert_eq!(store.test(saved.id).unwrap(), CredentialHealth::Available);
    }

    #[test]
    fn redaction_masks_common_secret_markers() {
        assert_eq!(
            redact_secret_text("password=hunter2 token=abc"),
            "password=[REDACTED] token=[REDACTED]"
        );
    }
}
