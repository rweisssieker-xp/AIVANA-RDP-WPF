# Changelog

## 0.1.0 - Rust Migration

- Removed the legacy .NET application and test projects.
- Added a native Rust desktop application using `eframe`/`egui`.
- Added profile management with search, groups, tags, favorites, and JSON persistence.
- Added a session dashboard with live metrics.
- Added a `RemoteDesktopEngine` trait and native IronRDP session runtime.
- Added decoded framebuffer events and egui texture rendering for the remote viewport.
- Added pointer, click, scroll, and typed-text forwarding through the RDP input channel.
- Added secure profile persistence that omits passwords and stores credential references.
- Added local preflight diagnostics, typed error classification, and local KI explanations.
- Added policy-gated Computer Use scaffolding on native RDP framebuffers.
- Added redacted timeline events and Markdown incident export.
- Added workspace, report, and AI/KI model boundaries for the planned cockpit.
- Added certificate trust gate and local KI risk explanation.
- Added Approval Center, runbook engine, host/workspace memory, blackbox snapshots, JSON evidence export, and proactive KI USP actions.
- Added reconnect/resize engine boundaries for RDP session hardening.
- Added Windows DPAPI-backed persistent credential store and profile password migration cleanup.
- Added persistent certificate trust, timeline/blackbox, workspace, and host memory stores.
- Added RDP TLS fingerprint probing for certificate trust decisions.
- Added approved-action execution, runbook next-step execution, pause/resume/abort, incident files on disk, and keyboard hotkey forwarding.
- Replaced legacy desktop documentation with Rust architecture notes.
- Added Rust unit tests for model behavior, credentials, diagnostics, redaction, policy, certificate trust, screen observation, runbooks, memory, AI explanations, and evidence export.
