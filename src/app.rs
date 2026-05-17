use std::collections::HashMap;

use chrono::Utc;
use eframe::egui::{
    self, Align, Color32, ColorImage, Context, CornerRadius, FontFamily, FontId, Frame, Layout,
    Margin, Pos2, Rect, RichText, ScrollArea, Sense, Stroke, StrokeKind, TextStyle, TextureHandle,
    TextureOptions, Ui, UiBuilder, Vec2, pos2,
};
use uuid::Uuid;

use crate::ai::{AiProvider, LocalAiProvider};
use crate::certificate::CertificateTrustStore;
use crate::computer_use::ComputerUseAgent;
use crate::diagnostics::{LocalPreflightService, PreflightService};
use crate::ironrdp_client::probe_server_fingerprint;
use crate::memory::MemoryStore;
use crate::models::{
    AiAction, AiExplanation, ApprovalRequest, ApprovalStatus, ConnectionProfile, DiagnosticFinding,
    DiagnosticSeverity, DraftProfile, EngineEvent, FrameUpdate, InputAction, MouseButton,
    PolicyDecision, Protocol, RemoteSession, RiskLevel, SecretCredential, SessionEventKind,
    SessionStatus,
};
use crate::runbook::RunbookEngine;
use crate::security::{CredentialStore, PersistentCredentialStore, app_data_file};
use crate::services::{NativeRdpEngine, ProfileStore, RemoteDesktopEngine};
use crate::timeline::{InMemoryTimelineStore, TimelineStore};
use crate::workspace::WorkspaceStore;

mod tw {
    use eframe::egui::Color32;

    pub const WHITE: Color32 = Color32::from_rgb(255, 255, 255);
    pub const SLATE_50: Color32 = Color32::from_rgb(248, 250, 252);
    pub const SLATE_100: Color32 = Color32::from_rgb(241, 245, 249);
    pub const SLATE_200: Color32 = Color32::from_rgb(226, 232, 240);
    pub const SLATE_300: Color32 = Color32::from_rgb(203, 213, 225);
    pub const SLATE_400: Color32 = Color32::from_rgb(148, 163, 184);
    pub const SLATE_600: Color32 = Color32::from_rgb(71, 85, 105);
    pub const SLATE_700: Color32 = Color32::from_rgb(51, 65, 85);
    pub const SLATE_800: Color32 = Color32::from_rgb(30, 41, 59);
    pub const SLATE_900: Color32 = Color32::from_rgb(15, 23, 42);
    pub const SLATE_950: Color32 = Color32::from_rgb(2, 6, 23);
    pub const BLUE_50: Color32 = Color32::from_rgb(239, 246, 255);
    pub const BLUE_100: Color32 = Color32::from_rgb(219, 234, 254);
    pub const BLUE_500: Color32 = Color32::from_rgb(59, 130, 246);
    pub const BLUE_600: Color32 = Color32::from_rgb(37, 99, 235);
    pub const BLUE_700: Color32 = Color32::from_rgb(29, 78, 216);
    pub const SKY_300: Color32 = Color32::from_rgb(125, 211, 252);
    pub const RED_600: Color32 = Color32::from_rgb(220, 38, 38);
    pub const RED_700: Color32 = Color32::from_rgb(185, 28, 28);
}

#[derive(Clone, Copy, PartialEq, Eq)]
enum View {
    Connections,
    Sessions,
    Approvals,
    Workspaces,
    Settings,
}

pub struct AivanaApp {
    profiles: Vec<ConnectionProfile>,
    sessions: Vec<RemoteSession>,
    selected_profile: Option<Uuid>,
    selected_session: Option<Uuid>,
    view: View,
    search: String,
    draft: DraftProfile,
    editing_profile: Option<Uuid>,
    status: String,
    store: Option<ProfileStore>,
    engine: NativeRdpEngine,
    textures: HashMap<Uuid, TextureHandle>,
    latest_frames: HashMap<Uuid, FrameUpdate>,
    credentials: PersistentCredentialStore,
    preflight: LocalPreflightService,
    ai: LocalAiProvider,
    timeline: InMemoryTimelineStore,
    workspaces: WorkspaceStore,
    certificates: CertificateTrustStore,
    runbooks: RunbookEngine,
    memory: MemoryStore,
    approvals: Vec<ApprovalRequest>,
    active_runbook_execution: Option<Uuid>,
    diagnostics: Vec<DiagnosticFinding>,
    ai_diagnosis: String,
    computer_use_status: String,
    certificate_notice: String,
    session_inspector_open: bool,
}

impl AivanaApp {
    pub fn new(cc: &eframe::CreationContext<'_>) -> Self {
        configure_style(&cc.egui_ctx);

        let (store, profiles, status) = match ProfileStore::new() {
            Ok(store) => match store.load() {
                Ok(profiles) => (Some(store), profiles, "Profiles loaded".to_owned()),
                Err(err) => (
                    Some(store),
                    Vec::new(),
                    format!("Could not load profiles: {err}"),
                ),
            },
            Err(err) => (
                None,
                Vec::new(),
                format!("Profile store unavailable: {err}"),
            ),
        };

        let selected_profile = profiles.first().map(|profile| profile.id);
        let initial_draft =
            profiles
                .first()
                .map(DraftProfile::from)
                .unwrap_or_else(|| DraftProfile {
                    port: Protocol::Rdp.default_port().to_string(),
                    group: "Default".to_owned(),
                    ..Default::default()
                });
        let mut workspaces = WorkspaceStore::new().unwrap_or_default();
        workspaces.ensure_default();

        let credentials = PersistentCredentialStore::new().unwrap_or_else(|_| {
            PersistentCredentialStore::at(
                std::env::temp_dir().join("aivana-credentials-fallback.json"),
            )
            .expect("fallback credential store")
        });
        let certificates = CertificateTrustStore::new().unwrap_or_default();
        let timeline = InMemoryTimelineStore::new().unwrap_or_default();
        let memory = MemoryStore::new().unwrap_or_default();

        Self {
            profiles,
            sessions: Vec::new(),
            selected_profile,
            selected_session: None,
            view: View::Connections,
            search: String::new(),
            draft: initial_draft,
            editing_profile: selected_profile,
            status,
            store,
            engine: NativeRdpEngine::default(),
            textures: HashMap::new(),
            latest_frames: HashMap::new(),
            credentials,
            preflight: LocalPreflightService,
            ai: LocalAiProvider,
            timeline,
            workspaces,
            certificates,
            runbooks: RunbookEngine::with_defaults(),
            memory,
            approvals: Vec::new(),
            active_runbook_execution: None,
            diagnostics: Vec::new(),
            ai_diagnosis: "Lokale KI-Diagnose wartet auf Preflight oder Sessiondaten.".to_owned(),
            computer_use_status: "Computer Use wartet auf einen Framebuffer.".to_owned(),
            certificate_notice: "Certificate Trust wartet auf einen Host.".to_owned(),
            session_inspector_open: false,
        }
    }

    fn filtered_profiles(&self) -> Vec<&ConnectionProfile> {
        let needle = self.search.trim().to_lowercase();
        self.profiles
            .iter()
            .filter(|profile| {
                needle.is_empty()
                    || profile.name.to_lowercase().contains(&needle)
                    || profile.host.to_lowercase().contains(&needle)
                    || profile.group.to_lowercase().contains(&needle)
                    || profile
                        .tags
                        .iter()
                        .any(|tag| tag.to_lowercase().contains(&needle))
            })
            .collect()
    }

    fn selected_profile(&self) -> Option<&ConnectionProfile> {
        let id = self.selected_profile?;
        self.profiles.iter().find(|profile| profile.id == id)
    }

    fn selected_session(&self) -> Option<&RemoteSession> {
        let id = self.selected_session?;
        self.sessions.iter().find(|session| session.id == id)
    }

    fn save_profiles(&mut self) {
        if let Some(store) = &self.store {
            match store.save(&self.profiles) {
                Ok(()) => self.status = "Profiles saved".to_owned(),
                Err(err) => self.status = format!("Could not save profiles: {err}"),
            }
        }
    }

    fn start_new_profile(&mut self) {
        self.editing_profile = None;
        self.draft = DraftProfile {
            port: Protocol::Rdp.default_port().to_string(),
            group: "Default".to_owned(),
            ..Default::default()
        };
        self.status = "Ready for new profile".to_owned();
    }

