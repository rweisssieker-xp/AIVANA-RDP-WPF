# Story 8.4: Monitor Layout Arrangement

Status: drafted

## Story

As a user,
I want to arrange monitor layout to match my physical setup,
so that remote desktop matches my local monitor arrangement.

## Acceptance Criteria

1. **AC1:** Users can drag monitors to match physical arrangement
2. **AC2:** Monitor positions saved
3. **AC3:** Remote desktop layout matches arrangement

## Tasks / Subtasks

- [ ] Task 1: Create Layout Arrangement UI (AC: #1, #2)
  - [ ] Visual monitor arrangement tool (drag-and-drop)
  - [ ] Store monitor positions in ConnectionProfile.Settings
- [ ] Task 2: Apply Monitor Arrangement (AC: #3)
  - [ ] Apply monitor arrangement to RDP control

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Monitor arrangement follows Architecture "Multi-Monitor Support" section

**Technical Constraints:**
- Monitor arrangement: Store monitor positions in ConnectionProfile.Settings
- Layout UI: Visual monitor arrangement tool (drag-and-drop)
- RDP configuration: Apply monitor arrangement to RDP control

### References

- [Source: docs/epics.md#Story-8.4] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-8.md#Acceptance-Criteria] - AC from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

