# Story 8.2: Monitor Selection

Status: drafted

## Story

As a user,
I want to select specific monitors for remote desktop display,
so that I can choose which monitors to use.

## Acceptance Criteria

1. **AC1:** Users can select which monitors to use (checkboxes for each monitor)
2. **AC2:** Remote desktop displays only on selected monitors
3. **AC3:** Selection saved per connection profile

## Tasks / Subtasks

- [ ] Task 1: Create Monitor Selection UI (AC: #1, #3)
  - [ ] Show monitor list with checkboxes in connection settings
  - [ ] Store selected monitor IDs in ConnectionProfile.Settings
- [ ] Task 2: Apply Monitor Selection (AC: #2)
  - [ ] Apply monitor selection to RDP control

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Monitor selection follows Architecture "Multi-Monitor Support" section

**Technical Constraints:**
- Monitor selection: Store selected monitor IDs in ConnectionProfile.Settings
- Monitor UI: Show monitor list with checkboxes in connection settings
- RDP configuration: Apply monitor selection to RDP control

### References

- [Source: docs/epics.md#Story-8.2] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-8.md#Acceptance-Criteria] - AC from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

