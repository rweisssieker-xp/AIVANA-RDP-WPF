# Story 6.6: File Transfer Settings

Status: drafted

## Story

As a user,
I want to configure file transfer settings,
so that transfers behave according to my preferences.

## Acceptance Criteria

1. **AC1:** Users can configure: default destination folder, maximum concurrent transfers (default 3), transfer speed limit (optional), overwrite behavior (ask/overwrite/skip)
2. **AC2:** Settings saved and applied to future transfers

## Tasks / Subtasks

- [ ] Task 1: Create Transfer Settings UI (AC: #1, #2)
  - [ ] Add file transfer settings in Settings > File Transfer
  - [ ] Configure: DefaultDestinationFolder, MaxConcurrentTransfers, SpeedLimit, OverwriteBehavior
  - [ ] Store settings in ApplicationSettings
  - [ ] Apply settings to future transfers

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Transfer settings follow Architecture "Configuration" patterns

**Technical Constraints:**
- Transfer settings: Store in ApplicationSettings
- Settings: DefaultDestinationFolder, MaxConcurrentTransfers, SpeedLimit, OverwriteBehavior
- UI: File transfer settings in Settings > File Transfer

### References

- [Source: docs/epics.md#Story-6.6] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-6.md#Acceptance-Criteria] - AC from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

