# Story 4.5: Toolbar Customization

Status: drafted

## Story

As a user,
I want to customize toolbar layouts and menu organization,
so that I can arrange tools according to my workflow preferences.

## Acceptance Criteria

1. **AC1:** Users can show/hide toolbar buttons
2. **AC2:** Users can reorder toolbar buttons via drag-and-drop
3. **AC3:** Toolbar layout saved and persists after restart

## Tasks / Subtasks

- [ ] Task 1: Implement Toolbar Customization (AC: #1, #2, #3)
  - [ ] Store toolbar configuration in ApplicationSettings
  - [ ] Use ToolBar control with customizable items
  - [ ] Implement drag-and-drop reordering
  - [ ] Save toolbar layout on change

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Toolbar customization follows Architecture "UI Framework" section
- Settings storage follows Architecture "Configuration" patterns

**Technical Constraints:**
- Toolbar customization: Store toolbar configuration in ApplicationSettings
- Toolbar UI: Use ToolBar control with customizable items
- Drag-and-drop: Implement reordering via drag-and-drop

### References

- [Source: docs/epics.md#Story-4.5] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-4.md#Acceptance-Criteria] - AC from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

