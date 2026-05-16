mod app;
mod models;
mod services;

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
