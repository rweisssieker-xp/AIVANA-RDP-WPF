mod ai;
mod app;
mod certificate;
mod computer_use;
mod diagnostics;
mod ironrdp_client;
mod memory;
mod models;
mod policy;
mod runbook;
mod security;
mod services;
mod timeline;
mod workspace;

use app::AivanaApp;

fn main() -> eframe::Result<()> {
    let options = eframe::NativeOptions {
        viewport: eframe::egui::ViewportBuilder::default()
            .with_inner_size([1280.0, 820.0])
            .with_min_inner_size([900.0, 620.0]),
        ..Default::default()
    };

    eframe::run_native(
        "Aivana Rust RDP Client",
        options,
        Box::new(|cc| Ok(Box::new(AivanaApp::new(cc)))),
    )
}
