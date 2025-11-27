# Story 4.2: Multiple Built-in Themes

Status: drafted

## Story

As a user,
I want to choose from multiple built-in themes,
so that I can personalize the application appearance.

## Acceptance Criteria

1. **AC1:** Users can choose from: Professional Blue (default), Dark Professional, Light Minimal, High Contrast
2. **AC2:** Theme preview shown before applying
3. **AC3:** Theme applies immediately when selected
4. **AC4:** Theme preference saved
5. **AC5:** High Contrast mode supported (NFR24)

## Tasks / Subtasks

- [ ] Task 1: Create Theme Variants (AC: #1, #5)
  - [ ] Create multiple ResourceDictionary files for each theme
  - [ ] Professional Blue (default)
  - [ ] Dark Professional
  - [ ] Light Minimal
  - [ ] High Contrast (support Windows High Contrast mode, NFR24)
- [ ] Task 2: Create Theme Selection UI (AC: #2, #3, #4)
  - [ ] Add theme selection dropdown or radio buttons in Settings
  - [ ] Show theme preview before applying
  - [ ] Apply theme immediately when selected
  - [ ] Save theme preference

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Theme variants follow Architecture "UI Framework" section
- High Contrast support follows NFR24 (accessibility)

**Technical Constraints:**
- High Contrast: Support Windows High Contrast mode (NFR24)
- Theme selection: Dropdown or radio buttons in Settings

### References

- [Source: docs/epics.md#Story-4.2] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-4.md#Acceptance-Criteria] - AC from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

