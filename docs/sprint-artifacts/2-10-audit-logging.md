# Story 2.10: Audit Logging

Status: drafted

## Story

As a user,
I want connection events logged for audit purposes,
so that I can track security-related activities.

## Acceptance Criteria

1. **AC1:** Events logged with: timestamp, event type, connection profile ID, user context, result
2. **AC2:** Logged events include: connection attempts, successes, failures, credential access, certificate changes, settings changes
3. **AC3:** Logs stored in tamper-evident format
4. **AC4:** Logs include timestamps and user identification
5. **AC5:** Users can view audit logs (Settings > Security > Audit Logs)
6. **AC6:** Users can filter logs by: date range, event type, connection profile
7. **AC7:** Users can export logs for compliance reporting

## Tasks / Subtasks

- [ ] Task 1: Create AuditLog Model (AC: #1, #2, #4)
  - [ ] Create AuditLog entity in Models folder
  - [ ] Properties: Timestamp, EventType, ConnectionProfileId, UserContext, Result, Details
  - [ ] Add AuditLog DbSet to ApplicationDbContext
- [ ] Task 2: Implement Audit Logging Service (AC: #1, #2, #3)
  - [ ] Create IAuditLogService interface
  - [ ] Implement LogEventAsync method
  - [ ] Store logs in SQLite database or separate log files
  - [ ] Implement tamper-evident format (hash-based integrity checking)
- [ ] Task 3: Integrate Audit Logging (AC: #2)
  - [ ] Log connection attempts (success/failure)
  - [ ] Log credential access (read operations)
  - [ ] Log certificate changes
  - [ ] Log security settings changes
- [ ] Task 4: Create Audit Log Viewer UI (AC: #5, #6, #7)
  - [ ] Create audit log viewer in Settings > Security > Audit Logs
  - [ ] Display list of security events
  - [ ] Implement filtering: date range, event type, connection profile
  - [ ] Implement export functionality (CSV/JSON)

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Audit logging follows Architecture "Security Architecture" section
- Log retention: 30 days (configurable per NFR18)

**Technical Constraints:**
- Log format: Structured logging with JSON or structured fields
- Tamper-evident: Use hash-based integrity checking or read-only log files
- Log retention: 30 days (configurable per NFR18)
- UI: Audit log viewer in Settings (can be post-MVP)

### References

- [Source: docs/epics.md#Story-2.10] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-2.md#Acceptance-Criteria] - AC from tech spec
- [Source: docs/architecture.md#Security-Architecture] - Audit logging requirements

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

