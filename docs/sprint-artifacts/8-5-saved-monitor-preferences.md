# Story 8.5: Saved Monitor Preferences

Status: drafted

## Story

As a user,
I want monitor preferences saved per connection profile,
so that each connection remembers its monitor configuration.

## Acceptance Criteria

1. **AC1:** Monitor preferences saved in ConnectionProfile.Settings
2. **AC2:** Monitor configuration automatically applied when reconnecting
3. **AC3:** Preferences persist across application restarts

## Tasks / Subtasks

- [ ] Task 1: Store Monitor Preferences (AC: #1, #3)
  - [ ] Store monitor configuration in ConnectionProfile.Settings JSON
  - [ ] Preferences persist in database
- [ ] Task 2: Auto-Apply Preferences (AC: #2)
  - [ ] Apply saved preferences when connection is established

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Preference storage follows Architecture "Configuration" patterns

**Technical Constraints:**
- Preference storage: Store monitor configuration in ConnectionProfile.Settings JSON
- Auto-apply: Apply saved preferences when connection is established
- Persistence: Preferences persist in database

### References

- [Source: docs/epics.md#Story-8.5] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-8.md#Acceptance-Criteria] - AC from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

