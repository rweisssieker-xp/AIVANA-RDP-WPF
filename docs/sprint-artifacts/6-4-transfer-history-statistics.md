# Story 6.4: Transfer History & Statistics

Status: drafted

## Story

As a user,
I want to view transfer history and statistics,
so that I can track my file transfer activity.

## Acceptance Criteria

1. **AC1:** List of past transfers with: file name, source, destination, size, date/time, duration, status
2. **AC2:** Users can filter history by: date range, connection, status
3. **AC3:** History shows last 100 transfers (configurable)

## Tasks / Subtasks

- [ ] Task 1: Store Transfer History (AC: #1, #3)
  - [ ] Store FileTransferItem records in database or log files
  - [ ] History retention: Last 100 transfers (configurable)
- [ ] Task 2: Create Transfer History View (AC: #1, #2)
  - [ ] Create transfer history view in Settings or dedicated panel
  - [ ] Display list with file name, source, destination, size, date/time, duration, status
  - [ ] Implement filtering: date range, connection, status

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Transfer history follows Architecture "Data Storage" patterns

**Technical Constraints:**
- Transfer history: Store FileTransferItem records in database or log files
- History retention: Last 100 transfers (configurable)
- UI: Transfer history view in Settings or dedicated panel

### References

- [Source: docs/epics.md#Story-6.4] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-6.md#Acceptance-Criteria] - AC from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

