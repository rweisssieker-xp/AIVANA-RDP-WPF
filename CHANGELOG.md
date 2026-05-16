# Changelog

## 0.1.0 - Rust Migration

- Removed the legacy WPF/.NET application and test projects.
- Added a native Rust desktop application using `eframe`/`egui`.
- Added profile management with search, groups, tags, favorites, and JSON persistence.
- Added a session dashboard with live metrics.
- Added a `RemoteDesktopEngine` trait and Windows `mstsc.exe` launch adapter.
- Replaced legacy WPF documentation with Rust architecture notes.
- Added initial Rust unit tests for model behavior.
