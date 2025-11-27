# Story 1.6: Configuration & Settings Infrastructure

Status: drafted

## Story

As a developer,
I want application configuration and settings management,
so that application behavior can be configured without code changes.

## Acceptance Criteria

1. **AC15:** appsettings.json exists in project root with default configuration
2. **AC16:** appsettings.Development.json exists for development overrides
3. **AC17:** ApplicationSettings model exists in Models folder
4. **AC18:** IOptions<ApplicationSettings> pattern used for settings access
5. **AC19:** User settings can be stored in %AppData%\Aivana_RDP_WPF\appsettings.json

## Tasks / Subtasks

- [ ] Task 1: Create Configuration Files (AC: #15, #16)
  - [ ] Create appsettings.json in project root with default configuration
  - [ ] Create appsettings.Development.json for development overrides
  - [ ] Configure JSON configuration provider
  - [ ] Verify file exists with valid JSON structure
  - [ ] Verify appsettings.Development.json can override appsettings.json
- [ ] Task 2: Create ApplicationSettings Model (AC: #17)
  - [ ] Create Models/ApplicationSettings.cs
  - [ ] Add properties: DefaultConnectionSettings, UIPreferences, UpdatePreferences, PerformanceSettings
  - [ ] Configure JSON serialization
  - [ ] Verify model exists with required properties
- [ ] Task 3: Configure IOptions Pattern (AC: #18)
  - [ ] Configure IOptions<ApplicationSettings> in DI container
  - [ ] Load configuration from appsettings.json
  - [ ] Unit test: IOptions resolves ApplicationSettings
- [ ] Task 4: Configure User Settings (AC: #19)
  - [ ] Configure user settings file location: %AppData%\Aivana_RDP_WPF\appsettings.json
  - [ ] Implement user settings override logic
  - [ ] Verify user settings file can be read/written

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Configuration follows Architecture "Configuration" section
- Settings model location matches Architecture "Project Structure"
- IOptions pattern matches Architecture requirements

**Technical Constraints:**
- User settings override: %AppData%\Aivana_RDP_WPF\appsettings.json
- Configuration hierarchy: default → user override
- Use Microsoft.Extensions.Configuration framework

### References

- [Source: docs/architecture.md#Configuration] - Configuration patterns
- [Source: docs/epics.md#Story-1.6] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-1.md#Dependencies-and-Integrations] - Configuration dependencies
- [Source: docs/sprint-artifacts/tech-spec-epic-1.md#Acceptance-Criteria] - AC15, AC16, AC17, AC18, AC19 from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

