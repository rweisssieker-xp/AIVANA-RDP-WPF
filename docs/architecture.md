# Aivana Native Rust RDP + KI Architecture

## Goal

Aivana is a native Rust desktop RDP and operations cockpit. The runtime is local-first: RDP display/input, profile storage, preflight diagnostics, timeline evidence, policy gates, and Computer Use traces run inside the Rust application.

## Runtime

- GUI: `eframe`/`egui`
- Language: Rust 2024 edition
- Storage: JSON profile file in the user data directory, with profile passwords omitted
- Secrets: Windows DPAPI-protected credential records on Windows, behind `CredentialStore`
- Remote backend: `RemoteDesktopEngine` trait backed by IronRDP
- KI: local deterministic provider by default; optional provider boundary exists for future configured cloud calls

## Source Layout

```text
src/
  app.rs             UI state, viewport rendering, diagnostics and KI panels
  certificate.rs     Certificate trust status and risk explanations
  ironrdp_client.rs  IronRDP connector, active stage, framebuffer events, input PDUs
  services.rs        Engine trait, session channels, profile store
  models.rs          Public app contracts
  security.rs        Credential boundary and redaction
  diagnostics.rs     Preflight, error classification, findings
  ai.rs              KI provider trait and local implementation
  computer_use.rs    Native framebuffer observer and policy-gated action loop
  policy.rs          Risk classification and approval decisions
  runbook.rs         Diagnostic and evidence runbook engine
  memory.rs          Redacted host and workspace memory
  timeline.rs        Session audit and incident export
  workspace.rs       Workspace cockpit model
```

## RDP Engine Boundary

`RemoteDesktopEngine` owns the native runtime contract:

- `connect` creates a session and starts an IronRDP worker thread.
- `reconnect` restarts the native worker for an existing session.
- `resize` records the display resize boundary for future IronRDP dynamic display support.
- `poll_frame` returns decoded `FrameUpdate` payloads for texture rendering.
- `send_input` forwards pointer, wheel, and text actions to the RDP input channel.
- `poll_events` exposes state, frame, error, and disconnect events for timelines and diagnostics.

The UI only consumes the trait. IronRDP-specific details stay in `ironrdp_client.rs`.

## Security Model

- Profile JSON stores host, port, username, domain, tags, workspace, and `credential_id`.
- Password fields are marked `skip_serializing`.
- `CredentialStore` persists protected credential records and keeps secrets outside profile persistence.
- `CertificateTrustStore` persists host/port/fingerprint decisions and can probe the RDP TLS server key before trusting.
- Timeline, diagnostics, KI prompts, and reports pass through redaction helpers before being stored or exported.
- Computer Use actions are classified as read-only, low-risk, elevated-risk, or destructive before execution.

## KI and Computer Use

- `LocalPreflightService` produces deterministic findings before RDP connection attempts.
- `LocalAiProvider` explains failures and summarizes sessions without cloud calls.
- `ComputerUseAgent` observes native RDP `FrameUpdate` data, plans a next action, applies `PolicyEngine`, and can verify frame changes.
- The app exposes proactive KI actions: failure explanation, session comparison, safe next action, evidence mode, ticket draft, and runbook recommendation.
- `RunbookEngine` provides first local read-only diagnostic/evidence workflows with start, next-step, pause, resume, and abort.
- `Approval Center` executes allowed actions through the native RDP input path and traces decisions.
- `MemoryStore` keeps redacted persistent host/workspace notes for better future recommendations.
- Incident export writes Markdown and JSON evidence bundles to the local app data folder.

## Verification

Core verification commands:

```powershell
cargo fmt
cargo check
cargo test
cargo build
```

Manual verification still requires a reachable RDP host to prove end-to-end display and input behavior against a real remote desktop.
