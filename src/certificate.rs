use std::collections::HashMap;

use chrono::Utc;

use crate::models::{CertificateIdentity, CertificateTrustStatus};

#[derive(Default)]
pub struct CertificateTrustStore {
    identities: HashMap<String, CertificateIdentity>,
}

impl CertificateTrustStore {
    pub fn classify(&self, host: &str, port: u16, fingerprint: &str) -> CertificateTrustStatus {
        let key = key(host, port);
        match self.identities.get(&key) {
            None => CertificateTrustStatus::Unknown,
            Some(identity) if identity.status == CertificateTrustStatus::Rejected => {
                CertificateTrustStatus::Rejected
            }
            Some(identity) if identity.fingerprint == fingerprint => identity.status,
            Some(_) => CertificateTrustStatus::Changed,
        }
    }

    pub fn trust(&mut self, host: &str, port: u16, fingerprint: &str) -> CertificateIdentity {
        self.upsert(host, port, fingerprint, CertificateTrustStatus::Trusted)
    }

    pub fn reject(&mut self, host: &str, port: u16, fingerprint: &str) -> CertificateIdentity {
        self.upsert(host, port, fingerprint, CertificateTrustStatus::Rejected)
    }

    pub fn explain_risk(&self, status: CertificateTrustStatus) -> String {
        match status {
            CertificateTrustStatus::Unknown => {
                "Unbekanntes Zertifikat: bei neuen Hosts normal, bei bestehenden Hosts pruefen."
                    .to_owned()
            }
            CertificateTrustStatus::Trusted => "Zertifikat ist lokal vertraut.".to_owned(),
            CertificateTrustStatus::Rejected => {
                "Zertifikat wurde abgelehnt; Verbindung sollte blockiert bleiben.".to_owned()
            }
            CertificateTrustStatus::Changed => {
                "Zertifikat hat sich geaendert; moeglicher Reinstall oder MITM-Risiko.".to_owned()
            }
        }
    }

    fn upsert(
        &mut self,
        host: &str,
        port: u16,
        fingerprint: &str,
        status: CertificateTrustStatus,
    ) -> CertificateIdentity {
        let now = Utc::now();
        let key = key(host, port);
        let first_seen_at = self
            .identities
            .get(&key)
            .map(|identity| identity.first_seen_at)
            .unwrap_or(now);
        let identity = CertificateIdentity {
            host: host.to_owned(),
            port,
            fingerprint: fingerprint.to_owned(),
            first_seen_at,
            last_seen_at: now,
            status,
        };
        self.identities.insert(key, identity.clone());
        identity
    }
}

pub fn synthetic_fingerprint(host: &str, port: u16) -> String {
    let mut hash = 0xcbf29ce484222325u64;
    for byte in format!("{host}:{port}").bytes() {
        hash ^= u64::from(byte);
        hash = hash.wrapping_mul(0x100000001b3);
    }
    format!("aivana-local-{hash:016x}")
}

fn key(host: &str, port: u16) -> String {
    format!("{}:{}", host.to_lowercase(), port)
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn detects_changed_certificate() {
        let mut store = CertificateTrustStore::default();
        store.trust("host", 3389, "one");
        assert_eq!(
            store.classify("host", 3389, "two"),
            CertificateTrustStatus::Changed
        );
    }
}
