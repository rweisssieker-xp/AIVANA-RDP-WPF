# Story 2.13: Session Timeout Configuration

Status: drafted

## Story

As a user,
I want to configure session timeout and idle disconnect settings,
so that my connections behave according to my security preferences.

## Acceptance Criteria

1. **AC1:** Users can set: idle timeout (minutes), maximum session duration (hours), disconnect on timeout (yes/no)
2. **AC2:** Settings saved per connection profile
3. **AC3:** Default values: idle timeout 30 minutes, max duration 8 hours, disconnect on timeout enabled
4. **AC4:** Warning notification shown before idle timeout
5. **AC5:** Connection disconnects after idle timeout (if enabled)
6. **AC6:** Connection disconnects when maximum session duration reached

## Tasks / Subtasks

- [ ] Task 1: Add Timeout Settings to Connection Dialog (AC: #1, #2, #3)
  - [ ] Add timeout settings section to ConnectionConfigDialog
  - [ ] Add idle timeout input (minutes)
  - [ ] Add maximum session duration input (hours)
  - [ ] Add disconnect on timeout checkbox
  - [ ] Set default values
- [ ] Task 2: Store Timeout Settings (AC: #2)
  - [ ] Store settings in ConnectionProfile.Settings JSON
- [ ] Task 3: Implement Idle Detection (AC: #4, #5)
  - [ ] Monitor last user activity (mouse/keyboard input)
  - [ ] Show Windows notification before timeout
  - [ ] Disconnect connection after idle timeout (if enabled)
- [ ] Task 4: Implement Maximum Duration Tracking (AC: #6)
  - [ ] Track session start time
  - [ ] Disconnect when maximum duration reached
  - [ ] Show notification before disconnection

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Timeout tracking follows Architecture "Session Management" patterns
- Timer implementation uses System.Timers.Timer or DispatcherTimer

**Technical Constraints:**
- Idle detection: Monitor last user activity (mouse/keyboard input)
- Timer implementation: Use System.Timers.Timer or DispatcherTimer for timeout tracking
- RDP control: Use RDP session timeout properties if available

### References

- [Source: docs/epics.md#Story-2.13] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-2.md#Acceptance-Criteria] - AC from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

