# Story 3.2: Multiple Simultaneous Connections

Status: drafted

## Story

As a user,
I want to establish multiple RDP connections simultaneously,
so that I can work with multiple remote desktops at once.

## Acceptance Criteria

1. **AC1:** New connection tab created when connecting to another server
2. **AC2:** Multiple connections active simultaneously
3. **AC3:** Users can switch between connections using tabs
4. **AC4:** Each connection maintains its own session state
5. **AC5:** Application supports at least 10 simultaneous connections (NFR20)
6. **AC6:** Users can disconnect individual connections without affecting others

## Tasks / Subtasks

- [ ] Task 1: Implement Session Management (AC: #2, #4, #5)
  - [ ] Track multiple RdpClientWrapper instances
  - [ ] Assign unique session ID to each connection
  - [ ] Store session state in SessionState model per connection
  - [ ] Enforce maximum 10 simultaneous connections (configurable, NFR20)
- [ ] Task 2: Memory Management (AC: #5)
  - [ ] Dispose RDP controls when disconnected to free resources
  - [ ] Follow Architecture section "Performance Considerations" for memory management
- [ ] Task 3: Tab Integration (AC: #1, #3, #6)
  - [ ] Create new tab when connecting (see Epic 5 for tab UI)
  - [ ] Implement tab switching
  - [ ] Implement disconnect per connection

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Session management follows Architecture "Session Management" section
- Performance follows Architecture "Performance Considerations"

**Technical Constraints:**
- Maximum connections: 10 simultaneous (configurable, NFR20)
- Memory management: Dispose RDP controls when disconnected

### References

- [Source: docs/epics.md#Story-3.2] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-3.md#Acceptance-Criteria] - AC from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

