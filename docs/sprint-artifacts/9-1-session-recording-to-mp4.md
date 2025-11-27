# Story 9.1: Session Recording to MP4

Status: drafted

## Story

As a user,
I want to record remote desktop sessions to video files,
so that I can capture sessions for later review.

## Acceptance Criteria

1. **AC1:** Recording begins immediately when started
2. **AC2:** Video saved as MP4 format
3. **AC3:** Recording indicator appears in UI
4. **AC4:** Recording can be stopped at any time
5. **AC5:** Video file saved to configured location

## Tasks / Subtasks

- [ ] Task 1: Implement Recording Service (AC: #1, #2, #5)
  - [ ] Use Windows Media Foundation for video encoding (see Architecture)
  - [ ] Video format: MP4 (H.264 codec)
  - [ ] Capture RDP screen content frame-by-frame
  - [ ] Save to %AppData%\Aivana_RDP_WPF\Recordings\ or user-configured location
- [ ] Task 2: Create Recording UI (AC: #3, #4)
  - [ ] Add recording indicator in UI
  - [ ] Add stop recording button

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Recording follows Architecture ADR-007 (Windows Media Foundation)
- File storage follows Architecture "File Storage" patterns

**Technical Constraints:**
- Recording: Use Windows Media Foundation for video encoding (see Architecture)
- Video format: MP4 (H.264 codec)
- Recording capture: Capture RDP screen content frame-by-frame
- File storage: Save to %AppData%\Aivana_RDP_WPF\Recordings\ or user-configured location

### References

- [Source: docs/epics.md#Story-9.1] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-9.md#Acceptance-Criteria] - AC from tech spec
- [Source: docs/architecture.md#Technology-Stack-Details] - Windows Media Foundation

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