    fn edit_selected_profile(&mut self) {
        if let Some(profile_id) = self.selected_profile {
            self.load_profile_into_editor(profile_id);
        }
    }

    fn load_profile_into_editor(&mut self, profile_id: Uuid) {
        if let Some(profile) = self
            .profiles
            .iter()
            .find(|profile| profile.id == profile_id)
            .cloned()
        {
            self.selected_profile = Some(profile.id);
            self.editing_profile = Some(profile.id);
            self.draft = DraftProfile::from(&profile);
            self.status = format!("Editing {}", profile.name);
        }
    }

    fn save_draft(&mut self) {
        let port = self
            .draft
            .port
            .parse::<u16>()
            .unwrap_or_else(|_| Protocol::Rdp.default_port());
        let tags = self
            .draft
            .tags
            .split(',')
            .map(str::trim)
            .filter(|tag| !tag.is_empty())
            .map(str::to_owned)
            .collect::<Vec<_>>();

        if self.draft.name.trim().is_empty() || self.draft.host.trim().is_empty() {
            self.status = "Name and host are required".to_owned();
            return;
        }

        let draft_secret = (!self.draft.password.is_empty()).then(|| SecretCredential {
            username: self.draft.username.trim().to_owned(),
            password: self.draft.password.clone(),
            domain: self.draft.domain.trim().to_owned(),
        });

        if let Some(id) = self.editing_profile {
            if let Some(profile) = self.profiles.iter_mut().find(|profile| profile.id == id) {
                profile.name = self.draft.name.trim().to_owned();
                profile.host = self.draft.host.trim().to_owned();
                profile.port = port;
                profile.username = self.draft.username.trim().to_owned();
                profile.password.clear();
                profile.domain = self.draft.domain.trim().to_owned();
                profile.group = self.draft.group.trim().to_owned();
                profile.tags = tags;
                profile.favorite = self.draft.favorite;
                profile.updated_at = Utc::now();
                if let Some(secret) = draft_secret {
                    if let Err(err) = self.credentials.save(profile, secret) {
                        self.status = format!("Could not save credential: {err}");
                    }
                }
                self.selected_profile = Some(profile.id);
            }
        } else {
            let now = Utc::now();
            let mut profile = ConnectionProfile {
                id: Uuid::new_v4(),
                workspace_id: None,
                name: self.draft.name.trim().to_owned(),
                host: self.draft.host.trim().to_owned(),
                port,
                username: self.draft.username.trim().to_owned(),
                credential_id: None,
                password: String::new(),
                domain: self.draft.domain.trim().to_owned(),
                protocol: Protocol::Rdp,
                group: self.draft.group.trim().to_owned(),
                tags,
                favorite: self.draft.favorite,
                created_at: now,
                updated_at: now,
            };
            if let Some(secret) = draft_secret {
                if let Err(err) = self.credentials.save(&mut profile, secret) {
                    self.status = format!("Could not save credential: {err}");
                }
            }
            self.selected_profile = Some(profile.id);
            self.profiles.push(profile);
        }

        let saved_profile = self.selected_profile;
        self.save_profiles();
        if let Some(profile_id) = saved_profile {
            self.load_profile_into_editor(profile_id);
            self.status = "Profile saved".to_owned();
        } else {
            self.start_new_profile();
        }
    }

    fn connect_selected(&mut self) {
        let Some(mut profile) = self.selected_profile().cloned() else {
            self.status = "Select a profile first".to_owned();
            return;
        };

        let mut legacy_standard_rdp = false;
        let fingerprint = match probe_server_fingerprint(&profile) {
            Ok(fingerprint) => fingerprint,
            Err(err) if is_standard_rdp_security_error(&format!("{err:#}")) => {
                legacy_standard_rdp = true;
                "legacy-standard-rdp-no-tls-certificate".to_owned()
            }
            Err(err) => {
                self.status = format!("RDP certificate probe failed: {err}");
                self.certificate_notice =
                    "Kein TLS-Zertifikat pruefbar; Verbindung bleibt blockiert.".to_owned();
                self.diagnostics = vec![DiagnosticFinding {
                    class: crate::models::DiagnosticClass::Tls,
                    severity: DiagnosticSeverity::Error,
                    title: "TLS-Zertifikat nicht pruefbar".to_owned(),
                    detail: err.to_string(),
                    fix: "RDP-Server muss TLS/NLA anbieten oder ein unterstuetzter Backendpfad muss ergaenzt werden.".to_owned(),
                }];
                self.ai_diagnosis = "RDP ist erreichbar, aber der TLS-/Certificate-Probe ist fehlgeschlagen. Aivana blockiert den Connect, damit kein unsicherer Fallback als Trust-Entscheidung gespeichert wird.".to_owned();
                return;
            }
        };
        if legacy_standard_rdp {
            self.certificate_notice = format!(
                "{}:{} nutzt Standard RDP Security ohne TLS-Zertifikat. Aivana versucht den nativen Legacy-Backendpfad.",
                profile.host, profile.port
            );
        } else {
            let cert_status = self
                .certificates
                .classify(&profile.host, profile.port, &fingerprint);
            if !matches!(cert_status, crate::models::CertificateTrustStatus::Trusted) {
                self.certificate_notice = format!(
                    "{:?}: {} Fingerprint: {}",
                    cert_status,
                    self.certificates.explain_risk(cert_status),
                    fingerprint
                );
                self.status = "Certificate trust decision required before connect".to_owned();
                return;
            }
        }

        if let Some(credential_id) = profile.credential_id {
            if let Ok(Some(secret)) = self.credentials.get(credential_id) {
                profile.username = secret.username;
                profile.password = secret.password;
                profile.domain = secret.domain;
            }
        }

        let report = self.preflight.run(&profile);
        self.ai_diagnosis = format_ai_explanation(&self.ai.explain_failure(&report));
        self.diagnostics = report.findings.clone();

        match self.engine.connect(&profile) {
            Ok(session) => {
                self.timeline.append_event(
                    session.id,
                    profile.workspace_id,
                    SessionEventKind::ConnectionStage,
                    format!("Connecting to {}:{}", profile.host, profile.port),
                );
                self.memory.record_known_issue(
                    &profile.host,
                    profile.workspace_id,
                    "Session started; host evidence available in Aivana timeline.",
                );
                self.selected_session = Some(session.id);
                self.sessions.push(session);
                self.view = View::Sessions;
                self.status = if report.connect_recommended {
                    format!("Connecting to {}", profile.name)
                } else {
                    format!("Connecting with preflight warnings for {}", profile.name)
                };
            }
            Err(err) => self.status = format!("Connection failed: {err}"),
        }
    }

    fn trust_selected_certificate(&mut self) {
        let Some(profile) = self.selected_profile().cloned() else {
            self.status = "Select a profile first".to_owned();
            return;
        };
        let (fingerprint, source) = match probe_server_fingerprint(&profile) {
            Ok(fingerprint) => (fingerprint, "RDP TLS probe"),
            Err(err) if is_standard_rdp_security_error(&format!("{err:#}")) => {
                self.block_standard_rdp_security(&profile, &format!("{err:#}"));
                return;
            }
            Err(err) => {
                self.status = format!("Certificate probe failed: {err}");
                self.certificate_notice =
                    "Trust blockiert: kein echtes TLS-Zertifikat vom RDP-Server erhalten."
                        .to_owned();
                return;
            }
        };
        let identity = self
            .certificates
            .trust(&profile.host, profile.port, &fingerprint);
        self.certificate_notice = format!(
            "Trusted {}:{} fingerprint {} ({source})",
            identity.host, identity.port, identity.fingerprint
        );
        self.status = "Certificate trusted locally".to_owned();
    }

    fn reject_selected_certificate(&mut self) {
        let Some(profile) = self.selected_profile().cloned() else {
            self.status = "Select a profile first".to_owned();
            return;
        };
        let fingerprint = match probe_server_fingerprint(&profile) {
            Ok(fingerprint) => fingerprint,
            Err(err) if is_standard_rdp_security_error(&format!("{err:#}")) => {
                self.block_standard_rdp_security(&profile, &format!("{err:#}"));
                return;
            }
            Err(err) => {
                self.status = format!("Certificate probe failed: {err}");
                self.certificate_notice =
                    "Reject blockiert: kein echtes TLS-Zertifikat vom RDP-Server erhalten."
                        .to_owned();
                return;
            }
        };
        let identity = self
            .certificates
            .reject(&profile.host, profile.port, &fingerprint);
        self.certificate_notice = format!(
            "Rejected {}:{} fingerprint {}",
            identity.host, identity.port, identity.fingerprint
        );
        self.status = "Certificate rejected locally".to_owned();
    }

