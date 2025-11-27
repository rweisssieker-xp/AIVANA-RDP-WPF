# Story 3.5: Connection-Specific Settings Application

Status: drafted

## Story

As a user,
I want connection-specific settings applied when I connect,
so that each remote desktop uses its preferred configuration.

## Acceptance Criteria

1. **AC1:** RDP session uses: resolution, color depth, audio settings, multi-monitor configuration from profile
2. **AC2:** Settings applied before connection is established
3. **AC3:** Fallback settings used if settings conflict with server capabilities
4. **AC4:** Applied settings logged for debugging
5. **AC5:** User notified if resolution was adjusted

## Tasks / Subtasks

- [ ] Task 1: Apply Settings Before Connection (AC: #1, #2)
  - [ ] Apply ConnectionProfile.Settings before RDP connection
  - [ ] Set RDP control properties: DesktopWidth, DesktopHeight, ColorDepth, AudioCaptureMode
- [ ] Task 2: Implement Resolution Fallback (AC: #3, #5)
  - [ ] Query server for supported resolutions
  - [ ] Use closest match if requested resolution not supported
  - [ ] Notify user if resolution was adjusted
- [ ] Task 3: Logging (AC: #4)
  - [ ] Log applied settings for debugging

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Settings application follows Architecture "RDP Configuration" patterns
- Multi-monitor settings handled in Epic 8

**Technical Constraints:**
- Settings storage: ConnectionProfile.Settings JSON field (see Story 2.1)
- Resolution fallback: Query server for supported resolutions, use closest match

### References

- [Source: docs/epics.md#Story-3.5] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-3.md#Acceptance-Criteria] - AC from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

