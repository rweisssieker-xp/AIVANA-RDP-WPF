# Story 3.4: Auto-Reconnect Functionality

Status: drafted

## Story

As a user,
I want dropped connections to automatically reconnect,
so that I don't lose my work when network interruptions occur.

## Acceptance Criteria

1. **AC1:** System detects disconnection
2. **AC2:** Auto-reconnect attempts begin automatically
3. **AC3:** Reconnect status shown: "Reconnecting... (attempt 1 of 3)"
4. **AC4:** Retry logic uses exponential backoff: 2s, 4s, 8s delays
5. **AC5:** Maximum 3 retry attempts (configurable)
6. **AC6:** Session state restored if reconnection succeeds
7. **AC7:** Error dialog appears with manual reconnect option if all retries fail
8. **AC8:** Users can cancel auto-reconnect

## Tasks / Subtasks

- [ ] Task 1: Implement Disconnection Detection (AC: #1)
  - [ ] Monitor RDP control OnDisconnected event
- [ ] Task 2: Implement Retry Logic (AC: #2, #4, #5)
  - [ ] Implement exponential backoff (2s, 4s, 8s) with CancellationToken
  - [ ] Maximum retries: Configurable in ConnectionProfile.Settings (default: 3)
- [ ] Task 3: Session State Restoration (AC: #6)
  - [ ] Attempt to restore session state after reconnection
- [ ] Task 3: UI for Reconnect Status (AC: #3, #8)
  - [ ] Show reconnect status in connection tab or status bar
  - [ ] Allow user to cancel auto-reconnect attempts
- [ ] Task 4: Error Handling (AC: #7)
  - [ ] Show error dialog with manual reconnect button if reconnection fails

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Retry logic follows Architecture section "Error Handling" for retry patterns

**Technical Constraints:**
- Retry logic: Exponential backoff (2s, 4s, 8s) with CancellationToken
- Maximum retries: Configurable in ConnectionProfile.Settings (default: 3)

### References

- [Source: docs/epics.md#Story-3.4] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-3.md#Acceptance-Criteria] - AC from tech spec
- [Source: docs/architecture.md#Error-Handling] - Retry patterns

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

