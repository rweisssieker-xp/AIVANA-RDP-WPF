# Story 1.2: MVVM Infrastructure & Dependency Injection

Status: drafted

## Story

As a developer,
I want MVVM infrastructure and dependency injection configured,
so that I can build features following consistent patterns.

## Acceptance Criteria

1. **AC4:** Dependency injection container configured in App.xaml.cs, services can be resolved via constructor injection
2. **AC5:** RelayCommand<T> and AsyncRelayCommand<T> classes implemented in Commands folder
3. **AC10:** Service interfaces registered in DI container with implementations (can be stubs initially)

## Tasks / Subtasks

- [ ] Task 1: Implement RelayCommand Classes (AC: #5)
  - [ ] Create Commands/RelayCommand.cs
  - [ ] Create Commands/AsyncRelayCommand.cs
  - [ ] Implement ICommand interface
  - [ ] Add CanExecute support
  - [ ] Unit test: RelayCommand executes action
  - [ ] Unit test: AsyncRelayCommand executes async action
- [ ] Task 2: Configure Dependency Injection (AC: #4)
  - [ ] Configure IServiceCollection in App.xaml.cs OnStartup
  - [ ] Register services as interfaces: `services.AddSingleton<IService, Service>()`
  - [ ] Build ServiceProvider
  - [ ] Set MainWindow.DataContext with injected ViewModel
  - [ ] Unit test: Service can be resolved from DI container
- [ ] Task 3: Register Service Interfaces (AC: #10)
  - [ ] Register IConnectionProfileService → ConnectionProfileService
  - [ ] Register IRdpConnectionService → RdpConnectionService
  - [ ] Register IFileTransferService → FileTransferService
  - [ ] Register IClipboardService → ClipboardService
  - [ ] Register ISessionRecordingService → SessionRecordingService
  - [ ] Register IPerformanceMonitorService → PerformanceMonitorService
  - [ ] Register ICredentialService → CredentialService
  - [ ] Register INotificationService → NotificationService
  - [ ] Verify services registered, can be resolved

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- MVVM infrastructure follows Architecture ADR-002
- Dependency injection uses Microsoft.Extensions.DependencyInjection (Architecture section "Technology Stack Details")
- Commands folder structure matches Architecture "Project Structure"

**Technical Constraints:**
- Use CommunityToolkit.Mvvm for INotifyPropertyChanged helpers
- Service registration follows interface-to-implementation pattern
- ViewModels receive services via constructor injection

### References

- [Source: docs/architecture.md#Architecture-Pattern] - MVVM pattern specification
- [Source: docs/architecture.md#Technology-Stack-Details] - Dependency injection details
- [Source: docs/epics.md#Story-1.2] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-1.md#Acceptance-Criteria] - AC4, AC5, AC10 from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