    fn block_standard_rdp_security(&mut self, profile: &ConnectionProfile, detail: &str) {
        self.status = format!("{} uses unsupported Standard RDP Security", profile.host);
        self.certificate_notice = format!(
            "{}:{} bietet kein TLS/NLA-Zertifikat an. Standard RDP Security wird vom nativen IronRDP-Backend nicht unterstuetzt.",
            profile.host, profile.port
        );
        self.diagnostics = vec![DiagnosticFinding {
            class: crate::models::DiagnosticClass::Protocol,
            severity: DiagnosticSeverity::Error,
            title: "Standard RDP Security nicht unterstuetzt".to_owned(),
            detail: detail.to_owned(),
            fix: "Auf dem Server TLS/NLA fuer RDP aktivieren oder einen zusaetzlichen Standard-RDP-Security-Backendpfad implementieren.".to_owned(),
        }];
        self.ai_diagnosis = format!(
            "Root Cause: {} ist per Netzwerk erreichbar, bietet aber nur altes Standard RDP Security an. Aivana nutzt kein mstsc und das aktuelle native IronRDP-Backend kann diesen Modus nicht oeffnen. Fix: TLS/NLA auf dem Host aktivieren oder Backend erweitern.",
            profile.host
        );
    }

    fn test_selected_credential(&mut self) {
        let Some(profile) = self.selected_profile().cloned() else {
            self.status = "Select a profile first".to_owned();
            return;
        };
        let Some(credential_id) = profile.credential_id else {
            self.status = "Profile has no saved credential".to_owned();
            return;
        };
        match self.credentials.test(credential_id) {
            Ok(health) => self.status = format!("Credential status: {health:?}"),
            Err(err) => self.status = format!("Credential test failed: {err}"),
        }
    }

    fn delete_selected_credential(&mut self) {
        let Some(profile_id) = self.selected_profile else {
            self.status = "Select a profile first".to_owned();
            return;
        };
        let Some(profile) = self
            .profiles
            .iter_mut()
            .find(|profile| profile.id == profile_id)
        else {
            self.status = "Profile not found".to_owned();
            return;
        };
        let Some(credential_id) = profile.credential_id.take() else {
            self.status = "Profile has no saved credential".to_owned();
            return;
        };
        match self.credentials.delete(credential_id) {
            Ok(()) => {
                self.save_profiles();
                self.status = "Credential deleted".to_owned();
            }
            Err(err) => self.status = format!("Credential delete failed: {err}"),
        }
    }

    fn disconnect_selected_session(&mut self) {
        let Some(session_id) = self.selected_session else {
            self.status = "No session selected".to_owned();
            return;
        };

        if self.engine.disconnect(session_id).is_ok() {
            self.sessions.retain(|session| session.id != session_id);
            self.textures.remove(&session_id);
            self.latest_frames.remove(&session_id);
            self.selected_session = self.sessions.last().map(|session| session.id);
            self.status = "Session disconnected".to_owned();
        }
    }

    fn reconnect_selected_session(&mut self) {
        let Some(session_id) = self.selected_session else {
            self.status = "No session selected".to_owned();
            return;
        };
        let Some(session) = self
            .sessions
            .iter_mut()
            .find(|session| session.id == session_id)
        else {
            self.status = "Selected session not found".to_owned();
            return;
        };
        match self.engine.reconnect(session) {
            Ok(()) => self.status = "Reconnect started".to_owned(),
            Err(err) => self.status = format!("Reconnect failed: {err}"),
        }
    }
}

impl eframe::App for AivanaApp {
    fn ui(&mut self, ui: &mut Ui, _frame: &mut eframe::Frame) {
        let ctx = ui.ctx().clone();

        for session in &mut self.sessions {
            self.engine.tick(session);
            for event in self.engine.poll_events(session.id) {
                let (kind, message) = timeline_message(&event);
                self.timeline
                    .append_event(session.id, None, kind, message.clone());
                if matches!(
                    event,
                    EngineEvent::Error { .. }
                        | EngineEvent::Disconnected { .. }
                        | EngineEvent::Diagnostic { .. }
                ) {
                    self.ai_diagnosis = message;
                }
            }
            if let Some(frame) = self.engine.poll_frame(session.id) {
                let image = ColorImage::from_rgba_unmultiplied(
                    [usize::from(frame.width), usize::from(frame.height)],
                    &frame.pixels_rgba,
                );
                self.textures
                    .entry(session.id)
                    .and_modify(|texture| texture.set(image.clone(), TextureOptions::LINEAR))
                    .or_insert_with(|| {
                        ctx.load_texture(
                            format!("rdp-frame-{}", session.id),
                            image,
                            TextureOptions::LINEAR,
                        )
                    });
                self.latest_frames.insert(session.id, frame);
            }
        }

        let root = ui.max_rect();
        ui.painter()
            .rect_filled(root, CornerRadius::ZERO, tw::SLATE_100);

        let compact_shell = root.width() < 920.0;
        let top_height = if compact_shell { 42.0 } else { 46.0 };
        let nav_width = if compact_shell { 0.0 } else { 188.0 };
        let nav_height = if compact_shell { 44.0 } else { 0.0 };
        let top_rect = Rect::from_min_max(root.min, pos2(root.max.x, root.min.y + top_height));
        let nav_rect = if compact_shell {
            Rect::from_min_max(
                pos2(root.min.x, top_rect.max.y),
                pos2(root.max.x, top_rect.max.y + nav_height),
            )
        } else {
            Rect::from_min_max(
                pos2(root.min.x, top_rect.max.y),
                pos2(root.min.x + nav_width, root.max.y),
            )
        };
        let content_rect = if compact_shell {
            Rect::from_min_max(pos2(root.min.x, nav_rect.max.y), root.max)
        } else {
            Rect::from_min_max(pos2(nav_rect.max.x, top_rect.max.y), root.max)
        };

        let mut top_ui = ui.new_child(
            UiBuilder::new()
                .max_rect(top_rect)
                .layout(Layout::left_to_right(Align::Center)),
        );
        Frame::NONE
            .fill(tw::SLATE_950)
            .inner_margin(Margin::symmetric(16, 0))
            .show(&mut top_ui, |ui| {
                ui.set_min_height(top_height);
                ui.label(
                    RichText::new("Aivana")
                        .font(FontId::proportional(22.0))
                        .color(Color32::WHITE),
                );
                ui.label(
                    RichText::new("Rust RDP Client")
                        .font(FontId::proportional(13.0))
                        .color(tw::SLATE_300),
                );
                ui.with_layout(Layout::right_to_left(Align::Center), |ui| {
                    ui.label(
                        RichText::new(&self.status)
                            .font(FontId::proportional(12.0))
                            .color(tw::SKY_300),
                    );
                });
            });

        let mut nav_ui = ui.new_child(UiBuilder::new().max_rect(nav_rect).layout(
            if compact_shell {
                Layout::left_to_right(Align::Center)
            } else {
                Layout::top_down(Align::Min)
            },
        ));
        Frame::NONE
            .fill(tw::SLATE_900)
            .inner_margin(if compact_shell {
                Margin::symmetric(8, 6)
            } else {
                Margin::symmetric(10, 12)
            })
            .show(&mut nav_ui, |ui| {
                if !compact_shell {
                    ui.set_min_width(nav_width - 20.0);
                }
                nav_button(ui, &mut self.view, View::Connections, "Connections");
                nav_button(ui, &mut self.view, View::Sessions, "Sessions");
                nav_button(ui, &mut self.view, View::Approvals, "Approvals");
                nav_button(ui, &mut self.view, View::Workspaces, "Workspaces");
                nav_button(ui, &mut self.view, View::Settings, "Settings");
            });

        let mut content_ui = ui.new_child(
            UiBuilder::new()
                .max_rect(content_rect)
                .layout(Layout::top_down(Align::Min)),
        );
        Frame::NONE
            .fill(tw::SLATE_100)
            .inner_margin(if self.view == View::Sessions {
                Margin::same(10)
            } else {
                Margin::same(24)
            })
            .show(&mut content_ui, |ui| {
                if self.view == View::Sessions {
                    self.sessions_view(ui);
                } else {
                    ScrollArea::vertical()
                        .auto_shrink([false, false])
                        .show(ui, |ui| match self.view {
                            View::Connections => self.connections_view(ui),
                            View::Sessions => unreachable!("sessions are rendered without scroll"),
                            View::Approvals => self.approvals_view(ui),
                            View::Workspaces => self.workspaces_view(ui),
                            View::Settings => self.settings_view(ui),
                        });
                }
            });

        ctx.request_repaint_after(std::time::Duration::from_millis(250));
    }
}

