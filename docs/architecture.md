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
  autopilot.rs       Local and OpenAI Computer Use Autopilot providers
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
- `--rdp-preflight` runs the local diagnostic path without the GUI and persists redacted JSON evidence for environment readiness, DNS/TCP reachability, credential presence, and effective timeout.
- `--rdp-smoke-test` runs the same Rust IronRDP runtime without the GUI, using `AIVANA_RDP_TEST_*` environment variables to prove connection, framebuffer, and a pointer-move input probe against a real endpoint, then emits and persists structured JSON evidence for both success and failure. Operators can tune slow endpoint verification with `AIVANA_RDP_TEST_TIMEOUT_SECS` or `--rdp-smoke-timeout` while the recorded evidence keeps the effective timeout.
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
- `AutopilotController` runs a bounded session loop over live framebuffers, supports local planning or opt-in OpenAI Computer Use via the Responses API, persists operator goal/provider/model/step/delay preferences, preserves batched `computer_call` actions through approval, execution, timeline audit, and LLM handoff evidence.
- The app exposes proactive KI actions: failure explanation, session comparison, safe next action, evidence mode, ticket draft, and runbook recommendation.
- `RunbookEngine` provides first local read-only diagnostic/evidence workflows with start, next-step, pause, resume, and abort.
- `Approval Center` executes allowed actions through the native RDP input path and traces decisions.
- `MemoryStore` keeps redacted persistent host/workspace notes for better future recommendations.
- Incident export writes Markdown and JSON evidence bundles to the local app data folder.
- LLM handoff export builds a redacted prompt-ready Markdown artifact from diagnostics, timeline events, blackbox snapshots, and Autopilot trace so operators can ask a cloud or local model for next-step analysis without pasting raw secrets.
- Workspace KI Operations provides session/provider/framebuffer readiness, a redacted KI Action Brief, Autopilot goal priming, and one-click LLM handoff without leaving the cockpit.
- KI Verification Center summarizes OpenAI key readiness, local `rdp-live.env` auto-detection, RDP smoke-test environment readiness, persisted Autopilot preference status, active session count, and copyable verification evidence.
- Runbook LLM Briefs summarize local runbooks, read-only actions, risk levels, approval requirements, and expected model output for safe triage.
- Prompt Library Briefs package the current action context into reusable diagnosis, runbook-selection, ticket, and verification prompts.
- AI Brief Pack export persists the action brief, prompt library, runbook brief, and verification brief as redacted Markdown files under local app data with a JSON manifest for audit.
- KI Verification Center reads the latest AI Brief Pack manifest so exported LLM artifacts remain visible after creation.
- Policy Guardrail Briefs expose the current safety behavior around allowed, approval-gated, and denied Autopilot actions.
- OpenAI CUA Request Briefs describe provider, model, goal, framebuffer readiness, bounded step configuration, and guardrails before hosted Computer Use is invoked.
- KI Goal Audit turns the active objective into a prompt-to-artifact checklist with displayed RDP env source, blocking OpenAI/RDP gates, next-gate guidance, structured next-gate JSON copy, command-index JSON copy, visible handoff-check status, handoff-check JSON copy, handoff risk and verification snapshot JSON copy/export, direct RDP proof check JSON copy, no-secrets RDP proof prompt copy, RDP recovery plan copy with acceptance criteria, direct live evidence, and proxy-evidence rejection, recovery command copy, live-gate doctor JSON copy, live-gate operator brief copy, LLM review prompt copy with `summary.md` Early LLM Triage and `operator-handoff-risk-summary.json` cross-check, live-gate LLM plan copy, LLM action-contract JSON copy/export with `direct_live_evidence_requirements`, latest handoff-pack and live-gate evidence, a live-gate runbook, copyable RDP env template, copyable Markdown, one-click Operator Handoff Pack export, and persisted JSON/Markdown audit files.

## Verification

Core verification commands:

```powershell
cargo fmt
cargo check
cargo test
cargo build
```

Manual verification still requires a reachable RDP host to prove end-to-end display and input behavior against a real remote desktop.

Use `cargo run -- --ai-help` to print the built-in headless KI/RDP command reference from the binary. It lists readiness, evidence bundle, completion audit, live gate, RDP preflight, RDP smoke test, operator handoff, and LLM review commands.

