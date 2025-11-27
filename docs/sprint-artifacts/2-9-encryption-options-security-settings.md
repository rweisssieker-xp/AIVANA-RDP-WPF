# Story 2.9: Encryption Options & Security Settings

Status: drafted

## Story

As a user,
I want to configure encryption options for my connections,
so that I can ensure secure communication with remote servers.

## Acceptance Criteria

1. **AC1:** Users can configure RDP security level (RDP, Negotiate, TLS)
2. **AC2:** Users can configure encryption level (Low, Client Compatible, High, FIPS)
3. **AC3:** Users can enable/disable NLA (Network Level Authentication)
4. **AC4:** Settings saved per connection profile
5. **AC5:** Default settings use highest security (TLS, High encryption, NLA enabled)
6. **AC6:** Encryption method logged for audit purposes

## Tasks / Subtasks

- [ ] Task 1: Add Security Settings to Connection Dialog (AC: #1, #2, #3)
  - [ ] Add security settings section to ConnectionConfigDialog
  - [ ] Add RDP security level dropdown
  - [ ] Add encryption level dropdown
  - [ ] Add NLA checkbox
- [ ] Task 2: Store Security Settings (AC: #4, #5)
  - [ ] Store settings in ConnectionProfile.Settings JSON
  - [ ] Set default values: Highest security (TLS, High encryption, NLA)
- [ ] Task 3: Apply Security Settings on Connection (AC: #1, #2, #3)
  - [ ] Use MSTSC ActiveX control security properties
  - [ ] Set SecuritySettings, EncryptionLevel, NegotiateSecurityLayer
  - [ ] Handle fallback if server doesn't support requested level
- [ ] Task 4: Audit Logging (AC: #6)
  - [ ] Log encryption method used (see Story 2.10)

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- RDP security follows Architecture ADR-004 for RDP implementation
- Settings storage follows Architecture "Configuration" patterns

**Technical Constraints:**
- RDP control properties: Set SecuritySettings, EncryptionLevel, NegotiateSecurityLayer
- Default values: Highest security (TLS, High encryption, NLA)
- Settings storage: ConnectionProfile.Settings JSON

### References

- [Source: docs/epics.md#Story-2.9] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-2.md#Acceptance-Criteria] - AC from tech spec
- [Source: docs/architecture.md#ADR-004] - RDP implementation details

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

