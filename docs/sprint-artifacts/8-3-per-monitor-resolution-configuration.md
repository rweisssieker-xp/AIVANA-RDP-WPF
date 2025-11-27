# Story 8.3: Per-Monitor Resolution Configuration

Status: drafted

## Story

As a user,
I want to configure different resolutions per monitor,
so that each monitor uses its optimal resolution.

## Acceptance Criteria

1. **AC1:** Users can set resolution for each monitor individually
2. **AC2:** Remote desktop uses the configured resolution per monitor
3. **AC3:** Resolutions saved per connection profile

## Tasks / Subtasks

- [ ] Task 1: Create Resolution Configuration UI (AC: #1, #3)
  - [ ] UI to set resolution for each selected monitor
  - [ ] Store resolution per monitor in ConnectionProfile.Settings
- [ ] Task 2: Apply Per-Monitor Resolution (AC: #2)
  - [ ] Apply resolutions when connection is established

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Per-monitor resolution follows Architecture "Multi-Monitor Support" section

**Technical Constraints:**
- Per-monitor resolution: Store resolution per monitor in ConnectionProfile.Settings
- Resolution configuration: UI to set resolution for each selected monitor
- RDP application: Apply resolutions when connection is established

### References

- [Source: docs/epics.md#Story-8.3] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-8.md#Acceptance-Criteria] - AC from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