Use `cargo run -- --command-index` to print the same headless KI/RDP workflow as structured JSON for CI, automation, or LLM agents. The index and next-live-gate command list include output format, persistence behavior, required environment variables, expected artifact folders, purpose, success conditions, and a recommended live-gate sequence from env template through smoke test, direct RDP proof check, proof recovery plan with direct live evidence requirements, acceptance criteria, and proxy-evidence rejection, live-gate doctor, operator brief, LLM review prompt with `summary.md` Early LLM Triage and risk-summary cross-check semantics, LLM live-gate plan, LLM action contract with `summary.md` Early LLM Triage, `direct_live_evidence_requirements`, and `early_triage_artifacts`, completion audit, goal evidence matrix, hard evidence check, handoff pack with `summary.md` Early LLM Triage plus `verification-snapshot.json` and `llm-action-contract.json`, handoff-pack validation, risk summary, and verification snapshot.

Use `cargo run -- --rdp-env-template` to print a redacted `.env` starter or `cargo run -- --save-rdp-env-template .\rdp-live.env` to create one without overwriting an existing file, then `cargo run -- --rdp-env-fill-guide`, then `cargo run -- --rdp-env-file-check --rdp-env-file .\rdp-live.env`, then `cargo run -- --rdp-preflight --rdp-env-file .\rdp-live.env --rdp-smoke-timeout 90`, then `cargo run -- --rdp-smoke-test --rdp-env-file .\rdp-live.env --rdp-smoke-timeout 90` with `AIVANA_RDP_TEST_HOST`, `AIVANA_RDP_TEST_USER`, and `AIVANA_RDP_TEST_PASSWORD` filled in the file to collect structured endpoint evidence from the native Rust path. The env-file loader and KI readiness checks reject unfilled starter placeholders such as `<host>` and `[REDACTED]`; `--rdp-env-file-check` performs that validation without DNS, TCP, or RDP connection attempts and persists JSON evidence under `rdp-env-file-checks`. Set `AIVANA_RDP_TEST_TIMEOUT_SECS` for a persistent timeout default. Operators can also pass `--rdp-env-file <path>` to `--rdp-preflight`, `--rdp-smoke-test`, or `--live-gate` to read the same `AIVANA_RDP_TEST_*` values from a local `.env` file without changing the process environment.

Use `cargo run -- --save-ki-preferences` to persist the same Autopilot preferences used by the GUI without launching the app. It writes or refreshes local `autopilot-preferences.json` and prints a redacted JSON save report.

Use `cargo run -- --ki-readiness-report` to emit and persist a redacted JSON readiness report for CI or handoff checks. The report includes OpenAI key status, RDP smoke-test environment readiness, persisted Autopilot preference status, latest RDP preflight, smoke, and live-gate evidence, exact missing `AIVANA_RDP_TEST_*` names or `--rdp-env-file` validation errors, missing requirements, readiness score, and the recommended next verification step.

Use `cargo run -- --ai-brief-pack` to export the redacted AI/KI action brief, prompt library, runbook brief, verification brief, and manifest without launching the GUI. This is the headless equivalent of the Verification Center AI Brief Pack export.

Use `cargo run -- --ki-evidence-bundle` to persist a redacted Markdown and JSON bundle that combines KI readiness, latest RDP preflight evidence, latest RDP smoke evidence, latest live-gate evidence, latest AI brief-pack evidence, missing requirements, and next-step guidance.

Use `cargo run -- --completion-audit --rdp-env-file .\rdp-live.env` to persist prompt-to-artifact JSON and Markdown audits that map the objective to implementation evidence, headless verification artifacts, AI brief pack export, the command index, operator handoff pack, handoff-pack validation, risk summary, and unresolved blockers. Passing `--rdp-env-file` keeps the embedded readiness view aligned with the local live-gate file. The audit only marks live completion when OpenAI CUA is configured and `rdp-proof-check.ok == true` with fresh matching env-file/preflight/smoke host-port evidence plus persisted RDP smoke-test evidence including `connected=true`.

Use `cargo run -- --goal-evidence-matrix` to print and persist the same requirement coverage as one structured JSON object for LLM or CI review. The matrix includes success criteria, verification commands, requirement evidence, uncovered requirements, and the next live gate.

Use `cargo run -- --goal-evidence-check` to print and persist a hard completion assertion over the matrix. It includes `ok`, failed requirements, and the embedded matrix, and exits non-zero until the audit is achieved and no uncovered requirements remain.

Use `cargo run -- --next-live-gate` to print and persist the first blocking live gate, the current evidence, exact missing RDP env vars, a redacted `.env` template, the goal evidence matrix/check commands, the handoff-pack validation follow-up, and exact next operator commands without reading the full audit JSON. Use `cargo run -- --live-gate-sequence` to print only that ordered command list for operators, CI, or LLM agents that do not need Markdown or JSON.

Use `cargo run -- --next-live-gate-json` to print the same next-gate data as structured JSON for CI or LLM agents.

