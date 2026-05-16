use std::collections::HashMap;

use chrono::Utc;
use eframe::egui::{
    self, Align, Color32, ColorImage, Context, CornerRadius, FontId, Frame, Layout, Margin, Pos2,
    Rect, RichText, ScrollArea, Sense, Stroke, StrokeKind, TextureHandle, TextureOptions, Ui,
    UiBuilder, Vec2, pos2,
};
use egui_extras::{Column, TableBuilder};
use uuid::Uuid;

use crate::ai::{AiProvider, LocalAiProvider};
use crate::certificate::{CertificateTrustStore, synthetic_fingerprint};
use crate::computer_use::ComputerUseAgent;
use crate::diagnostics::{LocalPreflightService, PreflightService};
use crate::memory::MemoryStore;
use crate::models::{
    AiAction, AiExplanation, ApprovalRequest, ApprovalStatus, ConnectionProfile, DiagnosticFinding,
    DraftProfile, EngineEvent, FrameUpdate, InputAction, MouseButton, PolicyDecision, Protocol,
    RemoteSession, RiskLevel, SecretCredential, SessionEventKind,
};
use crate::runbook::RunbookEngine;
use crate::security::{CredentialStore, LocalCredentialStore};
use crate::services::{NativeRdpEngine, ProfileStore, RemoteDesktopEngine};
use crate::timeline::{InMemoryTimelineStore, TimelineStore};
use crate::workspace::WorkspaceStore;

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
    credentials: LocalCredentialStore,
    preflight: LocalPreflightService,
    ai: LocalAiProvider,
    timeline: InMemoryTimelineStore,
    workspaces: WorkspaceStore,
    certificates: CertificateTrustStore,
    runbooks: RunbookEngine,
    memory: MemoryStore,
    approvals: Vec<ApprovalRequest>,
    diagnostics: Vec<DiagnosticFinding>,
    ai_diagnosis: String,
    computer_use_status: String,
    certificate_notice: String,
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
        let mut workspaces = WorkspaceStore::default();
        workspaces.ensure_default();

        Self {
            profiles,
            sessions: Vec::new(),
            selected_profile,
            selected_session: None,
            view: View::Connections,
            search: String::new(),
            draft: DraftProfile {
                port: Protocol::Rdp.default_port().to_string(),
                group: "Default".to_owned(),
                ..Default::default()
            },
            editing_profile: None,
            status,
            store,
            engine: NativeRdpEngine::default(),
            textures: HashMap::new(),
            latest_frames: HashMap::new(),
            credentials: LocalCredentialStore::default(),
            preflight: LocalPreflightService,
            ai: LocalAiProvider,
            timeline: InMemoryTimelineStore::default(),
            workspaces,
            certificates: CertificateTrustStore::default(),
            runbooks: RunbookEngine::with_defaults(),
            memory: MemoryStore::default(),
            approvals: Vec::new(),
            diagnostics: Vec::new(),
            ai_diagnosis: "Lokale KI-Diagnose wartet auf Preflight oder Sessiondaten.".to_owned(),
            computer_use_status: "Computer Use wartet auf einen Framebuffer.".to_owned(),
            certificate_notice: "Certificate Trust wartet auf einen Host.".to_owned(),
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
        if let Some(profile) = self.selected_profile().cloned() {
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

        self.save_profiles();
        self.start_new_profile();
    }

    fn connect_selected(&mut self) {
        let Some(mut profile) = self.selected_profile().cloned() else {
            self.status = "Select a profile first".to_owned();
            return;
        };

        let fingerprint = synthetic_fingerprint(&profile.host, profile.port);
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
        let fingerprint = synthetic_fingerprint(&profile.host, profile.port);
        let identity = self
            .certificates
            .trust(&profile.host, profile.port, &fingerprint);
        self.certificate_notice = format!(
            "Trusted {}:{} fingerprint {}",
            identity.host, identity.port, identity.fingerprint
        );
        self.status = "Certificate trusted locally".to_owned();
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
                    EngineEvent::Error { .. } | EngineEvent::Disconnected { .. }
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
            .rect_filled(root, CornerRadius::ZERO, Color32::from_rgb(239, 242, 246));

        let top_height = 66.0;
        let nav_width = 236.0;
        let top_rect = Rect::from_min_max(root.min, pos2(root.max.x, root.min.y + top_height));
        let nav_rect = Rect::from_min_max(
            pos2(root.min.x, top_rect.max.y),
            pos2(root.min.x + nav_width, root.max.y),
        );
        let content_rect = Rect::from_min_max(pos2(nav_rect.max.x, top_rect.max.y), root.max);

        let mut top_ui = ui.new_child(
            UiBuilder::new()
                .max_rect(top_rect)
                .layout(Layout::left_to_right(Align::Center)),
        );
        Frame::NONE
            .fill(Color32::from_rgb(15, 20, 29))
            .inner_margin(Margin::symmetric(22, 0))
            .show(&mut top_ui, |ui| {
                ui.set_min_height(top_height);
                ui.label(
                    RichText::new("Aivana")
                        .font(FontId::proportional(27.0))
                        .color(Color32::WHITE),
                );
                ui.label(
                    RichText::new("Rust RDP Client")
                        .font(FontId::proportional(15.0))
                        .color(Color32::from_rgb(176, 185, 201)),
                );
                ui.with_layout(Layout::right_to_left(Align::Center), |ui| {
                    ui.label(
                        RichText::new(&self.status)
                            .font(FontId::proportional(13.0))
                            .color(Color32::from_rgb(133, 206, 255)),
                    );
                });
            });

        let mut nav_ui = ui.new_child(
            UiBuilder::new()
                .max_rect(nav_rect)
                .layout(Layout::top_down(Align::Min)),
        );
        Frame::NONE
            .fill(Color32::from_rgb(24, 30, 41))
            .inner_margin(Margin::symmetric(18, 18))
            .show(&mut nav_ui, |ui| {
                ui.set_min_width(nav_width - 36.0);
                ui.add_space(4.0);
                nav_button(ui, &mut self.view, View::Connections, "Connections");
                nav_button(ui, &mut self.view, View::Sessions, "Sessions");
                nav_button(ui, &mut self.view, View::Approvals, "Approvals");
                nav_button(ui, &mut self.view, View::Workspaces, "Workspaces");
                nav_button(ui, &mut self.view, View::Settings, "Settings");
                ui.add_space(26.0);
                stat_block(ui, "Active sessions", self.sessions.len().to_string());
                ui.add_space(10.0);
                stat_block(ui, "Profiles", self.profiles.len().to_string());
                ui.add_space(10.0);
                stat_block(ui, "Approvals", self.approvals.len().to_string());
            });

        let mut content_ui = ui.new_child(
            UiBuilder::new()
                .max_rect(content_rect)
                .layout(Layout::top_down(Align::Min)),
        );
        Frame::NONE
            .fill(Color32::from_rgb(239, 242, 246))
            .inner_margin(Margin::same(24))
            .show(&mut content_ui, |ui| {
                ScrollArea::vertical()
                    .auto_shrink([false, false])
                    .show(ui, |ui| match self.view {
                        View::Connections => self.connections_view(ui),
                        View::Sessions => self.sessions_view(ui),
                        View::Approvals => self.approvals_view(ui),
                        View::Workspaces => self.workspaces_view(ui),
                        View::Settings => self.settings_view(ui),
                    });
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

        ui.horizontal(|ui| {
            ui.add_sized(
                [ui.available_width().min(420.0), 38.0],
                egui::TextEdit::singleline(&mut self.search)
                    .hint_text("Search name, host, group, tag"),
            );
            if ui
                .add_sized([82.0, 38.0], egui::Button::new("New"))
                .clicked()
            {
                self.start_new_profile();
            }
            if ui
                .add_sized([82.0, 38.0], egui::Button::new("Edit"))
                .clicked()
            {
                self.edit_selected_profile();
            }
            if ui
                .add_sized([104.0, 38.0], egui::Button::new("Connect"))
                .clicked()
            {
                self.connect_selected();
            }
            if ui
                .add_sized([128.0, 38.0], egui::Button::new("Trust Cert"))
                .clicked()
            {
                self.trust_selected_certificate();
            }
        });
        ui.label(RichText::new(&self.certificate_notice).color(Color32::from_rgb(93, 103, 119)));

        ui.add_space(14.0);
        let available_width = ui.available_width();
        if available_width < 860.0 {
            self.profile_table(ui);
            ui.add_space(14.0);
            self.profile_editor(ui);
        } else {
            ui.horizontal_top(|ui| {
                let editor_width = 350.0;
                ui.scope(|ui| {
                    ui.set_width((available_width - editor_width - 16.0).max(420.0));
                    self.profile_table(ui);
                });
                ui.add_space(6.0);
                ui.scope(|ui| {
                    ui.set_width(editor_width);
                    self.profile_editor(ui);
                });
            });
        }
    }

    fn profile_table(&mut self, ui: &mut Ui) {
        panel(ui, |ui| {
            ui.set_min_height(430.0);
            ui.heading("Profiles");
            ui.add_space(8.0);

            let rows = self
                .filtered_profiles()
                .into_iter()
                .map(|profile| profile.id)
                .collect::<Vec<_>>();

            TableBuilder::new(ui)
                .striped(true)
                .column(Column::auto())
                .column(Column::remainder())
                .column(Column::auto())
                .column(Column::auto())
                .column(Column::auto())
                .header(24.0, |mut header| {
                    header.col(|ui| {
                        ui.label("Fav");
                    });
                    header.col(|ui| {
                        ui.label("Name");
                    });
                    header.col(|ui| {
                        ui.label("Host");
                    });
                    header.col(|ui| {
                        ui.label("Protocol");
                    });
                    header.col(|ui| {
                        ui.label("Group");
                    });
                })
                .body(|body| {
                    body.rows(34.0, rows.len(), |mut row| {
                        let profile_id = rows[row.index()];
                        let profile = self
                            .profiles
                            .iter()
                            .find(|profile| profile.id == profile_id)
                            .expect("filtered id must exist");
                        let selected = self.selected_profile == Some(profile.id);

                        row.col(|ui| {
                            ui.label(if profile.favorite { "*" } else { "" });
                        });
                        row.col(|ui| {
                            if ui.selectable_label(selected, &profile.name).clicked() {
                                self.selected_profile = Some(profile.id);
                            }
                        });
                        row.col(|ui| {
                            ui.label(format!("{}:{}", profile.host, profile.port));
                        });
                        row.col(|ui| {
                            ui.label(profile.protocol.label());
                        });
                        row.col(|ui| {
                            ui.label(&profile.group);
                        });
                    });
                });
        });
    }

    fn profile_editor(&mut self, ui: &mut Ui) {
        panel(ui, |ui| {
            ui.set_min_height(430.0);
            ui.heading(if self.editing_profile.is_some() {
                "Edit Profile"
            } else {
                "New Profile"
            });
            ui.add_space(8.0);
            text_field(ui, "Name", &mut self.draft.name);
            text_field(ui, "Host", &mut self.draft.host);
            text_field(ui, "Port", &mut self.draft.port);
            text_field(ui, "Username", &mut self.draft.username);
            password_field(ui, "Password", &mut self.draft.password);
            text_field(ui, "Domain", &mut self.draft.domain);
            text_field(ui, "Group", &mut self.draft.group);
            text_field(ui, "Tags", &mut self.draft.tags);
            ui.checkbox(&mut self.draft.favorite, "Favorite");
            ui.add_space(12.0);
            if ui.button("Save Profile").clicked() {
                self.save_draft();
            }
        });
    }

    fn sessions_view(&mut self, ui: &mut Ui) {
        page_header(
            ui,
            "Remote Sessions",
            "Native Rust RDP handshake and session runtime.",
        );

        ui.horizontal(|ui| {
            if ui.button("Disconnect").clicked() {
                self.disconnect_selected_session();
            }
            if ui.button("Reconnect").clicked() {
                self.reconnect_selected_session();
            }
            ui.label(format!("{} running", self.sessions.len()));
        });

        ui.add_space(14.0);
        let available_width = ui.available_width();
        if available_width < 820.0 {
            self.session_list_panel(ui);
            ui.add_space(14.0);
            self.session_details_panel(ui);
        } else {
            ui.horizontal_top(|ui| {
                ui.scope(|ui| {
                    ui.set_width(330.0);
                    self.session_list_panel(ui);
                });

                ui.add_space(6.0);
                ui.scope(|ui| {
                    ui.set_width((available_width - 352.0).max(420.0));
                    self.session_details_panel(ui);
                });
            });
        }
    }

    fn session_list_panel(&mut self, ui: &mut Ui) {
        panel(ui, |ui| {
            ui.set_min_height(430.0);
            ui.heading("Sessions");
            ui.add_space(8.0);
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
                        format!("{}  -  {}", session.title, session.status.label()),
                    )
                    .clicked()
                {
                    self.selected_session = Some(session.id);
                }
            }
        });
    }

    fn session_details_panel(&mut self, ui: &mut Ui) {
        panel(ui, |ui| {
            ui.set_min_height(430.0);
            if let Some(session) = self.selected_session() {
                let session_id = session.id;
                ui.heading(&session.title);
                ui.label(format!("Profile: {}", session.profile_id));
                ui.label(format!(
                    "Connected: {}",
                    session.connected_at.format("%H:%M:%S")
                ));
                if let Some(error) = &session.last_error {
                    ui.colored_label(Color32::from_rgb(171, 58, 58), error);
                }
                ui.add_space(10.0);
                metric(ui, "Quality", format!("{}%", session.metrics.quality_score));
                metric(
                    ui,
                    "Latency",
                    format!("{:.0} ms", session.metrics.latency_ms),
                );
                metric(
                    ui,
                    "Bandwidth",
                    format!("{:.0} Mbps", session.metrics.bandwidth_mbps),
                );
                metric(
                    ui,
                    "Frame rate",
                    format!("{:.0} fps", session.metrics.frame_rate),
                );
                metric(
                    ui,
                    "Packet loss",
                    format!("{:.1}%", session.metrics.packet_loss_pct),
                );
                ui.add_space(16.0);
                self.remote_canvas(ui, session_id);
                ui.add_space(12.0);
                self.ai_session_panel(ui, session_id);
            } else {
                ui.heading("No session selected");
                ui.label("Connect to a profile to start a remote desktop session.");
            }
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
                let recommendation = self.ai.recommend_runbook(&context);
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
                    if let Some(step) = self.runbooks.next_step(execution.id) {
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
            if ui.button("Export Incident").clicked() {
                let report = self.timeline.export_incident_markdown(session_id);
                let evidence_json = self.timeline.export_evidence_json(session_id);
                let messages = self
                    .timeline
                    .events_for_session(session_id)
                    .into_iter()
                    .map(|event| event.message)
                    .collect::<Vec<_>>();
                self.ai_diagnosis = self.ai.summarize_session(&messages).headline;
                self.computer_use_status = format!(
                    "Incident markdown {} bytes, evidence JSON {} bytes",
                    report.len(),
                    evidence_json.len()
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

    fn remote_canvas(&mut self, ui: &mut Ui, session_id: Uuid) {
        let desired_size = Vec2::new(ui.available_width(), 360.0);
        let (rect, response) = ui.allocate_exact_size(desired_size, Sense::click_and_drag());
        let painter = ui.painter_at(rect);
        painter.rect(
            rect,
            CornerRadius::same(8),
            Color32::from_rgb(19, 24, 32),
            Stroke::new(1.0, Color32::from_rgb(69, 79, 96)),
            StrokeKind::Outside,
        );

        if let (Some(texture), Some(frame)) = (
            self.textures.get(&session_id),
            self.latest_frames.get(&session_id),
        ) {
            let image_rect = fit_rect(
                rect.shrink(8.0),
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
        } else {
            painter.text(
                rect.center(),
                egui::Align2::CENTER_CENTER,
                "Waiting for native RDP framebuffer",
                FontId::proportional(20.0),
                Color32::from_rgb(190, 202, 220),
            );
        }
    }
}

fn configure_style(ctx: &Context) {
    let mut style = (*ctx.global_style()).clone();
    style.visuals.widgets.inactive.corner_radius = CornerRadius::same(6);
    style.visuals.widgets.hovered.corner_radius = CornerRadius::same(6);
    style.visuals.widgets.active.corner_radius = CornerRadius::same(6);
    style.spacing.item_spacing = Vec2::new(10.0, 8.0);
    ctx.set_global_style(style);
}

fn nav_button(ui: &mut Ui, view: &mut View, target: View, label: &str) {
    let selected = *view == target;
    let response = ui.add_sized(
        [188.0, 38.0],
        egui::Button::new(RichText::new(label).color(if selected {
            Color32::WHITE
        } else {
            Color32::from_rgb(194, 202, 216)
        }))
        .fill(if selected {
            Color32::from_rgb(42, 105, 168)
        } else {
            Color32::from_rgb(25, 29, 37)
        }),
    );
    if response.clicked() {
        *view = target;
    }
}

fn stat_block(ui: &mut Ui, label: &str, value: String) {
    Frame::new()
        .fill(Color32::from_rgb(31, 39, 53))
        .stroke(Stroke::new(1.0, Color32::from_rgb(48, 60, 79)))
        .corner_radius(CornerRadius::same(8))
        .inner_margin(Margin::same(14))
        .show(ui, |ui| {
            ui.set_min_width(172.0);
            ui.label(
                RichText::new(label)
                    .color(Color32::from_rgb(156, 166, 184))
                    .size(12.0),
            );
            ui.label(
                RichText::new(value)
                    .color(Color32::WHITE)
                    .font(FontId::proportional(30.0)),
            );
        });
}

fn page_header(ui: &mut Ui, title: &str, subtitle: &str) {
    ui.add_space(18.0);
    ui.heading(
        RichText::new(title)
            .size(28.0)
            .color(Color32::from_rgb(22, 28, 38)),
    );
    ui.label(RichText::new(subtitle).color(Color32::from_rgb(87, 96, 111)));
    ui.add_space(18.0);
}

fn panel(ui: &mut Ui, add_contents: impl FnOnce(&mut Ui)) {
    Frame::new()
        .fill(Color32::WHITE)
        .stroke(Stroke::new(1.0, Color32::from_rgb(219, 225, 232)))
        .corner_radius(CornerRadius::same(8))
        .inner_margin(Margin::same(16))
        .show(ui, add_contents);
}

fn text_field(ui: &mut Ui, label: &str, value: &mut String) {
    ui.label(label);
    ui.add_sized(
        [ui.available_width().max(220.0), 32.0],
        egui::TextEdit::singleline(value),
    );
}

fn password_field(ui: &mut Ui, label: &str, value: &mut String) {
    ui.label(label);
    ui.add_sized(
        [ui.available_width().max(220.0), 32.0],
        egui::TextEdit::singleline(value).password(true),
    );
}

fn metric(ui: &mut Ui, label: &str, value: String) {
    ui.horizontal(|ui| {
        ui.label(RichText::new(label).color(Color32::from_rgb(93, 103, 119)));
        ui.with_layout(Layout::right_to_left(Align::Center), |ui| {
            ui.label(RichText::new(value).strong());
        });
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
