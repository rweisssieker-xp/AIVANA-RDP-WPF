# Story 1.5: Logging Infrastructure

Status: drafted

## Story

As a developer,
I want structured logging configured throughout the application,
so that I can debug issues and monitor application behavior.

## Acceptance Criteria

1. **AC11:** ILogger<T> can be injected into services and ViewModels
2. **AC12:** Log files written to %AppData%\Aivana_RDP_WPF\Logs\aivana-{Date}.log
3. **AC13:** Console logging enabled for development environment
4. **AC14:** Structured logging pattern used: `_logger.LogInformation("Message {Parameter}", value)`

## Tasks / Subtasks

- [ ] Task 1: Configure Logging (AC: #11, #13)
  - [ ] Configure logging in App.xaml.cs or LoggerConfiguration.cs
  - [ ] Add console logging provider for development
  - [ ] Add file logging provider
  - [ ] Configure log levels (Trace, Debug, Information, Warning, Error, Critical)
  - [ ] Unit test: Service receives ILogger via constructor injection
  - [ ] Verify console logging enabled in development
- [ ] Task 2: Configure File Logging (AC: #12)
  - [ ] Configure file logging provider
  - [ ] Set log file location: %AppData%\Aivana_RDP_WPF\Logs\aivana-{Date}.log
  - [ ] Create Logs directory if it doesn't exist
  - [ ] Configure log retention (30 days)
  - [ ] Verify log file created in correct location
- [ ] Task 3: Implement Structured Logging (AC: #14)
  - [ ] Use ILogger<T> pattern for typed logging
  - [ ] Use structured logging: `_logger.LogInformation("Message {Parameter}", value)`
  - [ ] Add logging examples in service implementations
  - [ ] Verify structured logging pattern used in code

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Logging follows Architecture "Logging Strategy" section
- Log file location matches Architecture specification
- Structured logging pattern matches Architecture requirements

**Technical Constraints:**
- Log file location: %AppData%\Aivana_RDP_WPF\Logs\aivana-{Date}.log
- Log retention: 30 days (configurable)
- Use Microsoft.Extensions.Logging framework

### References

- [Source: docs/architecture.md#Logging-Strategy] - Logging requirements and patterns
- [Source: docs/epics.md#Story-1.5] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-1.md#Observability] - Logging requirements
- [Source: docs/sprint-artifacts/tech-spec-epic-1.md#Acceptance-Criteria] - AC11, AC12, AC13, AC14 from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

