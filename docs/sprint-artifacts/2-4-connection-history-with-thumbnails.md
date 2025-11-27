# Story 2.4: Connection History with Thumbnails

Status: drafted

## Story

As a user,
I want to view my connection history with visual thumbnails,
so that I can quickly reconnect to recently used sessions.

## Acceptance Criteria

1. **AC1:** List of recent sessions displayed with connection name, server address, last connected time
2. **AC2:** Thumbnail image of remote desktop (captured at connection time)
3. **AC3:** Users can click history item to reconnect
4. **AC4:** History limited to last 50 sessions
5. **AC5:** Empty state shown when no history exists

## Tasks / Subtasks

- [ ] Task 1: Implement SessionHistoryRepository (AC: #1, #4)
  - [ ] Create ISessionHistoryRepository interface
  - [ ] Implement GetRecentSessionsAsync(limit: 50) method
  - [ ] Use SessionHistory model from Story 1.3
- [ ] Task 2: Capture Thumbnails (AC: #2)
  - [ ] Implement thumbnail capture on connection
  - [ ] Take screenshot of RDP session when connected
  - [ ] Store thumbnail as base64 or file path
  - [ ] Save to %AppData%\Aivana_RDP_WPF\Thumbnails\{SessionId}.jpg
- [ ] Task 3: Create History View UI (AC: #1, #3, #5)
  - [ ] Create history view in sidebar or dedicated panel
  - [ ] Display session list with thumbnails
  - [ ] Format dates using DateTimeToRelativeTimeConverter
  - [ ] Implement empty state per UX spec section 7.1
  - [ ] Implement reconnect on click

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- SessionHistory model already defined in Story 1.3
- Thumbnail storage follows Architecture "File Storage" patterns
- Empty state follows UX spec section 7.1 Empty State Patterns

**Technical Constraints:**
- Thumbnail storage: %AppData%\Aivana_RDP_WPF\Thumbnails\{SessionId}.jpg
- History limit: Last 50 sessions (configurable)
- Date formatting: Use DateTimeToRelativeTimeConverter

### References

- [Source: docs/epics.md#Story-2.4] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-2.md#Acceptance-Criteria] - AC from tech spec
- [Source: docs/ux-design-specification.md#Section-7.1] - Empty State Patterns

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

