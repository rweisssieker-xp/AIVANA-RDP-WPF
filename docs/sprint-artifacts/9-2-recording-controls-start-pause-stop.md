# Story 9.2: Recording Controls (Start/Pause/Stop)

Status: drafted

## Story

As a user,
I want to control recording (start/pause/stop),
so that I can manage recording sessions.

## Acceptance Criteria

1. **AC1:** Recording starts when record button clicked
2. **AC2:** Record button changes to pause button
3. **AC3:** Recording status shows "Recording..."
4. **AC4:** Recording pauses when pause button clicked
5. **AC5:** Pause button changes to resume button
6. **AC6:** Recording can be resumed
7. **AC7:** Recording stops when stop button clicked
8. **AC8:** Video file finalized and saved
9. **AC9:** Success notification appears

## Tasks / Subtasks

- [ ] Task 1: Implement Recording Controls (AC: #1, #2, #3, #4, #5, #6, #7, #8)
  - [ ] Start, Pause, Resume, Stop buttons
  - [ ] Track recording state (Stopped, Recording, Paused)
  - [ ] Recording controls in toolbar or connection tab
  - [ ] Complete video encoding when stopping
- [ ] Task 2: Success Notification (AC: #9)
  - [ ] Show success notification when recording stops

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Recording controls follow Architecture "Session Recording" section

**Technical Constraints:**
- Recording controls: Start, Pause, Resume, Stop buttons
- State management: Track recording state (Stopped, Recording, Paused)
- UI: Recording controls in toolbar or connection tab
- File finalization: Complete video encoding when stopping

### References

- [Source: docs/epics.md#Story-9.2] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-9.md#Acceptance-Criteria] - AC from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

