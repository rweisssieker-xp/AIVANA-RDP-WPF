# Story 2.6: Export Connection Profiles

Status: drafted

## Story

As a user,
I want to export connection profiles to files,
so that I can backup my connections or share them with my team.

## Acceptance Criteria

1. **AC1:** Users can choose export format: RDP files, JSON, or CSV
2. **AC2:** Users can select which profiles to export (all, selected, or by group)
3. **AC3:** RDP format: Each profile saved as separate .rdp file
4. **AC4:** JSON format: All profiles saved in single JSON array
5. **AC5:** CSV format: All profiles saved in CSV with headers
6. **AC6:** Credentials NOT exported (security requirement)
7. **AC7:** Success notification appears

## Tasks / Subtasks

- [ ] Task 1: Implement ExportProfilesAsync Method (AC: #1, #2, #7)
  - [ ] Add ExportProfilesAsync to IConnectionProfileService
  - [ ] Implement profile selection (all, selected, by group)
  - [ ] Route to appropriate exporter based on format
  - [ ] Show success notification (InfoBar with success style)
- [ ] Task 2: Implement RDP Exporter (AC: #3)
  - [ ] Generate .rdp file content (key-value pairs)
  - [ ] Save each profile as separate .rdp file
  - [ ] Use connection name (sanitized) as filename
  - [ ] Verify .rdp files can be opened by standard Windows RDP client
- [ ] Task 3: Implement JSON Exporter (AC: #4)
  - [ ] Serialize List<ConnectionProfile> to JSON
  - [ ] Exclude credentials from export
  - [ ] Save to single JSON file
- [ ] Task 4: Implement CSV Exporter (AC: #5)
  - [ ] Generate CSV with headers
  - [ ] Exclude credentials from CSV
  - [ ] Save to single CSV file
- [ ] Task 5: Security: Credential Exclusion (AC: #6)
  - [ ] Never export passwords or credentials (NFR11)
  - [ ] Verify credentials excluded from all formats

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Export follows Architecture "Data Import/Export" patterns
- Security follows NFR11 (credentials never exported)

**Technical Constraints:**
- Credential exclusion: Never export passwords (security requirement NFR11)
- File picker: Use Microsoft.Win32.SaveFileDialog
- Success notification: InfoBar with success style (green, auto-dismiss 3s)

### References

- [Source: docs/epics.md#Story-2.6] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-2.md#Acceptance-Criteria] - AC from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

