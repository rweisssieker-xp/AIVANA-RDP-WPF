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
- Persistent credential store: Windows DPAPI-protected secrets on Windows, profile JSON stores only `credential_id`.
- Certificate trust gate with RDP TLS fingerprint probing, local persistence, reject/trust decisions, and KI risk explanation.
- Persistent Timeline and Blackbox evidence store with redaction, Markdown incident export, JSON evidence export, and disk export folders.
- Guarded Computer Use flow that observes native framebuffers, detects basic screen state, plans actions, queues approvals, executes approved input, and traces verification.
- Approval Center for elevated-risk KI actions with allow/deny/runbook/abort decisions.
- Runbook engine with first diagnostic/evidence runbooks, Evidence Mode, next-step execution, pause/resume/abort.
- Persistent Workspace Cockpit with host memory, workspace memory, runbook inventory, open approvals, and recommended next step.
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
  security.rs        DPAPI-backed credential persistence and redaction
  certificate.rs     Certificate trust classification, probing, persistence
  diagnostics.rs     Preflight and typed error classification
  ai.rs              Local and optional-provider KI abstraction
  computer_use.rs    Frame observation, action planning, policy-gated execution
  policy.rs          Computer Use safety decisions
  runbook.rs         Local diagnostic and evidence runbooks
  memory.rs          Redacted persistent host and workspace memory
  timeline.rs        Persistent session audit, blackbox snapshots, incident export
  workspace.rs       Persistent workspace cockpit model
```

## Verification

```powershell
cargo fmt
cargo check
cargo test
cargo build
```
