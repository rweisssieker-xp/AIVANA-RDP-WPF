# Story 2.5: Import Connection Profiles

Status: drafted

## Story

As a user,
I want to import connection profiles from files,
so that I can migrate from other RDP clients or share connections with my team.

## Acceptance Criteria

1. **AC1:** Users can import .rdp, .json, or .csv files
2. **AC2:** RDP file parser maps standard properties correctly
3. **AC3:** JSON file parser validates structure matches ConnectionProfile model
4. **AC4:** CSV file parser maps columns correctly with header detection
5. **AC5:** Summary dialog shows import results (X successful, Y errors)
6. **AC6:** Imported profiles appear in connection list

## Tasks / Subtasks

- [ ] Task 1: Implement ImportProfilesAsync Method (AC: #1, #5, #6)
  - [ ] Add ImportProfilesAsync to IConnectionProfileService
  - [ ] Implement file picker (OpenFileDialog)
  - [ ] Route to appropriate parser based on file extension
  - [ ] Create summary dialog with import results
  - [ ] Handle duplicate profiles (ask overwrite/skip)
- [ ] Task 2: Implement RDP File Parser (AC: #2)
  - [ ] Parse .rdp file format (key-value pairs)
  - [ ] Map standard RDP properties: server address, port, username, resolution, color depth
  - [ ] Preserve custom properties in Settings JSON field
- [ ] Task 3: Implement JSON File Parser (AC: #3)
  - [ ] Deserialize JSON to List<ConnectionProfile>
  - [ ] Validate JSON structure matches ConnectionProfile model
  - [ ] Report validation errors
- [ ] Task 4: Implement CSV File Parser (AC: #4)
  - [ ] Parse CSV format (use CsvHelper or manual parsing)
  - [ ] Detect header row automatically
  - [ ] Map columns: Name, ServerAddress, Port, Username, Domain
- [ ] Task 5: Error Handling (AC: #5)
  - [ ] Show InfoBar with import results
  - [ ] Log errors for debugging
  - [ ] Handle file format errors gracefully

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- File parsing follows Architecture "Data Import/Export" patterns
- Error handling follows Architecture "Error Handling" section

**Technical Constraints:**
- RDP file format: Key-value pairs (see NFR30)
- CSV parsing: Use CsvHelper library or manual parsing
- File picker: Use Microsoft.Win32.OpenFileDialog

### References

- [Source: docs/epics.md#Story-2.5] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-2.md#Acceptance-Criteria] - AC from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

