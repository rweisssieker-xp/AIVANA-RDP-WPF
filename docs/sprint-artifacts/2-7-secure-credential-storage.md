# Story 2.7: Secure Credential Storage

Status: drafted

## Story

As a user,
I want my credentials stored securely,
so that my passwords are protected and I don't have to enter them every time.

## Acceptance Criteria

1. **AC1:** Passwords stored in Windows Credential Manager (not in database)
2. **AC2:** Credentials encrypted by Windows
3. **AC3:** Username stored in ConnectionProfile (for display)
4. **AC4:** Password never stored in plain text
5. **AC5:** Credentials retrieved from Credential Manager on connection
6. **AC6:** Credentials removed from Credential Manager on profile deletion

## Tasks / Subtasks

- [ ] Task 1: Implement ICredentialService (AC: #1, #2, #5, #6)
  - [ ] Create ICredentialService interface
  - [ ] Wrap Windows Credential Manager API (CredWrite, CredRead, CredDelete)
  - [ ] Use credential target format: `Aivana_RDP_WPF:{ConnectionProfileId}`
  - [ ] Handle credential access errors gracefully
- [ ] Task 2: Update ConnectionProfileService (AC: #3, #4)
  - [ ] Store username in ConnectionProfile
  - [ ] Never store password in database
  - [ ] Use ICredentialService for password operations
- [ ] Task 3: Integrate with Connection Flow (AC: #5)
  - [ ] Retrieve credentials from Credential Manager on connection
  - [ ] Prompt for credentials if missing
- [ ] Task 4: Integrate with Deletion Flow (AC: #6)
  - [ ] Remove credentials from Credential Manager on profile deletion
  - [ ] Verify credentials cannot be recovered

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Security follows Architecture ADR-005 (Windows Credential Manager)
- Security follows NFR11, NFR16 (credentials encrypted by Windows)

**Technical Constraints:**
- Use Windows API: CredWrite, CredRead, CredDelete
- Credential target format: `Aivana_RDP_WPF:{ConnectionProfileId}`
- Security: Follow NFR11, NFR16 - credentials encrypted by Windows

### References

- [Source: docs/epics.md#Story-2.7] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-2.md#Acceptance-Criteria] - AC from tech spec
- [Source: docs/architecture.md#Security-Architecture] - Windows Credential Manager implementation

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

