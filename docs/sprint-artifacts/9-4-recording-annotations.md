# Story 9.4: Recording Annotations

Status: drafted

## Story

As a user,
I want to add annotations to recorded sessions,
so that I can highlight important moments.

## Acceptance Criteria

1. **AC1:** Annotation marker added at current timestamp
2. **AC2:** Annotation text saved with timestamp
3. **AC3:** Annotations appear in recording playback (if supported)
4. **AC4:** Annotations saved in metadata file

## Tasks / Subtasks

- [ ] Task 1: Implement Annotation Storage (AC: #1, #2, #4)
  - [ ] Store annotations with timestamps
  - [ ] Save in separate metadata file (JSON) or embed in video
- [ ] Task 2: Create Annotation UI (AC: #1, #2)
  - [ ] Annotation button/panel during recording
  - [ ] Add annotation marker at current timestamp
  - [ ] Save annotation text with timestamp
- [ ] Task 3: Playback Integration (AC: #3)
  - [ ] Annotations appear in recording playback (if supported)

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Annotations follow Architecture "Session Recording" section

**Technical Constraints:**
- Annotations: Store annotations with timestamps
- Annotation storage: Save in separate metadata file (JSON) or embed in video
- Annotation UI: Annotation button/panel during recording
- Metadata: Store annotations with recording file

### References

- [Source: docs/epics.md#Story-9.4] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-9.md#Acceptance-Criteria] - AC from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

