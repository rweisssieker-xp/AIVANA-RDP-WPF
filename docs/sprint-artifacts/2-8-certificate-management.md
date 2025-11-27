# Story 2.8: Certificate Management

Status: drafted

## Story

As a user,
I want to manage SSL/TLS certificates for secure connections,
so that I can trust or reject server certificates.

## Acceptance Criteria

1. **AC1:** Certificate warning dialog appears for invalid/untrusted certificates
2. **AC2:** Users can view certificate details: issuer, expiration date, thumbprint
3. **AC3:** Users can choose: Accept once, Accept and save, or Reject
4. **AC4:** Saved certificates stored for future connections
5. **AC5:** Certificate pinning supported (NFR17)
6. **AC6:** Users can view and remove saved certificates

## Tasks / Subtasks

- [ ] Task 1: Implement Certificate Validation (AC: #1)
  - [ ] Use System.Security.Cryptography.X509Certificates
  - [ ] Validate certificates per NFR13
  - [ ] Show warnings for invalid certificates
- [ ] Task 2: Create Certificate Dialog UI (AC: #2, #3)
  - [ ] Create CertificateDialog (ContentDialog, 600px width)
  - [ ] Display certificate details: issuer, subject, expiration, thumbprint
  - [ ] Add buttons: Accept once, Accept and save, Reject
- [ ] Task 3: Implement Certificate Storage (AC: #4, #5)
  - [ ] Store certificate thumbprints in ConnectionProfile.Settings JSON
  - [ ] Implement certificate pinning per NFR17
- [ ] Task 4: Certificate Management UI (AC: #6)
  - [ ] Show saved certificates in connection settings
  - [ ] Implement remove certificate functionality

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Certificate validation follows Architecture Security section
- Certificate pinning follows NFR17 (Architecture Security section)

**Technical Constraints:**
- Certificate storage: Store thumbprints in ConnectionProfile.Settings JSON
- Certificate pinning: Implement per NFR17
- UI: CertificateDialog (ContentDialog, 600px width)

### References

- [Source: docs/epics.md#Story-2.8] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-2.md#Acceptance-Criteria] - AC from tech spec
- [Source: docs/architecture.md#Security-Architecture] - Certificate pinning requirements

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

