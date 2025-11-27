# Story 9.5: Recording Export Formats

Status: drafted

## Story

As a user,
I want to export recordings in multiple formats,
so that I can use recordings in different contexts.

## Acceptance Criteria

1. **AC1:** Users can choose export format: MP4, AVI, or MOV
2. **AC2:** Export converts video to selected format
3. **AC3:** Export preserves video quality
4. **AC4:** Export progress shown

## Tasks / Subtasks

- [ ] Task 1: Implement Format Conversion (AC: #1, #2, #3)
  - [ ] Export formats: MP4 (default), AVI, MOV
  - [ ] Format conversion: Use Windows Media Foundation or FFmpeg
- [ ] Task 2: Create Export UI (AC: #1, #4)
  - [ ] Export dialog with format selection
  - [ ] Show export progress during conversion

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Export formats follow Architecture "Session Recording" section

**Technical Constraints:**
- Export formats: MP4 (default), AVI, MOV
- Format conversion: Use Windows Media Foundation or FFmpeg
- Export UI: Export dialog with format selection
- Progress: Show export progress during conversion

### References

- [Source: docs/epics.md#Story-9.5] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-9.md#Acceptance-Criteria] - AC from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