impl AivanaApp {
    fn connections_view(&mut self, ui: &mut Ui) {
        page_header(
            ui,
            "Connection Profiles",
            "Manage saved endpoints and launch sessions.",
        );

        ui.horizontal_wrapped(|ui| {
            ui.add_sized(
                [ui.available_width().min(360.0), 42.0],
                egui::TextEdit::singleline(&mut self.search)
                    .hint_text("Search name, host, group, tag"),
            );
            if action_button(ui, "New", 88.0, ActionTone::Neutral).clicked() {
                self.start_new_profile();
            }
            if action_button(ui, "Edit", 88.0, ActionTone::Neutral).clicked() {
                self.edit_selected_profile();
            }
            if action_button(ui, "Connect", 108.0, ActionTone::Primary).clicked() {
                self.connect_selected();
            }
            if action_button(ui, "Trust Cert", 122.0, ActionTone::Primary).clicked() {
                self.trust_selected_certificate();
            }
            if action_button(ui, "Reject Cert", 126.0, ActionTone::Danger).clicked() {
                self.reject_selected_certificate();
            }
            if action_button(ui, "Test Cred", 112.0, ActionTone::Neutral).clicked() {
                self.test_selected_credential();
            }
            if action_button(ui, "Delete Cred", 124.0, ActionTone::Danger).clicked() {
                self.delete_selected_credential();
            }
        });
        ui.label(RichText::new(&self.certificate_notice).color(tw::SLATE_600));
        ui.label(
            RichText::new(format!("Status: {}", self.status))
                .strong()
                .color(tw::SLATE_700),
        );

        ui.add_space(14.0);
        self.profile_editor(ui);
        ui.add_space(14.0);
        self.profile_table(ui);
    }

    fn profile_table(&mut self, ui: &mut Ui) {
        panel(ui, |ui| {
            ui.heading("Profiles");
            ui.add_space(8.0);

            let rows = self
                .filtered_profiles()
                .into_iter()
                .map(|profile| profile.id)
                .collect::<Vec<_>>();

            for profile_id in rows {
                let Some(profile) = self
                    .profiles
                    .iter()
                    .find(|profile| profile.id == profile_id)
                else {
                    continue;
                };
                let selected = self.selected_profile == Some(profile.id);
                let row_fill = if selected { tw::BLUE_50 } else { tw::WHITE };
                let stroke = if selected {
                    Stroke::new(1.0, tw::BLUE_500)
                } else {
                    Stroke::new(1.0, tw::SLATE_200)
                };
                let row_width = ui.available_width().max(260.0);
                let (rect, response) =
                    ui.allocate_exact_size(Vec2::new(row_width, 74.0), Sense::click());
                let painter = ui.painter_at(rect);
                painter.rect(
                    rect,
                    CornerRadius::same(8),
                    row_fill,
                    stroke,
                    StrokeKind::Outside,
                );

                let favorite = if profile.favorite { "* " } else { "" };
                painter.text(
                    pos2(rect.left() + 14.0, rect.top() + 14.0),
                    egui::Align2::LEFT_TOP,
                    format!("{favorite}{}", profile.name),
                    FontId::proportional(19.0),
                    tw::SLATE_900,
                );
                painter.text(
                    pos2(rect.left() + 14.0, rect.top() + 42.0),
                    egui::Align2::LEFT_TOP,
                    format!(
                        "{}:{}  |  {}  |  {}",
                        profile.host,
                        profile.port,
                        profile.protocol.label(),
                        profile.group
                    ),
                    FontId::proportional(15.0),
                    tw::SLATE_600,
                );
                if selected {
                    painter.text(
                        pos2(rect.right() - 14.0, rect.top() + 14.0),
                        egui::Align2::RIGHT_TOP,
                        "Selected",
                        FontId::proportional(14.0),
                        tw::BLUE_700,
                    );
                }
                if response.clicked() {
                    self.load_profile_into_editor(profile.id);
                }
                ui.add_space(6.0);
            }
        });
    }

    fn profile_editor(&mut self, ui: &mut Ui) {
        panel(ui, |ui| {
            ui.heading(if self.editing_profile.is_some() {
                "Edit Profile"
            } else {
                "New Profile"
            });
            ui.label(
                RichText::new(self.credential_status_label())
                    .strong()
                    .color(tw::SLATE_600),
            );
            ui.add_space(8.0);
            let draft = &mut self.draft;
            ui.columns(2, |columns| {
                text_field(&mut columns[0], "Name", &mut draft.name);
                text_field(&mut columns[1], "Host", &mut draft.host);
            });
            ui.columns(2, |columns| {
                text_field(&mut columns[0], "Port", &mut draft.port);
                text_field(&mut columns[1], "Username", &mut draft.username);
            });
            ui.columns(2, |columns| {
                text_field(&mut columns[0], "Domain", &mut draft.domain);
                text_field(&mut columns[1], "Group", &mut draft.group);
            });
            password_field(ui, "Password", &mut draft.password);
            text_field(ui, "Tags", &mut draft.tags);
            ui.checkbox(&mut draft.favorite, "Favorite");
            ui.add_space(12.0);
            if action_button(ui, "Save Profile", 132.0, ActionTone::Primary).clicked() {
                self.save_draft();
            }
        });
    }

    fn sessions_view(&mut self, ui: &mut Ui) {
        self.session_command_bar(ui);

        let available_width = ui.available_width();
        if available_width < 760.0 {
            self.session_tabs(ui);
            ui.add_space(6.0);
            self.session_workspace(ui);
            return;
        }

        ui.horizontal_top(|ui| {
            ui.scope(|ui| {
                ui.set_width((available_width * 0.15).clamp(190.0, 245.0));
                self.session_list_panel(ui);
            });

            ui.add_space(4.0);
            ui.scope(|ui| {
                let inspector_width = if self.session_inspector_open {
                    (available_width * 0.22).clamp(260.0, 360.0)
                } else {
                    0.0
                };
                ui.set_width((ui.available_width() - inspector_width - 6.0).max(360.0));
                self.session_workspace(ui);
            });

            if self.session_inspector_open {
                ui.add_space(4.0);
                ui.scope(|ui| {
                    ui.set_width((available_width * 0.22).clamp(260.0, 360.0));
                    self.session_inspector(ui);
                });
            }
        });
    }

    fn session_command_bar(&mut self, ui: &mut Ui) {
        Frame::new()
            .fill(tw::WHITE)
            .stroke(Stroke::new(1.0, tw::SLATE_200))
            .corner_radius(CornerRadius::same(6))
            .inner_margin(Margin::symmetric(8, 6))
            .show(ui, |ui| {
                ui.horizontal_wrapped(|ui| {
                    ui.label(
                        RichText::new("Remote")
                            .size(16.0)
                            .strong()
                            .color(tw::SLATE_900),
                    );
                    if ui.button("Disconnect").clicked() {
                        self.disconnect_selected_session();
                    }
                    if ui.button("Reconnect").clicked() {
                        self.reconnect_selected_session();
                    }
                    if ui
                        .selectable_label(self.session_inspector_open, "Inspector")
                        .clicked()
                    {
                        self.session_inspector_open = !self.session_inspector_open;
                    }
                    ui.label(
                        RichText::new(format!("{} running", self.sessions.len()))
                            .size(12.0)
                            .color(tw::SLATE_600),
                    );
                });
            });
        ui.add_space(6.0);
    }

