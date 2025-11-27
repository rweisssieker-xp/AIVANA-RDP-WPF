# Story 1.4: Core Services Infrastructure

Status: drafted

## Story

As a developer,
I want core service interfaces and base implementations,
so that business logic is separated from UI concerns.

## Acceptance Criteria

1. **AC9:** All core service interfaces defined: IConnectionProfileService, IRdpConnectionService, IFileTransferService, IClipboardService, ISessionRecordingService, IPerformanceMonitorService, ICredentialService, INotificationService
2. **AC10:** Service interfaces registered in DI container with implementations (can be stubs initially)

## Tasks / Subtasks

- [ ] Task 1: Create Service Interfaces (AC: #9)
  - [ ] Create Services/IConnectionProfileService.cs with async method signatures
  - [ ] Create Services/IRdpConnectionService.cs with async method signatures
  - [ ] Create Services/IFileTransferService.cs with async method signatures
  - [ ] Create Services/IClipboardService.cs with async method signatures
  - [ ] Create Services/ISessionRecordingService.cs with async method signatures
  - [ ] Create Services/IPerformanceMonitorService.cs with async method signatures
  - [ ] Create Services/ICredentialService.cs with async method signatures
  - [ ] Create Services/INotificationService.cs with async method signatures
  - [ ] Verify all interfaces exist with async method signatures
- [ ] Task 2: Create Service Implementations (AC: #10)
  - [ ] Create stub implementations for all service interfaces
  - [ ] Implement methods with NotImplementedException initially
  - [ ] Use async/await pattern for all I/O operations
  - [ ] Register services in DI container (from Story 1.2)
  - [ ] Verify services registered, can be resolved

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Service interfaces follow Architecture "API Contracts" section
- Service location matches Architecture "Project Structure"
- Naming conventions follow Architecture "Implementation Patterns"

**Technical Constraints:**
- All service methods use async/await pattern
- Services follow interface-to-implementation pattern
- Stub implementations can throw NotImplementedException initially

### References

- [Source: docs/architecture.md#Services] - Service structure specification
- [Source: docs/epics.md#Story-1.4] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-1.md#APIs-and-Interfaces] - Service interface patterns
- [Source: docs/sprint-artifacts/tech-spec-epic-1.md#Acceptance-Criteria] - AC9, AC10 from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

