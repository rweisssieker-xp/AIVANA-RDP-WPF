# Story 5.2: Session Switching & Management

Status: drafted

## Story

As a user,
I want to switch between sessions and manage them,
so that I can efficiently work with multiple remote desktops.

## Acceptance Criteria

1. **AC1:** Session switches instantly (< 100ms)
2. **AC2:** Session state preserved (no reconnection needed)
3. **AC3:** Ctrl+Tab keyboard shortcut cycles through tabs
4. **AC4:** Ctrl+W keyboard shortcut closes tab

## Tasks / Subtasks

- [ ] Task 1: Implement Session Switching (AC: #1, #2)
  - [ ] Instant switch by showing/hiding RDP controls
  - [ ] Maintain RDP control instances for each session
- [ ] Task 2: Implement Keyboard Shortcuts (AC: #3, #4)
  - [ ] Ctrl+Tab for tab switching
  - [ ] Ctrl+W for closing tab

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Session switching follows Architecture "Session Management" section
- Performance follows NFR (session switching < 100ms)

**Technical Constraints:**
- Session switching: Instant switch by showing/hiding RDP controls
- Session state: Maintain RDP control instances for each session
- Keyboard shortcuts: Ctrl+Tab for tab switching, Ctrl+W for closing tab

### References

- [Source: docs/epics.md#Story-5.2] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-5.md#Acceptance-Criteria] - AC from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