    fn session_tabs(&mut self, ui: &mut Ui) {
        Frame::new()
            .fill(tw::WHITE)
            .stroke(Stroke::new(1.0, tw::SLATE_200))
            .corner_radius(CornerRadius::same(6))
            .inner_margin(Margin::symmetric(8, 6))
            .show(ui, |ui| {
                ui.horizontal_wrapped(|ui| {
                    let ids = self
                        .sessions
                        .iter()
                        .map(|session| session.id)
                        .collect::<Vec<_>>();
                    for id in ids {
                        let session = self
                            .sessions
                            .iter()
                            .find(|session| session.id == id)
                            .expect("session id must exist");
                        let selected = self.selected_session == Some(session.id);
                        if ui
                            .selectable_label(
                                selected,
                                format!("{}  {}", session.title, session.status.label()),
                            )
                            .clicked()
                        {
                            self.selected_session = Some(session.id);
                        }
                    }
                });
            });
    }

    fn session_list_panel(&mut self, ui: &mut Ui) {
        Frame::new()
            .fill(tw::WHITE)
            .stroke(Stroke::new(1.0, tw::SLATE_200))
            .corner_radius(CornerRadius::same(6))
            .inner_margin(Margin::same(10))
            .show(ui, |ui| {
                ui.set_min_height(ui.available_height().clamp(220.0, 900.0));
                ui.label(
                    RichText::new("Sessions")
                        .size(15.0)
                        .strong()
                        .color(tw::SLATE_700),
                );
                ui.add_space(6.0);
                let ids = self
                    .sessions
                    .iter()
                    .map(|session| session.id)
                    .collect::<Vec<_>>();
                for id in ids {
                    let session = self
                        .sessions
                        .iter()
                        .find(|session| session.id == id)
                        .expect("session id must exist");
                    let selected = self.selected_session == Some(session.id);
                    if ui
                        .selectable_label(
                            selected,
                            format!("{}  {}", session.title, session.status.label()),
                        )
                        .clicked()
                    {
                        self.selected_session = Some(session.id);
                    }
                }
            });
    }

    fn session_workspace(&mut self, ui: &mut Ui) {
        Frame::new()
            .fill(tw::SLATE_950)
            .stroke(Stroke::new(1.0, tw::SLATE_200))
            .corner_radius(CornerRadius::same(6))
            .inner_margin(Margin::same(6))
            .show(ui, |ui| {
                ui.set_min_height(ui.available_height().clamp(320.0, 1200.0));
                if let Some(session) = self.selected_session().cloned() {
                    let session_id = session.id;
                    ui.horizontal(|ui| {
                        ui.label(
                            RichText::new(&session.title)
                                .size(14.0)
                                .strong()
                                .color(tw::WHITE),
                        );
                        ui.add_space(8.0);
                        ui.label(
                            RichText::new(session.status.label())
                                .size(12.0)
                                .strong()
                                .color(match session.status {
                                    SessionStatus::Connected => tw::SKY_300,
                                    SessionStatus::Failed | SessionStatus::Disconnected => {
                                        tw::RED_700
                                    }
                                    _ => tw::SLATE_300,
                                }),
                        );
                        ui.with_layout(Layout::right_to_left(Align::Center), |ui| {
                            ui.label(
                                RichText::new(format!(
                                    "{}%  {:.0}ms  {:.0}fps",
                                    session.metrics.quality_score,
                                    session.metrics.latency_ms,
                                    session.metrics.frame_rate
                                ))
                                .size(12.0)
                                .color(tw::SLATE_300),
                            );
                        });
                    });
                    if let Some(error) = &session.last_error {
                        ui.colored_label(tw::RED_700, error);
                    }
                    ui.add_space(6.0);
                    self.remote_canvas(ui, session_id);
                } else {
                    ui.heading("No session selected");
                    ui.label("Connect to a profile to start a remote desktop session.");
                }
            });
    }

    fn session_inspector(&mut self, ui: &mut Ui) {
        Frame::new()
            .fill(tw::WHITE)
            .stroke(Stroke::new(1.0, tw::SLATE_200))
            .corner_radius(CornerRadius::same(6))
            .inner_margin(Margin::same(10))
            .show(ui, |ui| {
                ui.set_min_height(ui.available_height().clamp(320.0, 1200.0));
                ui.label(
                    RichText::new("Inspector")
                        .size(15.0)
                        .strong()
                        .color(tw::SLATE_800),
                );
                ui.add_space(8.0);
                if let Some(session) = self.selected_session() {
                    metric(ui, "Status", session.status.label().to_owned());
                    metric(ui, "Quality", format!("{}%", session.metrics.quality_score));
                    metric(
                        ui,
                        "Latency",
                        format!("{:.0} ms", session.metrics.latency_ms),
                    );
                    metric(
                        ui,
                        "Frame rate",
                        format!("{:.0} fps", session.metrics.frame_rate),
                    );
                    metric(
                        ui,
                        "Bandwidth",
                        format!("{:.0} Mbps", session.metrics.bandwidth_mbps),
                    );
                }
                ui.separator();
                ui.label(RichText::new("Diagnostics").strong().color(tw::SLATE_800));
                ui.label(
                    RichText::new(&self.ai_diagnosis)
                        .size(12.0)
                        .color(tw::SLATE_600),
                );
            });
    }

    fn approvals_view(&mut self, ui: &mut Ui) {
        page_header(
            ui,
            "Approval Center",
            "Review KI Computer Use actions before they can mutate remote state.",
        );
        panel(ui, |ui| {
            if self.approvals.is_empty() {
                ui.label("No pending KI approvals.");
                return;
            }

            let mut updated = Vec::new();
            for mut approval in self.approvals.drain(..) {
                ui.separator();
                ui.label(RichText::new(&approval.description).strong());
                ui.label(format!(
                    "Risk: {:?} | Status: {:?}",
                    approval.risk, approval.status
                ));
                ui.label(format!("Reason: {}", approval.reason));
                ui.label(format!("Expected: {}", approval.expected_result));
                ui.horizontal(|ui| {
                    if ui.button("Allow once").clicked() {
                        approval.status = ApprovalStatus::AllowedOnce;
                    }
                    if ui.button("Allow for Runbook").clicked() {
                        approval.status = ApprovalStatus::AllowedForRunbook;
                    }
                    if ui.button("Deny").clicked() {
                        approval.status = ApprovalStatus::Denied;
                    }
                    if ui.button("Abort Agent").clicked() {
                        approval.status = ApprovalStatus::Aborted;
                    }
                });
                if approval.status == ApprovalStatus::Pending {
                    updated.push(approval);
                } else if let Some(session_id) = approval.session_id {
                    if matches!(
                        approval.status,
                        ApprovalStatus::AllowedOnce | ApprovalStatus::AllowedForRunbook
                    ) {
                        match self.engine.send_input(session_id, approval.action.clone()) {
                            Ok(()) => {
                                if let Some(frame) = self.latest_frames.get(&session_id) {
                                    let verification = ComputerUseAgent::default().verify_step(
                                        session_id,
                                        frame.frame_hash,
                                        frame,
                                    );
                                    self.timeline.append_event(
                                        session_id,
                                        None,
                                        SessionEventKind::AiAction,
                                        format!(
                                            "Verification after approval: {}",
                                            verification.evidence
                                        ),
                                    );
                                }
                                self.computer_use_status =
                                    format!("Approved action executed: {}", approval.description);
                            }
                            Err(err) => {
                                self.computer_use_status =
                                    format!("Approved action could not execute: {err}");
                            }
                        }
                    }
                    self.timeline.append_event(
                        session_id,
                        None,
                        SessionEventKind::Approval,
                        format!("Approval {:?}: {}", approval.status, approval.description),
                    );
                }
            }
            self.approvals = updated;
        });
    }

    fn workspaces_view(&mut self, ui: &mut Ui) {
        page_header(
            ui,
            "Workspace Cockpit",
            "Customers, incidents, runbooks, host memory and evidence.",
        );
        let selected_host = self
            .selected_profile()
            .map(|profile| profile.host.clone())
            .unwrap_or_else(|| "no-host-selected".to_owned());
        panel(ui, |ui| {
            ui.heading("Dashboard");
            metric(ui, "Workspaces", self.workspaces.all().len().to_string());
            metric(ui, "Profiles", self.profiles.len().to_string());
            metric(ui, "Sessions", self.sessions.len().to_string());
            metric(ui, "Open approvals", self.approvals.len().to_string());
            ui.add_space(10.0);
            ui.label(format!("Selected host: {selected_host}"));
            ui.label(format!(
                "Recommended next step: {}",
                self.memory.recommended_next_step(&selected_host)
            ));
            if let Some(memory) = self.memory.host_memory(&selected_host) {
                ui.label("Known for this host:");
                for issue in memory.known_issues.iter().rev().take(4) {
                    ui.label(format!("- {issue}"));
                }
            }
            if ui.button("Record successful safe fix").clicked() {
                self.memory.record_successful_fix(
                    &selected_host,
                    None,
                    "Operator confirmed latest safe diagnostic workflow as useful.",
                );
                self.status = "Host memory updated".to_owned();
            }
            if let Some(workspace) = self.workspaces.all().first() {
                if self.memory.workspace_note_count(workspace.id) == 0 {
                    self.memory
                        .record_workspace_note(workspace.id, "Workspace cockpit initialized.");
                }
                ui.label(format!(
                    "Workspace memory notes: {}",
                    self.memory.workspace_note_count(workspace.id)
                ));
            }
            ui.add_space(10.0);
            ui.heading("Runbooks");
            for runbook in self.runbooks.list_runbooks().iter().take(9) {
                ui.label(format!("- {} [{:?}]", runbook.name, runbook.category));
            }
        });
    }

