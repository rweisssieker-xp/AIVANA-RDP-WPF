# Aivana Rust RDP Client

Native Rust desktop application for RDP operations, diagnostics, incident evidence, and guarded KI Computer Use.

The app builds as a single Rust desktop binary with `eframe`/`egui`. There is no external Windows RDP launcher, no WebView shell, and no browser runtime.

## Current Capabilities

- Native desktop shell with sidebar navigation, profiles, sessions, settings, and a live RDP viewport.
- Connection profile list, search, editor, groups, tags, favorites, and JSON persistence.
- Passwords are skipped from profile JSON; the credential boundary stores only `credential_id` on profiles.
- Long-running IronRDP session thread with state events, framebuffer updates, disconnect/error handling, and native input channel.
- egui texture rendering for decoded IronRDP framebuffer frames.
- Pointer move, click, scroll, and typed text forwarding into the RDP input path.
- Local preflight diagnostics for DNS/TCP/credential readiness.
- Local KI diagnosis from deterministic findings and session evidence.
- Certificate trust gate with local fingerprint status and KI risk explanation.
- Timeline and Blackbox evidence store with redaction, Markdown incident export, and JSON evidence export.
- Guarded Computer Use flow that observes native framebuffers, detects basic screen state, plans actions, queues approvals, and traces observations.
- Approval Center for elevated-risk KI actions.
- Runbook engine with first diagnostic/evidence runbooks and Evidence Mode.
- Workspace cockpit with host memory, runbook inventory, open approvals, and recommended next step.
- Proactive KI USP actions: Why did this fail, What changed, Safe next action, Evidence mode, Ticket in 30 seconds, Runbook recommendation.

## Requirements

- Rust 1.95 or newer
- Windows, Linux, or macOS supported by `eframe`
- A reachable RDP endpoint for real-session testing

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
  main.rs            Native app bootstrap
  app.rs             egui desktop UI, framebuffer viewport, diagnostics panels
  ironrdp_client.rs  Native IronRDP connection, active-stage loop, frames, input PDUs
  services.rs        Profile persistence and RemoteDesktopEngine runtime channels
  models.rs          Public models and interface payloads
  security.rs        Credential boundary and redaction
  certificate.rs     Certificate trust classification and local fingerprint decisions
  diagnostics.rs     Preflight and typed error classification
  ai.rs              Local and optional-provider KI abstraction
  computer_use.rs    Frame observation, action planning, policy-gated execution
  policy.rs          Computer Use safety decisions
  runbook.rs         Local diagnostic and evidence runbooks
  memory.rs          Redacted host and workspace memory
  timeline.rs        Session event audit and incident export
  workspace.rs       Workspace cockpit model
```

## Verification

```powershell
cargo fmt
cargo check
cargo test
cargo build
```
