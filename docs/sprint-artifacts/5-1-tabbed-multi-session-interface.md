# Story 5.1: Tabbed Multi-Session Interface

Status: drafted

## Story

As a user,
I want a tabbed interface for multiple sessions,
so that I can easily switch between active connections.

## Acceptance Criteria

1. **AC1:** Tabs appear at the top showing each active session
2. **AC2:** Each tab shows: connection name, status indicator (green/yellow/red), close button
3. **AC3:** Users can click a tab to switch to that session
4. **AC4:** Active tab is highlighted
5. **AC5:** Maximum 10 tabs are supported (NFR20)

## Tasks / Subtasks

- [ ] Task 1: Create Tab Control (AC: #1, #2, #3, #4)
  - [ ] Use TabControl or custom tab implementation
  - [ ] Each tab shows connection name, status indicator, close button
  - [ ] Track active sessions and create/remove tabs dynamically
  - [ ] Highlight active tab
- [ ] Task 2: Tab Management (AC: #5)
  - [ ] Enforce maximum 10 tabs (NFR20)
  - [ ] See UX spec section 6.1 Multi-Connection Dashboard Component

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Tab control follows Architecture "UI Layer" section
- UI follows UX spec section 6.1 Multi-Connection Dashboard Component

**Technical Constraints:**
- Tab control: Use TabControl or custom tab implementation
- Maximum tabs: 10 (NFR20)

### References

- [Source: docs/epics.md#Story-5.1] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-5.md#Acceptance-Criteria] - AC from tech spec
- [Source: docs/ux-design-specification.md#Section-6.1] - Multi-Connection Dashboard Component

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

