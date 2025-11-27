# Story 4.6: Context-Aware UI Elements

Status: drafted

## Story

As a user,
I want UI elements to adapt based on connection state,
so that I see relevant actions and information for my current context.

## Acceptance Criteria

1. **AC1:** Toolbar shows: New Connection, Import Connections, Settings when no active connections
2. **AC2:** Main content shows connection dashboard with Quick Connect prompt when no active connections
3. **AC3:** Toolbar shows: Disconnect, File Transfer, Record Session, Performance Monitor when active connection exists
4. **AC4:** Connection-specific actions enabled when active connection exists

## Tasks / Subtasks

- [ ] Task 1: Implement Context Detection (AC: #1, #2, #3, #4)
  - [ ] Track application state (no connections, active connection, etc.)
  - [ ] Use ViewModels to track application state
- [ ] Task 2: Implement UI Adaptation (AC: #1, #2, #3, #4)
  - [ ] Show/hide UI elements based on context
  - [ ] Update toolbar based on connection state
  - [ ] Update main content based on connection state

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Context-aware UI follows Architecture "UI Framework" section
- State management follows MVVM pattern

**Technical Constraints:**
- Context detection: Track application state (no connections, active connection, etc.)
- UI adaptation: Show/hide UI elements based on context
- State management: Use ViewModels to track application state

### References

- [Source: docs/epics.md#Story-4.6] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-4.md#Acceptance-Criteria] - AC from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

