# Story 9.6: Recording History Management

Status: drafted

## Story

As a user,
I want to view recording history and manage recorded files,
so that I can find and organize my recordings.

## Acceptance Criteria

1. **AC1:** List of recordings with: connection name, date/time, duration, file size
2. **AC2:** Users can play recordings (if player available)
3. **AC3:** Users can delete recordings
4. **AC4:** Users can open recording folder
5. **AC5:** History shows last 50 recordings (configurable)

## Tasks / Subtasks

- [ ] Task 1: Store Recording Metadata (AC: #1, #5)
  - [ ] Store recording metadata in database
  - [ ] History model: ConnectionName, RecordedAt, Duration, FileSize, FilePath
  - [ ] History retention: Last 50 recordings (configurable)
- [ ] Task 2: Create Recording History View (AC: #1, #2, #3, #4)
  - [ ] Recording history view in Settings or dedicated panel
  - [ ] Display list with connection name, date/time, duration, file size
  - [ ] Play recordings (if player available)
  - [ ] Delete recordings
  - [ ] Open recording folder

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Recording history follows Architecture "Data Storage" patterns

**Technical Constraints:**
- Recording history: Store recording metadata in database
- History model: ConnectionName, RecordedAt, Duration, FileSize, FilePath
- History retention: Last 50 recordings (configurable)
- UI: Recording history view in Settings or dedicated panel
- File management: Delete recordings, open folder

### References

- [Source: docs/epics.md#Story-9.6] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-9.md#Acceptance-Criteria] - AC from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

