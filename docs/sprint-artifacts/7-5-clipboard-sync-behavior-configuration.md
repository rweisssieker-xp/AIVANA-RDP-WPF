# Story 7.5: Clipboard Sync Behavior Configuration

Status: drafted

## Story

As a user,
I want to configure clipboard sync behavior,
so that sync works according to my preferences.

## Acceptance Criteria

1. **AC1:** Users can configure: auto-sync (enabled/disabled), manual sync mode, format filtering (text only, images only, all formats)
2. **AC2:** Settings saved and applied globally or per-connection

## Tasks / Subtasks

- [ ] Task 1: Create Clipboard Sync Configuration UI (AC: #1, #2)
  - [ ] Add clipboard sync configuration in Settings > Clipboard
  - [ ] Configure: AutoSync, ManualSync, FormatFilter
  - [ ] Store in ApplicationSettings or ConnectionProfile.Settings

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Sync behavior configuration follows Architecture "Configuration" patterns

**Technical Constraints:**
- Sync behavior: Store in ApplicationSettings or ConnectionProfile.Settings
- Configuration options: AutoSync, ManualSync, FormatFilter
- UI: Clipboard sync configuration in Settings > Clipboard

### References

- [Source: docs/epics.md#Story-7.5] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-7.md#Acceptance-Criteria] - AC from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