Use `cargo run -- --live-gate-doctor --rdp-env-file .\rdp-live.env` to print and persist one redacted JSON ladder for env-file validation, preflight readiness, smoke connectivity, direct `rdp-proof-check` status, handoff-pack validation, completion audit status, the exact next command, and the canonical RDP proof recovery-plan command. The command exits non-zero until the live RDP proof and completion audit are satisfied, and it does not advance past the proof step while framebuffer or input-probe evidence is missing.

Use `cargo run -- --live-gate-runbook` to print and persist the same audit-derived live-gate runbook as redacted Markdown for operators, CI handoff, or LLM review. The runbook includes `--goal-evidence-matrix`, `--goal-evidence-check`, handoff-pack creation, `--operator-handoff-check` validation, `--operator-handoff-risk-summary`, and the final `--verification-snapshot` status export after the evidence exports.

Use `cargo run -- --llm-review-prompt` to print and persist a focused reviewer prompt that asks an LLM to inspect `summary.md` for Early LLM Triage first, then the current handoff evidence including `verification-snapshot.json`, `llm-action-contract.json`, `operator-handoff-risk-summary.json`, `goal-evidence-matrix.json`, `goal-evidence-check.json`, `command-index.json`, and `live-gate-sequence.txt`, cross-check the risk summary for the LLM triage entrypoint, required evidence files, and `early_triage_artifacts`, reject proxy completion signals, list weak live-gate proof, and flag any unredacted secret-looking text.

Use `cargo run -- --live-gate-operator-brief --rdp-env-file .\rdp-live.env` to print and persist a concise Markdown brief generated from the live-gate doctor. It gives a human operator or LLM the current stage, blocker, acceptance criteria, next command, next-after-success command, and evidence snapshot without parsing JSON.

Use `cargo run -- --llm-live-gate-plan --rdp-env-file .\rdp-live.env` to print and persist a Markdown instruction plan generated from the live-gate doctor, completion audit, and runbook. It tells an LLM or CI assistant to inspect `summary.md` for Early LLM Triage first, then the current artifacts including `verification-snapshot.json`, `llm-action-contract.json`, `live-gate-operator-brief.md`, and `live-gate-sequence.txt`, reject proxy completion evidence, preserve redaction, and return the next operator command.

Use `cargo run -- --llm-action-contract --rdp-env-file .\rdp-live.env` to print and persist a strict JSON contract for LLM and CI agents. It exposes the next command, allowed command sequence, acceptance criteria, required evidence files including `summary.md` for Early LLM Triage, direct live evidence requirements, required success signals, failed proof recovery actions, response rules including `early_triage_artifacts`, and proxy-evidence rejection rules, and exits non-zero until direct live RDP evidence is complete.

Use `cargo run -- --rdp-env-fill-guide` to print and persist a redacted local checklist for filling the ignored `rdp-live.env` file without putting credentials into Git, handoff packs, screenshots, or LLM prompts.

Use `cargo run -- --gui-operator-actions` to print and persist the GUI-focused operator checklist that maps the current blocking gate to Verification Center and Mission Control actions, including the displayed RDP env source line, evidence matrix/check JSON, handoff-check validation status, RDP proof recovery-plan and recovery-command copy, the full live-gate sequence copy, LLM action-contract `summary.md` Early LLM Triage evidence, `direct_live_evidence_requirements`, `early_triage_artifacts`, success signals and proxy-evidence rejection rules, LLM action-contract copy/export, and copy controls.

Use `cargo run -- --operator-handoff-pack --rdp-env-file .\rdp-live.env` to write a redacted app-data pack containing readiness JSON, completion-audit JSON, the goal evidence matrix/check JSON, `verification-snapshot.json`, `operator-handoff-risk-summary.json`, the command index JSON, latest RDP env-file check JSON, direct RDP proof check JSON, live-gate doctor JSON, live-gate operator brief Markdown, structured next-live-gate JSON, the plain-text live-gate sequence, the next-live-gate summary, the live-gate runbook, GUI operator actions, an RDP env template, an RDP env fill guide, an LLM review prompt, an RDP proof prompt, an RDP proof recovery plan, an LLM live-gate plan, `llm-action-contract.json`, a short summary, and a schema-versioned manifest with file roles and recommended inspection order for one-step operator or LLM handoff. The summary includes an early LLM triage hint that points from `verification-snapshot.json` to `llm-action-contract.json` and rejects proxy evidence unless contract success signals are met; the manifest `summary.md` role advertises the same early triage path. The manifest verification-snapshot role identifies the compact GUI/latest-evidence snapshot and embedded LLM action-contract, the review-prompt and live-gate-plan roles identify Early LLM Triage, risk-summary cross-checks, required evidence files, `early_triage_artifacts`, and proxy-evidence rejection, the proof-prompt role identifies Early LLM Triage, `llm-action-contract.json`, recovery-plan command, framebuffer/input evidence, and proxy-evidence rejection, the recovery-plan role identifies exact commands, acceptance criteria, direct live evidence, and proxy-evidence rejection, the risk-summary role identifies exact recovery commands plus the LLM triage entrypoint, required evidence files, and `early_triage_artifacts`, and the recommended inspection order keeps `llm-action-contract.json` immediately after `verification-snapshot.json` for early LLM triage. Passing `--rdp-env-file` keeps `readiness.json` aligned with the local file used by preflight and smoke tests.

