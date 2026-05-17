use std::net::{TcpStream, ToSocketAddrs};
use std::time::Duration;

use chrono::Utc;

use crate::models::{
    ConnectionProfile, DiagnosticClass, DiagnosticFinding, DiagnosticSeverity, PreflightReport,
};

pub trait PreflightService {
    fn run(&self, profile: &ConnectionProfile) -> PreflightReport;
}

#[derive(Default)]
pub struct LocalPreflightService;

impl PreflightService for LocalPreflightService {
    fn run(&self, profile: &ConnectionProfile) -> PreflightReport {
        let mut findings = Vec::new();

        if profile.host.trim().is_empty() {
            findings.push(finding(
                DiagnosticClass::Dns,
                DiagnosticSeverity::Error,
                "Host fehlt",
                "Das Profil hat keinen Hostnamen oder keine IP-Adresse.",
                "Host im Profil eintragen.",
            ));
        } else {
            match (profile.host.as_str(), profile.port).to_socket_addrs() {
                Ok(mut addrs) => {
                    if let Some(addr) = addrs.next() {
                        findings.push(finding(
                            DiagnosticClass::Dns,
                            DiagnosticSeverity::Info,
                            "DNS aufgeloest",
                            &format!("{} wurde zu {} aufgeloest.", profile.host, addr),
                            "Keine Aktion erforderlich.",
                        ));
                        if TcpStream::connect_timeout(&addr, Duration::from_secs(2)).is_err() {
                            findings.push(finding(
                                DiagnosticClass::Tcp,
                                DiagnosticSeverity::Error,
                                "TCP-Port nicht erreichbar",
                                "Der Zielhost antwortet nicht auf dem RDP-Port.",
                                "Firewall, VPN, Routing und RDP-Port pruefen.",
                            ));
                        }
                    } else {
                        findings.push(finding(
                            DiagnosticClass::Dns,
                            DiagnosticSeverity::Error,
                            "Keine Adresse gefunden",
                            "DNS lieferte keine nutzbare Zieladresse.",
                            "Hostnamen oder DNS-Konfiguration pruefen.",
                        ));
                    }
                }
                Err(err) => findings.push(finding(
                    DiagnosticClass::Dns,
                    DiagnosticSeverity::Error,
                    "DNS-Fehler",
                    &err.to_string(),
                    "Hostnamen, DNS-Server oder Netzwerk pruefen.",
                )),
            }
        }

        if profile.credential_id.is_none() && profile.password.is_empty() {
            findings.push(finding(
                DiagnosticClass::Credential,
                DiagnosticSeverity::Warning,
                "Keine gespeicherten Credentials",
                "Das Profil hat weder Credential-Referenz noch ein temporaeres Passwort.",
                "Credential speichern oder Passwort fuer die Session eingeben.",
            ));
        }

        let connect_recommended = !findings
            .iter()
            .any(|f| matches!(f.severity, DiagnosticSeverity::Error));

        PreflightReport {
            profile_id: profile.id,
            checked_at: Utc::now(),
            findings,
            connect_recommended,
        }
    }
}

pub fn classify_error(message: &str) -> DiagnosticClass {
    let lower = message.to_lowercase();
    if lower.contains("standard rdp security") || lower.contains("negotiation failure") {
        DiagnosticClass::Protocol
    } else if lower.contains("dns") || lower.contains("socket address") {
        DiagnosticClass::Dns
    } else if lower.contains("tcp") || lower.contains("connect") {
        DiagnosticClass::Tcp
    } else if lower.contains("tls") {
        DiagnosticClass::Tls
    } else if lower.contains("credssp") || lower.contains("nla") {
        DiagnosticClass::CredSspNla
    } else if lower.contains("auth") || lower.contains("password") {
        DiagnosticClass::Auth
    } else if lower.contains("certificate") || lower.contains("cert") {
        DiagnosticClass::Certificate
    } else if lower.contains("timeout") {
        DiagnosticClass::Timeout
    } else {
        DiagnosticClass::Unknown
    }
}

pub fn finding(
    class: DiagnosticClass,
    severity: DiagnosticSeverity,
    title: &str,
    detail: &str,
    fix: &str,
) -> DiagnosticFinding {
    DiagnosticFinding {
        class,
        severity,
        title: title.to_owned(),
        detail: detail.to_owned(),
        fix: fix.to_owned(),
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn classifies_common_error_text() {
        assert_eq!(classify_error("TLS upgrade failed"), DiagnosticClass::Tls);
        assert_eq!(
            classify_error("CredSSP failure"),
            DiagnosticClass::CredSspNla
        );
        assert_eq!(
            classify_error("negotiation failure: server only supports Standard RDP Security"),
            DiagnosticClass::Protocol
        );
    }
}
