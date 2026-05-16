# Aivana Rust RDP Client

Native Rust desktop application for managing remote desktop connection profiles and sessions.

The previous WPF/.NET project has been removed. The app now builds as a Rust binary using `eframe`/`egui` for a modern native GUI without WebView or Tauri.

## Current Capabilities

- Native desktop shell with modern sidebar navigation
- Connection profile list, search, editor, groups, tags, favorites
- Persistent profile storage in the user data directory
- Session dashboard with active session list and live metrics panel
- Trait-based remote desktop backend abstraction
- Windows RDP launch adapter using `mstsc.exe`

## Important Status

The GUI and application structure are now Rust-native. The current RDP adapter launches `mstsc.exe` on Windows from Rust and tracks the session in the native UI. The low-level protocol remains isolated behind a Rust trait, so a deeper backend such as IronRDP or FreeRDP bindings can replace the launcher without changing the profile editor or session dashboard.

## Requirements

- Rust 1.95 or newer
- Windows, Linux, or macOS supported by `eframe`

## Build

```powershell
cargo build
```

## Run

```powershell
cargo run
```

## Project Structure

```text
src/
  main.rs       App entry point
  app.rs        Native egui desktop UI
  models.rs     Profiles, sessions, metrics
  services.rs   Profile persistence and RDP backend abstraction
```

## Next Engineering Step

Replace `NativeRdpEngine` in `src/services.rs` with an embedded RDP backend if the remote desktop viewport must render inside the Aivana window instead of launching the Windows RDP client.