    fn settings_view(&mut self, ui: &mut Ui) {
        page_header(ui, "Settings", "Runtime and integration status.");
        panel(ui, |ui| {
            ui.heading("Rust migration status");
            ui.label("UI runtime: native eframe/egui");
            ui.label("Persistent profile store: JSON in user data directory");
            ui.label("RDP backend: native Rust IronRDP connector");
            ui.label("Computer Use: native framebuffer observation and RDP input policy gates");
            ui.label("Security: profile JSON omits passwords; credential backend boundary exists");
            ui.label(format!(
                "Workspaces prepared: {}",
                self.workspaces.all().len()
            ));
        });
    }

    fn ai_session_panel(&mut self, ui: &mut Ui, session_id: Uuid) {
        ui.heading("KI Diagnose & Computer Use");
        if !self.diagnostics.is_empty() {
            ui.label("Preflight findings:");
            for finding in &self.diagnostics {
                ui.label(format!("- {}: {}", finding.title, finding.fix));
            }
        }
        ui.label(format!("Lokale KI: {}", self.ai_diagnosis));
        ui.label(format!("Computer Use: {}", self.computer_use_status));

        ui.horizontal(|ui| {
            if ui.button("Why did this fail?").clicked() {
                let messages = self.session_messages(session_id);
                let report = self.ai.write_incident_report(&messages);
                self.ai_diagnosis = format!("{} | Root cause: {}", report.title, report.root_cause);
            }
            if ui.button("What changed?").clicked() {
                let messages = self.session_messages(session_id);
                let change = self.ai.compare_sessions(&messages, &[]);
                self.ai_diagnosis = format!("{} {:?}", change.headline, change.changes);
            }
            if ui.button("Safe next action").clicked() {
                if let Some(frame) = self.latest_frames.get(&session_id) {
                    let agent = ComputerUseAgent::default();
                    let observation = agent.observe(frame);
                    let action =
                        agent.plan_next_step(&observation, "Diagnose aktuellen UI-Zustand");
                    if let Ok(input_action) = agent.execute_step(&action) {
                        let _ = self.engine.send_input(session_id, input_action);
                    }
                    self.computer_use_status = format!(
                        "{} | decision: {:?} | risk: {:?}",
                        observation.summary, action.decision, action.risk
                    );
                    self.timeline
                        .append_snapshot("KI screen observation".to_owned(), frame);
                    self.timeline.append_event(
                        session_id,
                        None,
                        SessionEventKind::AiObservation,
                        self.computer_use_status.clone(),
                    );
                } else {
                    self.computer_use_status =
                        "Noch kein Framebuffer fuer Observation vorhanden.".to_owned();
                }
            }
        });

        ui.horizontal(|ui| {
            if ui.button("Plan admin action").clicked() {
                let action = AiAction {
                    id: Uuid::new_v4(),
                    session_id: Some(session_id),
                    description: "Open diagnostics tool through remote UI".to_owned(),
                    action: InputAction::Hotkey {
                        keys: vec!["Win".to_owned(), "R".to_owned()],
                    },
                    risk: RiskLevel::ElevatedRisk,
                    decision: PolicyDecision::RequireApproval,
                };
                let agent = ComputerUseAgent::default();
                let approval = agent.request_approval(
                    &action,
                    "Opening remote tooling changes UI state and needs operator approval."
                        .to_owned(),
                );
                self.approvals.push(approval);
                self.computer_use_status = "Approval request queued.".to_owned();
            }
            if ui.button("Runbook recommendation").clicked() {
                let context = self.session_messages(session_id).join("\n");
                let recommendation = self
                    .runbooks
                    .recommend(&context)
                    .unwrap_or_else(|| self.ai.recommend_runbook(&context));
                self.computer_use_status = format!(
                    "{} ({:.0}%): {}",
                    recommendation.name,
                    recommendation.confidence * 100.0,
                    recommendation.reason
                );
            }
            if ui.button("Evidence mode").clicked() {
                let Some((runbook_id, runbook_name)) = self
                    .runbooks
                    .list_runbooks()
                    .first()
                    .map(|runbook| (runbook.id, runbook.name.clone()))
                else {
                    self.computer_use_status = "No runbooks available.".to_owned();
                    return;
                };
                if let Some(execution) = self.runbooks.start(session_id, runbook_id) {
                    self.active_runbook_execution = Some(execution.id);
                    if let Some(step) = self.runbooks.next_step(execution.id) {
                        let _ = self.engine.send_input(session_id, step.action.clone());
                        self.timeline.append_event(
                            session_id,
                            None,
                            SessionEventKind::AiAction,
                            format!("Runbook {} step: {}", runbook_name, step.title),
                        );
                        self.memory.record_runbook_result(
                            &self.selected_host_label(),
                            None,
                            &format!("{} started", runbook_name),
                        );
                        self.computer_use_status = format!("Evidence mode: {}", step.title);
                    }
                }
            }
            if ui.button("Next runbook step").clicked() {
                let Some(execution_id) = self.active_runbook_execution else {
                    self.computer_use_status = "No active runbook execution.".to_owned();
                    return;
                };
                if let Some(step) = self.runbooks.next_step(execution_id) {
                    match self.engine.send_input(session_id, step.action.clone()) {
                        Ok(()) => {
                            self.computer_use_status =
                                format!("Runbook step executed: {}", step.title);
                            self.timeline.append_event(
                                session_id,
                                None,
                                SessionEventKind::AiAction,
                                format!("Runbook step executed: {}", step.title),
                            );
                        }
                        Err(err) => {
                            self.computer_use_status = format!("Runbook step failed: {err}");
                        }
                    }
                } else {
                    self.computer_use_status = "Runbook has no next step.".to_owned();
                    self.active_runbook_execution = None;
                }
            }
            if ui.button("Pause runbook").clicked() {
                if let Some(execution_id) = self.active_runbook_execution {
                    self.runbooks.pause(execution_id);
                    self.computer_use_status = "Runbook paused.".to_owned();
                }
            }
            if ui.button("Resume runbook").clicked() {
                if let Some(execution_id) = self.active_runbook_execution {
                    self.runbooks.resume(execution_id);
                    self.computer_use_status = "Runbook resumed.".to_owned();
                }
            }
            if ui.button("Abort runbook").clicked() {
                if let Some(execution_id) = self.active_runbook_execution.take() {
                    self.runbooks.abort(execution_id);
                    self.timeline.append_event(
                        session_id,
                        None,
                        SessionEventKind::AiAction,
                        "Runbook aborted by operator.".to_owned(),
                    );
                    self.computer_use_status = "Runbook aborted.".to_owned();
                }
            }
            if ui.button("Export Incident").clicked() {
                let report = self.timeline.export_incident_markdown(session_id);
                let evidence_json = self.timeline.export_evidence_json(session_id);
                let saved = save_incident_files(session_id, &report, &evidence_json)
                    .map(|path| format!(" saved to {}", path.display()))
                    .unwrap_or_else(|err| format!(" save failed: {err}"));
                let messages = self
                    .timeline
                    .events_for_session(session_id)
                    .into_iter()
                    .map(|event| event.message)
                    .collect::<Vec<_>>();
                self.ai_diagnosis = self.ai.summarize_session(&messages).headline;
                self.computer_use_status = format!(
                    "Incident markdown {} bytes, evidence JSON {} bytes;{}",
                    report.len(),
                    evidence_json.len(),
                    saved
                );
            }
            if ui.button("Ticket in 30 seconds").clicked() {
                let messages = self.session_messages(session_id);
                let ticket = self.ai.write_incident_report(&messages);
                self.computer_use_status = format!("Ticket: {}", ticket.customer_text);
            }
        });
    }

