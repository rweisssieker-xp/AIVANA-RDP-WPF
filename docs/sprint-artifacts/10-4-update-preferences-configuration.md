# Story 10.4: Update Preferences Configuration

Status: drafted

## Story

As a user,
I want to configure update preferences,
so that updates work according to my preferences.

## Acceptance Criteria

1. **AC1:** Users can configure: auto-update (enabled/disabled), update notification (always/never), update check frequency
2. **AC2:** Settings saved
3. **AC3:** Update behavior follows configured preferences

## Tasks / Subtasks

- [ ] Task 1: Create Update Settings UI (AC: #1, #2, #3)
  - [ ] Add update settings in Settings > Updates
  - [ ] Configure: AutoUpdate, NotificationMode, CheckFrequency
  - [ ] Store in ApplicationSettings
  - [ ] Update implementation: Check for updates based on preferences (can be post-MVP)

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Update settings follow Architecture "Configuration" patterns

**Technical Constraints:**
- Update settings: Store in ApplicationSettings
- Update options: AutoUpdate, NotificationMode, CheckFrequency
- Update implementation: Check for updates based on preferences (can be post-MVP)

### References

- [Source: docs/epics.md#Story-10.4] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-10.md#Acceptance-Criteria] - AC from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

