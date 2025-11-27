# Story 1.2: MVVM Infrastructure & Dependency Injection

Status: done

## Story

As a developer,
I want MVVM infrastructure and dependency injection configured,
so that I can build features following consistent patterns.

## Acceptance Criteria

1. **AC4:** Dependency injection container configured in App.xaml.cs, services can be resolved via constructor injection
2. **AC5:** RelayCommand<T> and AsyncRelayCommand<T> classes implemented in Commands folder
3. **AC10:** Service interfaces registered in DI container with implementations (can be stubs initially)

## Tasks / Subtasks

- [x] Task 1: Implement RelayCommand Classes (AC: #5)
  - [x] Create Commands/RelayCommand.cs (wrapper for CommunityToolkit.Mvvm RelayCommand)
  - [x] Create Commands/AsyncRelayCommand.cs (wrapper for CommunityToolkit.Mvvm AsyncRelayCommand)
  - [x] Implement ICommand interface
  - [x] Add CanExecute support
  - [ ] Unit test: RelayCommand executes action (deferred to test story)
  - [ ] Unit test: AsyncRelayCommand executes async action (deferred to test story)
- [x] Task 2: Configure Dependency Injection (AC: #4)
  - [x] Configure IServiceCollection in App.xaml.cs OnStartup
  - [x] Register services as interfaces: `services.AddSingleton<IService, Service>()`
  - [x] Build ServiceProvider
  - [x] Set MainWindow.DataContext with injected ViewModel
  - [ ] Unit test: Service can be resolved from DI container (deferred to test story)
- [x] Task 3: Register Service Interfaces (AC: #10)
  - [x] Register IConnectionProfileService → ConnectionProfileService
  - [x] Register IRdpConnectionService → RdpConnectionService
  - [x] Register IFileTransferService → FileTransferService
  - [x] Register IClipboardService → ClipboardService
  - [x] Register ISessionRecordingService → SessionRecordingService
  - [x] Register IPerformanceMonitorService → PerformanceMonitorService
  - [x] Register ICredentialService → CredentialService
  - [x] Register INotificationService → NotificationService
  - [x] Verify services registered, can be resolved

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

**Completed:** 2025-11-27
- RelayCommand and AsyncRelayCommand wrappers created (using CommunityToolkit.Mvvm internally)
- Dependency injection configured in App.xaml.cs OnStartup
- All service interfaces and stub implementations created
- All services registered in DI container
- MainWindow receives ViewModel via constructor injection
- Project builds successfully

### File List

- `Aivana_RDP_WPF/Commands/RelayCommand.cs` - Synchronous command wrapper
- `Aivana_RDP_WPF/Commands/AsyncRelayCommand.cs` - Asynchronous command wrapper
- `Aivana_RDP_WPF/App.xaml.cs` - DI configuration
- `Aivana_RDP_WPF/MainWindow.xaml.cs` - ViewModel injection
- `Aivana_RDP_WPF/ViewModels/MainViewModel.cs` - Main ViewModel
- `Aivana_RDP_WPF/Services/*.cs` - All service interfaces and implementations

