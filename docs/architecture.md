# Aivana Rust RDP Client Architecture

## Goal

Aivana is now a native Rust desktop application. The legacy WPF/.NET codebase has been removed from the application source tree.

## Runtime

- GUI: `eframe`/`egui`
- Language: Rust 2024 edition
- Storage: JSON profile file in the user data directory
- Remote backend: `RemoteDesktopEngine` trait with a Windows `mstsc.exe` launch adapter

## Source Layout

```text
src/
  main.rs       Native app bootstrap
  app.rs        UI state and egui rendering
  models.rs     Profiles, sessions, metrics
  services.rs   Persistence and remote desktop backend boundary
```

## Backend Boundary

The UI does not depend on a concrete RDP implementation. It calls `RemoteDesktopEngine`, whose current adapter launches `mstsc.exe` on Windows and tracks the session in the Rust UI. A deeper embedded implementation can replace `NativeRdpEngine` without changing the profile editor, session dashboard, or metrics UI.

Candidate backends:

- IronRDP-based Rust integration
- FreeRDP bindings
- Windows-specific adapter behind the same Rust trait

## Current Verification

- `cargo fmt`
- `cargo check`
- `cargo test`
- `cargo build`
