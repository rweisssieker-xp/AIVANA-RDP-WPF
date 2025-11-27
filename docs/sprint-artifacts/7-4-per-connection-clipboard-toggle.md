# Story 7.4: Per-Connection Clipboard Toggle

Status: drafted

## Story

As a user,
I want to toggle clipboard synchronization per connection,
so that I can control sync behavior for each session.

## Acceptance Criteria

1. **AC1:** Users can enable/disable clipboard synchronization per connection
2. **AC2:** Setting saved in ConnectionProfile.Settings
3. **AC3:** Clipboard sync does not occur if disabled for that connection

## Tasks / Subtasks

- [ ] Task 1: Add Clipboard Toggle to Connection Settings (AC: #1, #2, #3)
  - [ ] Add clipboard sync toggle in connection settings
  - [ ] Store setting in ConnectionProfile.Settings JSON
  - [ ] Apply setting when connection is established

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Per-connection settings follow Architecture "Configuration" patterns

**Technical Constraints:**
- Clipboard toggle: Store setting in ConnectionProfile.Settings JSON
- Per-connection: Apply setting when connection is established
- UI: Clipboard sync toggle in connection settings

### References

- [Source: docs/epics.md#Story-7.4] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-7.md#Acceptance-Criteria] - AC from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

