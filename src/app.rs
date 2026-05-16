use chrono::Utc;
use eframe::egui::{
    self, Align, CentralPanel, Color32, Context, CornerRadius, FontId, Frame, Layout, Margin,
    Panel, RichText, Sense, Stroke, StrokeKind, Ui, Vec2,
};
use egui_extras::{Column, TableBuilder};
use uuid::Uuid;

use crate::models::{ConnectionProfile, DraftProfile, Protocol, RemoteSession};
use crate::services::{NativeRdpEngine, ProfileStore, RemoteDesktopEngine};

#[derive(Clone, Copy, PartialEq, Eq)]
enum View {
    Connections,
    Sessions,
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

        if let Some(id) = self.editing_profile {
            if let Some(profile) = self.profiles.iter_mut().find(|profile| profile.id == id) {
                profile.name = self.draft.name.trim().to_owned();
                profile.host = self.draft.host.trim().to_owned();
                profile.port = port;
                profile.username = self.draft.username.trim().to_owned();
                profile.domain = self.draft.domain.trim().to_owned();
                profile.group = self.draft.group.trim().to_owned();
                profile.tags = tags;
                profile.favorite = self.draft.favorite;
                profile.updated_at = Utc::now();
                self.selected_profile = Some(profile.id);
            }
        } else {
            let now = Utc::now();
            let profile = ConnectionProfile {
                id: Uuid::new_v4(),
                name: self.draft.name.trim().to_owned(),
                host: self.draft.host.trim().to_owned(),
                port,
                username: self.draft.username.trim().to_owned(),
                domain: self.draft.domain.trim().to_owned(),
                protocol: Protocol::Rdp,
                group: self.draft.group.trim().to_owned(),
                tags,
                favorite: self.draft.favorite,
                created_at: now,
                updated_at: now,
            };
            self.selected_profile = Some(profile.id);
            self.profiles.push(profile);
        }

        self.save_profiles();
        self.start_new_profile();
    }

    fn connect_selected(&mut self) {
        let Some(profile) = self.selected_profile().cloned() else {
            self.status = "Select a profile first".to_owned();
            return;
        };

        match self.engine.connect(&profile) {
            Ok(session) => {
                self.selected_session = Some(session.id);
                self.sessions.push(session);
                self.view = View::Sessions;
                self.status = format!("Connected to {}", profile.name);
            }
            Err(err) => self.status = format!("Connection failed: {err}"),
        }
    }

    fn disconnect_selected_session(&mut self) {
        let Some(session_id) = self.selected_session else {
            self.status = "No session selected".to_owned();
            return;
        };

        if self.engine.disconnect(session_id).is_ok() {
            self.sessions.retain(|session| session.id != session_id);
            self.selected_session = self.sessions.last().map(|session| session.id);
            self.status = "Session disconnected".to_owned();
        }
    }
}