    fn session_messages(&self, session_id: Uuid) -> Vec<String> {
        self.timeline
            .events_for_session(session_id)
            .into_iter()
            .map(|event| event.message)
            .collect()
    }

    fn selected_host_label(&self) -> String {
        self.selected_profile()
            .map(|profile| profile.host.clone())
            .unwrap_or_else(|| "unknown-host".to_owned())
    }

    fn credential_status_label(&self) -> String {
        let Some(profile) = self.selected_profile() else {
            return "Credential: kein Profil ausgewaehlt".to_owned();
        };
        match profile.credential_id {
            Some(credential_id) if self.credentials.has_credential(credential_id) => {
                "Credential: gespeichert".to_owned()
            }
            Some(_) => "Credential: Referenz vorhanden, Secret fehlt".to_owned(),
            None => "Credential: fehlt, Passwort eintragen und Save Profile klicken".to_owned(),
        }
    }

    fn remote_canvas(&mut self, ui: &mut Ui, session_id: Uuid) {
        let width = ui.available_width().max(280.0);
        let height = ui.available_height().clamp(260.0, 1400.0);
        let desired_size = Vec2::new(width, height);
        let (rect, response) = ui.allocate_exact_size(desired_size, Sense::click_and_drag());
        let painter = ui.painter_at(rect);
        painter.rect(
            rect,
            CornerRadius::same(4),
            tw::SLATE_950,
            Stroke::new(1.0, tw::SLATE_800),
            StrokeKind::Outside,
        );

        if let (Some(texture), Some(frame)) = (
            self.textures.get(&session_id),
            self.latest_frames.get(&session_id),
        ) {
            let image_rect = fit_rect(
                rect.shrink(2.0),
                Vec2::new(f32::from(frame.width), f32::from(frame.height)),
            );
            let _ = self.engine.resize(
                session_id,
                image_rect.width().round().clamp(320.0, 3840.0) as u16,
                image_rect.height().round().clamp(200.0, 2160.0) as u16,
            );
            painter.image(
                texture.id(),
                image_rect,
                Rect::from_min_max(Pos2::ZERO, Pos2::new(1.0, 1.0)),
                Color32::WHITE,
            );

            if response.hovered() {
                if let Some(pointer_pos) = ui.ctx().pointer_latest_pos() {
                    if image_rect.contains(pointer_pos) {
                        let (x, y) = viewport_to_remote(pointer_pos, image_rect, frame);
                        let _ = self
                            .engine
                            .send_input(session_id, InputAction::MovePointer { x, y });
                    }
                }
            }

            if response.clicked_by(egui::PointerButton::Primary) {
                if let Some(pos) = response.interact_pointer_pos() {
                    let (x, y) = viewport_to_remote(pos, image_rect, frame);
                    let _ = self.engine.send_input(
                        session_id,
                        InputAction::Click {
                            x,
                            y,
                            button: MouseButton::Left,
                        },
                    );
                }
            }
            if response.clicked_by(egui::PointerButton::Secondary) {
                if let Some(pos) = response.interact_pointer_pos() {
                    let (x, y) = viewport_to_remote(pos, image_rect, frame);
                    let _ = self.engine.send_input(
                        session_id,
                        InputAction::Click {
                            x,
                            y,
                            button: MouseButton::Right,
                        },
                    );
                }
            }

            let scroll_delta = ui.input(|input| input.smooth_scroll_delta.y);
            if response.hovered() && scroll_delta.abs() > f32::EPSILON {
                if let Some(pos) = ui.ctx().pointer_latest_pos() {
                    let (x, y) = viewport_to_remote(pos, image_rect, frame);
                    let _ = self.engine.send_input(
                        session_id,
                        InputAction::Scroll {
                            x,
                            y,
                            delta: scroll_delta.clamp(i16::MIN as f32, i16::MAX as f32) as i16,
                        },
                    );
                }
            }

            let typed = ui.input(|input| {
                input
                    .events
                    .iter()
                    .filter_map(|event| match event {
                        egui::Event::Text(text) => Some(text.clone()),
                        _ => None,
                    })
                    .collect::<Vec<_>>()
                    .join("")
            });
            if response.has_focus() && !typed.is_empty() {
                let _ = self
                    .engine
                    .send_input(session_id, InputAction::TypeText { text: typed });
            }
            let hotkeys = ui.input(|input| {
                input
                    .events
                    .iter()
                    .filter_map(|event| match event {
                        egui::Event::Key {
                            key,
                            pressed: true,
                            modifiers,
                            ..
                        } => key_event_to_hotkey(*key, *modifiers),
                        _ => None,
                    })
                    .collect::<Vec<_>>()
            });
            if response.has_focus() {
                for keys in hotkeys {
                    let _ = self
                        .engine
                        .send_input(session_id, InputAction::Hotkey { keys });
                }
            }
        } else {
            let message = self
                .sessions
                .iter()
                .find(|session| session.id == session_id)
                .map(|session| match session.status {
                    SessionStatus::Connected => {
                        "Connected - waiting for first remote desktop frame"
                    }
                    SessionStatus::Connecting | SessionStatus::Authenticating => {
                        "Connecting to native RDP session"
                    }
                    SessionStatus::Failed => "RDP session failed before a framebuffer arrived",
                    SessionStatus::Disconnected => "RDP session is disconnected",
                    SessionStatus::Reconnecting => "Reconnecting to native RDP session",
                    SessionStatus::Suspended => "RDP session is suspended",
                })
                .unwrap_or("Waiting for native RDP framebuffer");
            painter.text(
                rect.center(),
                egui::Align2::CENTER_CENTER,
                message,
                FontId::proportional(20.0),
                tw::SLATE_300,
            );
        }
    }
}

fn configure_style(ctx: &Context) {
    let mut style = (*ctx.global_style()).clone();
    style.visuals = egui::Visuals::light();
    style.visuals.widgets.inactive.corner_radius = CornerRadius::same(6);
    style.visuals.widgets.hovered.corner_radius = CornerRadius::same(6);
    style.visuals.widgets.active.corner_radius = CornerRadius::same(6);
    style.visuals.override_text_color = None;
    style.visuals.panel_fill = tw::SLATE_100;
    style.visuals.window_fill = tw::WHITE;
    style.visuals.extreme_bg_color = tw::WHITE;
    style.visuals.faint_bg_color = tw::SLATE_50;
    style.visuals.selection.bg_fill = tw::BLUE_600;
    style.visuals.selection.stroke = Stroke::new(1.0, tw::WHITE);
    style.visuals.widgets.noninteractive.fg_stroke = Stroke::new(1.0, tw::SLATE_800);
    style.visuals.widgets.inactive.fg_stroke = Stroke::new(1.0, tw::SLATE_800);
    style.visuals.widgets.hovered.fg_stroke = Stroke::new(1.0, tw::SLATE_950);
    style.visuals.widgets.active.fg_stroke = Stroke::new(1.0, tw::SLATE_950);
    style.visuals.widgets.inactive.bg_fill = tw::SLATE_50;
    style.visuals.widgets.hovered.bg_fill = tw::BLUE_50;
    style.visuals.widgets.active.bg_fill = tw::BLUE_100;
    style.text_styles.insert(
        TextStyle::Heading,
        FontId::new(30.0, FontFamily::Proportional),
    );
    style
        .text_styles
        .insert(TextStyle::Body, FontId::new(17.0, FontFamily::Proportional));
    style.text_styles.insert(
        TextStyle::Button,
        FontId::new(16.0, FontFamily::Proportional),
    );
    style.text_styles.insert(
        TextStyle::Small,
        FontId::new(14.0, FontFamily::Proportional),
    );
    style.spacing.item_spacing = Vec2::new(10.0, 8.0);
    style.spacing.button_padding = Vec2::new(14.0, 9.0);
    ctx.set_global_style(style);
}

#[derive(Clone, Copy)]
enum ActionTone {
    Neutral,
    Primary,
    Danger,
}