Use `cargo run -- --operator-handoff-check` to validate the latest handoff pack schema, expected files, semantic file roles for machine and human handoff artifacts, recommended inspection order, machine-readable JSON schemas including the LLM action contract, consistency between the manifest, completion audit, evidence matrix, next-gate artifact, risk-summary recovery-plan fields, command-index command catalog and step expectations including handoff-pack early-triage semantics and action-contract success signals, the LLM action contract versus the doctor/proof/allowed command sequence, required action-contract evidence including `summary.md`, success-signal, and must-return/must-not-return response-rule fields including `early_triage_artifacts`, evidence-matrix GUI action-contract early-triage and success-signal coverage, GUI action mentions for action-contract success signals and proxy rejection, and that `live-gate-sequence.txt` matches `next-live-gate.json.next_commands` before sharing it with an operator, CI system, or LLM. The command exits non-zero when `ok=false`.

Use `cargo run -- --operator-handoff-risk-summary --rdp-env-file .\rdp-live.env` to print and persist a compact JSON risk snapshot over the live-gate doctor, latest handoff-pack check, and goal-evidence check. It exposes severity, blocker, acceptance criteria, next command, handoff validation counts, failed requirements, RDP proof failed-check actions, the canonical recovery-plan command, a recovery-plan evidence summary, and compact LLM triage fields for `summary.md`, `llm-action-contract.json`, `llm_action_contract_direct_live_evidence_requirements`, and `early_triage_artifacts`, and exits non-zero until `ok=true`.

Use `cargo run -- --verification-snapshot --rdp-env-file .\rdp-live.env` to print and persist a compact redacted GUI/live-gate/handoff snapshot for operators, CI jobs, and LLM reviewers. It combines the displayed RDP env source, readiness, completion blocker, live-gate stage, next commands, handoff validation counts, LLM action-contract required evidence files, direct live evidence requirements, LLM review prompt triage order and risk-summary cross-check fields, `early_triage_artifacts`, success signals and proxy rejection rules, risk summary, RDP proof failed-check actions, the proof recovery-plan command, and latest evidence pointers in one JSON artifact.

Use `cargo run -- --rdp-proof-check --rdp-env-file .\rdp-live.env` to print and persist a proof-only JSON checklist over env-file validation, preflight readiness, preflight/smoke host-port matching, env-file freshness, smoke connection, framebuffer, and input probe evidence. The JSON includes `failed_check_actions` with a recovery command for each failed proof check, and the operator handoff risk summary plus verification snapshot risk block mirror proof status, failed checks, actions, recovery-plan command/summary, blocker, acceptance criteria, and next commands for GUI/LLM recovery. Use `cargo run -- --rdp-proof-prompt --rdp-env-file .\rdp-live.env` to print and persist a no-secrets Markdown prompt focused on finishing only the live RDP smoke proof, starting from `summary.md` Early LLM Triage through `verification-snapshot.json` and `llm-action-contract.json`, and including the recovery-plan export command. Use `cargo run -- --rdp-proof-recovery-plan --rdp-env-file .\rdp-live.env` to print and persist a human-readable failed-check-to-command plan with acceptance criteria and proxy-evidence rejection rules.

The Verification Center exposes the same handoff risk and verification snapshot as visible status plus copy/export controls for human operators, CI handoff, and LLM review, including recovery-plan, recovery-plan-command, and failed-check recovery-command copy actions.

Use `cargo run -- --live-gate --rdp-env-file .\rdp-live.env --rdp-smoke-timeout 90` to execute the live gate as one headless operator command. It persists RDP preflight evidence, attempts the smoke test only when preflight recommends connecting, refreshes readiness and completion audit evidence, writes a redacted `live-gate-reports` JSON file with exact missing RDP env vars and an embedded `.env` template, and exits non-zero while any live gate remains blocked.
