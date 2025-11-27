# Story 6.1: Drag-and-Drop File Transfer

Status: drafted

## Story

As a user,
I want to transfer files by dragging and dropping,
so that I can quickly move files without complex dialogs.

## Acceptance Criteria

1. **AC1:** Drop zone overlay appears showing file count and total size
2. **AC2:** File transfer begins immediately when files are released
3. **AC3:** Transfer progress shown in transfer queue panel

## Tasks / Subtasks

- [ ] Task 1: Implement Drag-and-Drop (AC: #1, #2)
  - [ ] Implement DragDrop.DoDragDrop and DragOver events
  - [ ] Create drop zone visual overlay showing drop area and file preview
  - [ ] Begin file transfer when files are released
- [ ] Task 2: Integrate with File Transfer Service (AC: #3)
  - [ ] Use RDP Virtual Channels for file transfer (see Architecture)
  - [ ] Show transfer progress in transfer queue panel
  - [ ] See UX spec section 5.1 Journey 3 for drag-and-drop flow

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- File transfer follows Architecture ADR-006 (RDP Virtual Channels)
- UI follows UX spec section 5.1 Journey 3

**Technical Constraints:**
- Drag-and-drop: Implement DragDrop.DoDragDrop and DragOver events
- Drop zone: Visual overlay showing drop area and file preview
- File transfer: Use RDP Virtual Channels for file transfer (see Architecture)

### References

- [Source: docs/epics.md#Story-6.1] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-6.md#Acceptance-Criteria] - AC from tech spec
- [Source: docs/ux-design-specification.md#Section-5.1] - Journey 3 drag-and-drop flow

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

