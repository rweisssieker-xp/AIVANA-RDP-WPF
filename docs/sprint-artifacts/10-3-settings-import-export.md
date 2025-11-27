# Story 10.3: Settings Import/Export

Status: drafted

## Story

As a user,
I want to import/export application settings,
so that I can backup settings or migrate to another machine.

## Acceptance Criteria

1. **AC1:** Settings exported to JSON file
2. **AC2:** File saved to selected location
3. **AC3:** Success notification appears
4. **AC4:** File picker opens for import
5. **AC5:** Settings imported and applied
6. **AC6:** Confirmation dialog appears before applying imported settings

## Tasks / Subtasks

- [ ] Task 1: Implement Settings Export (AC: #1, #2, #3)
  - [ ] Serialize ApplicationSettings to JSON
  - [ ] Use SaveFileDialog for export location
  - [ ] Show success notification: InfoBar with success style
- [ ] Task 2: Implement Settings Import (AC: #4, #5, #6)
  - [ ] Use OpenFileDialog for file selection
  - [ ] Deserialize JSON to ApplicationSettings
  - [ ] Validate imported settings before applying
  - [ ] Show confirmation dialog before applying

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Import/export follows Architecture "Configuration" patterns

**Technical Constraints:**
- Settings export: Serialize ApplicationSettings to JSON
- Settings import: Deserialize JSON to ApplicationSettings
- File picker: Use OpenFileDialog/SaveFileDialog
- Validation: Validate imported settings before applying

### References

- [Source: docs/epics.md#Story-10.3] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-10.md#Acceptance-Criteria] - AC from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