fn action_button(ui: &mut Ui, label: &str, width: f32, tone: ActionTone) -> egui::Response {
    let (fill, text, stroke) = match tone {
        ActionTone::Neutral => (tw::WHITE, tw::SLATE_800, tw::SLATE_300),
        ActionTone::Primary => (tw::BLUE_600, tw::WHITE, tw::BLUE_700),
        ActionTone::Danger => (tw::RED_600, tw::WHITE, tw::RED_700),
    };

    ui.add_sized(
        [width, 42.0],
        egui::Button::new(RichText::new(label).color(text).strong())
            .fill(fill)
            .stroke(Stroke::new(1.0, stroke)),
    )
}

fn nav_button(ui: &mut Ui, view: &mut View, target: View, label: &str) {
    let selected = *view == target;
    let compact = ui.available_width() > 360.0 && ui.available_height() < 64.0;
    let width = if compact {
        (ui.available_width() / 5.4).clamp(82.0, 150.0)
    } else {
        ui.available_width().max(120.0)
    };
    let response = ui.add_sized(
        [width, if compact { 30.0 } else { 38.0 }],
        egui::Button::new(RichText::new(label).color(if selected {
            tw::WHITE
        } else {
            tw::SLATE_300
        }))
        .fill(if selected {
            tw::BLUE_600
        } else {
            tw::SLATE_800
        }),
    );
    if response.clicked() {
        *view = target;
    }
}

fn stat_block(ui: &mut Ui, label: &str, value: String) {
    Frame::new()
        .fill(tw::SLATE_800)
        .stroke(Stroke::new(1.0, tw::SLATE_700))
        .corner_radius(CornerRadius::same(8))
        .inner_margin(Margin::same(14))
        .show(ui, |ui| {
            ui.set_min_width(172.0);
            ui.label(RichText::new(label).color(tw::SLATE_400).size(12.0));
            ui.label(
                RichText::new(value)
                    .color(tw::WHITE)
                    .font(FontId::proportional(30.0)),
            );
        });
}

fn page_header(ui: &mut Ui, title: &str, subtitle: &str) {
    ui.add_space(8.0);
    ui.heading(RichText::new(title).size(34.0).color(tw::SLATE_900));
    ui.label(RichText::new(subtitle).size(18.0).color(tw::SLATE_600));
    ui.add_space(18.0);
}

fn panel(ui: &mut Ui, add_contents: impl FnOnce(&mut Ui)) {
    Frame::new()
        .fill(tw::WHITE)
        .stroke(Stroke::new(1.0, tw::SLATE_200))
        .corner_radius(CornerRadius::same(8))
        .inner_margin(Margin::same(16))
        .show(ui, add_contents);
}

fn text_field(ui: &mut Ui, label: &str, value: &mut String) {
    ui.label(RichText::new(label).strong().color(tw::SLATE_700));
    input_frame(ui, |ui| {
        ui.add_sized(
            [ui.available_width().max(220.0), 28.0],
            egui::TextEdit::singleline(value)
                .frame(Frame::NONE)
                .desired_width(f32::INFINITY),
        );
    });
}

fn password_field(ui: &mut Ui, label: &str, value: &mut String) {
    ui.label(RichText::new(label).strong().color(tw::SLATE_700));
    input_frame(ui, |ui| {
        ui.add_sized(
            [ui.available_width().max(220.0), 28.0],
            egui::TextEdit::singleline(value)
                .password(true)
                .frame(Frame::NONE)
                .desired_width(f32::INFINITY),
        );
    });
}

fn input_frame(ui: &mut Ui, add_contents: impl FnOnce(&mut Ui)) {
    Frame::new()
        .fill(tw::SLATE_50)
        .stroke(Stroke::new(1.0, tw::SLATE_300))
        .corner_radius(CornerRadius::same(6))
        .inner_margin(Margin::symmetric(10, 5))
        .show(ui, add_contents);
}

fn metric(ui: &mut Ui, label: &str, value: String) {
    ui.horizontal(|ui| {
        ui.label(RichText::new(label).color(tw::SLATE_600));
        ui.with_layout(Layout::right_to_left(Align::Center), |ui| {
            ui.label(RichText::new(value).strong());
        });
    });
}

fn compact_metric(ui: &mut Ui, label: &str, value: String) {
    Frame::new()
        .fill(tw::SLATE_50)
        .stroke(Stroke::new(1.0, tw::SLATE_200))
        .corner_radius(CornerRadius::same(6))
        .inner_margin(Margin::symmetric(10, 6))
        .show(ui, |ui| {
            ui.set_min_width(118.0);
            ui.label(RichText::new(label).size(12.0).color(tw::SLATE_600));
            ui.label(RichText::new(value).strong().color(tw::SLATE_900));
        });
}

fn fit_rect(bounds: Rect, image_size: Vec2) -> Rect {
    let scale = (bounds.width() / image_size.x).min(bounds.height() / image_size.y);
    let size = image_size * scale;
    Rect::from_center_size(bounds.center(), size)
}

fn viewport_to_remote(pos: Pos2, image_rect: Rect, frame: &FrameUpdate) -> (u16, u16) {
    let x = ((pos.x - image_rect.left()) / image_rect.width() * f32::from(frame.width))
        .clamp(0.0, f32::from(frame.width.saturating_sub(1))) as u16;
    let y = ((pos.y - image_rect.top()) / image_rect.height() * f32::from(frame.height))
        .clamp(0.0, f32::from(frame.height.saturating_sub(1))) as u16;
    (x, y)
}

fn timeline_message(event: &EngineEvent) -> (SessionEventKind, String) {
    match event {
        EngineEvent::StatusChanged { status, .. } => (
            SessionEventKind::ConnectionStage,
            format!("Session status changed to {}", status.label()),
        ),
        EngineEvent::Frame(frame) => (
            SessionEventKind::Screenshot,
            format!(
                "Framebuffer {}x{} hash={} dirty_regions={}",
                frame.width,
                frame.height,
                frame.frame_hash,
                frame.dirty_regions.len()
            ),
        ),
        EngineEvent::Error { class, message, .. } => (
            SessionEventKind::Error,
            format!("RDP error {:?}: {}", class, message),
        ),
        EngineEvent::Diagnostic { message, .. } => (
            SessionEventKind::Diagnostic,
            format!("RDP diagnostic: {message}"),
        ),
        EngineEvent::Disconnected { reason, .. } => (
            SessionEventKind::ConnectionStage,
            format!("Disconnected: {reason}"),
        ),
    }
}

fn format_ai_explanation(explanation: &AiExplanation) -> String {
    format!(
        "{}: {} Next: {} Confidence: {:.0}%",
        explanation.title,
        explanation.likely_root_cause,
        explanation.next_safe_step,
        explanation.confidence * 100.0
    )
}

fn save_incident_files(
    session_id: Uuid,
    markdown: &str,
    evidence_json: &str,
) -> anyhow::Result<std::path::PathBuf> {
    let dir = app_data_file("incidents")?.join(session_id.to_string());
    std::fs::create_dir_all(&dir)?;
    std::fs::write(dir.join("incident.md"), markdown)?;
    std::fs::write(dir.join("evidence.json"), evidence_json)?;
    Ok(dir)
}

fn is_standard_rdp_security_error(message: &str) -> bool {
    let lower = message.to_lowercase();
    lower.contains("standard rdp security") || lower.contains("server only supports standard rdp")
}

fn key_event_to_hotkey(key: egui::Key, modifiers: egui::Modifiers) -> Option<Vec<String>> {
    let mut keys = Vec::new();
    if modifiers.ctrl {
        keys.push("Ctrl".to_owned());
    }
    if modifiers.alt {
        keys.push("Alt".to_owned());
    }
    if modifiers.shift {
        keys.push("Shift".to_owned());
    }
    if modifiers.mac_cmd || modifiers.command {
        keys.push("Win".to_owned());
    }

    let key_name = match key {
        egui::Key::Enter => "Enter",
        egui::Key::Tab => "Tab",
        egui::Key::Escape => "Esc",
        egui::Key::Backspace => return None,
        egui::Key::Delete => "Delete",
        egui::Key::Home => "Home",
        egui::Key::End => "End",
        egui::Key::PageUp => "PageUp",
        egui::Key::PageDown => "PageDown",
        egui::Key::ArrowLeft => "Left",
        egui::Key::ArrowRight => "Right",
        egui::Key::ArrowUp => "Up",
        egui::Key::ArrowDown => "Down",
        _ => return None,
    };
    keys.push(key_name.to_owned());
    Some(keys)
}
