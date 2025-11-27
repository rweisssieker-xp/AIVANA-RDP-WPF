# Story 9.3: Recording Quality & Compression Settings

Status: drafted

## Story

As a user,
I want to configure recording quality and compression,
so that I can balance file size and video quality.

## Acceptance Criteria

1. **AC1:** Users can configure: quality (Low, Medium, High), frame rate (15, 30, 60 FPS), compression level
2. **AC2:** Settings saved and applied to future recordings
3. **AC3:** File size estimate shown based on settings

## Tasks / Subtasks

- [ ] Task 1: Create Recording Settings UI (AC: #1, #2, #3)
  - [ ] Add recording settings in Settings > Recording
  - [ ] Quality levels: Low (480p), Medium (720p), High (1080p)
  - [ ] Frame rate: 15, 30, 60 FPS options
  - [ ] Compression: H.264 codec with configurable bitrate
  - [ ] Store in ApplicationSettings or ConnectionProfile.Settings
  - [ ] Calculate and display file size estimate based on settings

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Recording settings follow Architecture "Configuration" patterns

**Technical Constraints:**
- Recording settings: Store in ApplicationSettings or ConnectionProfile.Settings
- Quality levels: Low (480p), Medium (720p), High (1080p)
- Frame rate: 15, 30, 60 FPS options
- Compression: H.264 codec with configurable bitrate
- File size estimation: Calculate estimated file size based on settings

### References

- [Source: docs/epics.md#Story-9.3] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-9.md#Acceptance-Criteria] - AC from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