impl eframe::App for AivanaApp {
    fn ui(&mut self, ui: &mut Ui, _frame: &mut eframe::Frame) {
        let ctx = ui.ctx().clone();

        for session in &mut self.sessions {
            self.engine.tick(session);
        }

        Panel::top("top_bar")
            .frame(Frame::NONE.fill(Color32::from_rgb(16, 18, 24)))
            .show_inside(ui, |ui| {
                ui.set_height(54.0);
                ui.horizontal_centered(|ui| {
                    ui.add_space(14.0);
                    ui.label(
                        RichText::new("Aivana")
                            .font(FontId::proportional(24.0))
                            .color(Color32::WHITE),
                    );
                    ui.label(
                        RichText::new("Rust RDP Client")
                            .font(FontId::proportional(15.0))
                            .color(Color32::from_rgb(176, 185, 201)),
                    );
                    ui.with_layout(Layout::right_to_left(Align::Center), |ui| {
                        ui.add_space(12.0);
                        ui.label(
                            RichText::new(&self.status)
                                .font(FontId::proportional(13.0))
                                .color(Color32::from_rgb(159, 207, 255)),
                        );
                    });
                });
            });

        Panel::left("nav")
            .exact_size(220.0)
            .frame(Frame::NONE.fill(Color32::from_rgb(25, 29, 37)))
            .show_inside(ui, |ui| {
                ui.add_space(12.0);
                nav_button(ui, &mut self.view, View::Connections, "Connections");
                nav_button(ui, &mut self.view, View::Sessions, "Sessions");
                nav_button(ui, &mut self.view, View::Settings, "Settings");
                ui.add_space(22.0);
                ui.label(
                    RichText::new("Active sessions")
                        .color(Color32::from_rgb(156, 166, 184))
                        .size(12.0),
                );
                ui.heading(
                    RichText::new(self.sessions.len().to_string())
                        .color(Color32::WHITE)
                        .size(34.0),
                );
            });

        CentralPanel::default()
            .frame(Frame::NONE.fill(Color32::from_rgb(239, 242, 246)))
            .show_inside(ui, |ui| match self.view {
                View::Connections => self.connections_view(ui),
                View::Sessions => self.sessions_view(ui),
                View::Settings => self.settings_view(ui),
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
                [340.0, 36.0],
                egui::TextEdit::singleline(&mut self.search)
                    .hint_text("Search name, host, group, tag"),
            );
            if ui.button("New").clicked() {
                self.start_new_profile();
            }
            if ui.button("Edit").clicked() {
                self.edit_selected_profile();
            }
            if ui.button("Connect").clicked() {
                self.connect_selected();
            }
        });

        ui.add_space(14.0);
        ui.columns(2, |columns| {
            self.profile_table(&mut columns[0]);
            self.profile_editor(&mut columns[1]);
        });
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
            "Native Rust shell with Windows RDP launch adapter.",
        );

        ui.horizontal(|ui| {
            if ui.button("Disconnect").clicked() {
                self.disconnect_selected_session();
            }
            ui.label(format!("{} running", self.sessions.len()));
        });

        ui.add_space(14.0);
        ui.columns(2, |columns| {
            panel(&mut columns[0], |ui| {
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

            panel(&mut columns[1], |ui| {
                if let Some(session) = self.selected_session() {
                    ui.heading(&session.title);
                    ui.label(format!("Profile: {}", session.profile_id));
                    ui.label(format!(
                        "Connected: {}",
                        session.connected_at.format("%H:%M:%S")
                    ));
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
                    remote_canvas(ui);
                } else {
                    ui.heading("No session selected");
                    ui.label("Connect to a profile to start a remote desktop session.");
                }
            });
        });
    }

    fn settings_view(&mut self, ui: &mut Ui) {
        page_header(ui, "Settings", "Runtime and integration status.");
        panel(ui, |ui| {
            ui.heading("Rust migration status");
            ui.label("UI runtime: native eframe/egui");
            ui.label("Persistent profile store: JSON in user data directory");
            ui.label("RDP backend: trait-based adapter launching mstsc.exe on Windows");
            ui.label("WPF/.NET project files: removed from workspace");
        });
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
    ui.add_sized([260.0, 30.0], egui::TextEdit::singleline(value));
}

fn metric(ui: &mut Ui, label: &str, value: String) {
    ui.horizontal(|ui| {
        ui.label(RichText::new(label).color(Color32::from_rgb(93, 103, 119)));
        ui.with_layout(Layout::right_to_left(Align::Center), |ui| {
            ui.label(RichText::new(value).strong());
        });
    });
}

fn remote_canvas(ui: &mut Ui) {
    let desired_size = Vec2::new(ui.available_width(), 260.0);
    let (rect, _) = ui.allocate_exact_size(desired_size, Sense::hover());
    let painter = ui.painter_at(rect);
    painter.rect(
        rect,
        CornerRadius::same(8),
        Color32::from_rgb(19, 24, 32),
        Stroke::new(1.0, Color32::from_rgb(69, 79, 96)),
        StrokeKind::Outside,
    );
    painter.text(
        rect.center(),
        egui::Align2::CENTER_CENTER,
        "Remote desktop viewport",
        FontId::proportional(20.0),
        Color32::from_rgb(190, 202, 220),
    );
}
