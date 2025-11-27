# Story 4.1: Dark Mode & Light Mode Themes

Status: drafted

## Story

As a user,
I want to switch between dark mode and light mode,
so that I can use the interface comfortably in different lighting conditions.

## Acceptance Criteria

1. **AC1:** Theme matches Windows system preference by default
2. **AC2:** Users can switch between dark mode and light mode via Settings or theme toggle button
3. **AC3:** Theme change applies immediately to all UI elements
4. **AC4:** Theme preference saved and persists after restart
5. **AC5:** All UI components use Fluent Design theme colors (see UX spec section 3.1)

## Tasks / Subtasks

- [ ] Task 1: Create Theme Resources (AC: #5)
  - [ ] Create LightTheme.xaml ResourceDictionary
  - [ ] Create DarkTheme.xaml ResourceDictionary
  - [ ] Use Fluent Design theme colors (UX spec section 3.1)
- [ ] Task 2: Implement Theme Detection (AC: #1)
  - [ ] Detect Windows system theme via Windows.UI.Settings
- [ ] Task 3: Implement Theme Switching (AC: #2, #3, #4)
  - [ ] Toggle ResourceDictionary in App.xaml
  - [ ] Add theme toggle button or Settings option
  - [ ] Save preference in ApplicationSettings
  - [ ] Apply theme immediately on change

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Theme implementation follows Architecture "UI Framework" section
- Color system follows UX spec section 3.1 color palette

**Technical Constraints:**
- Theme detection: Detect Windows system theme via Windows.UI.Settings
- Theme switching: Toggle ResourceDictionary in App.xaml
- Theme storage: Save preference in ApplicationSettings

### References

- [Source: docs/epics.md#Story-4.1] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-4.md#Acceptance-Criteria] - AC from tech spec
- [Source: docs/ux-design-specification.md#Section-3.1] - Color palette

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

