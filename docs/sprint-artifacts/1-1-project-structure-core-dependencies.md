# Story 1.1: Project Structure & Core Dependencies

Status: drafted

## Story

As a developer,
I want a properly structured WPF project with core dependencies installed,
so that I can build the application foundation.

## Acceptance Criteria

1. **AC1:** WPF project created with .NET 8.0 target framework, builds successfully without errors
2. **AC2:** Project folder structure matches Architecture Specification: Views/, ViewModels/, Models/, Services/, Infrastructure/, Commands/, Converters/, Helpers/, Resources/, Tests/
3. **AC3:** Core NuGet packages installed: Microsoft.Extensions.DependencyInjection, Microsoft.Extensions.Logging, Microsoft.Extensions.Configuration, CommunityToolkit.Mvvm, Microsoft.EntityFrameworkCore.Sqlite

## Tasks / Subtasks

- [ ] Task 1: Create WPF Project (AC: #1)
  - [ ] Create new WPF project using `dotnet new wpf -n Aivana_RDP_WPF -f net8.0`
  - [ ] Verify project builds successfully without errors
  - [ ] Verify target framework is net8.0 in .csproj file
- [ ] Task 2: Set Up Project Folder Structure (AC: #2)
  - [ ] Create Views/ folder
  - [ ] Create ViewModels/ folder
  - [ ] Create Models/ folder
  - [ ] Create Services/ folder
  - [ ] Create Infrastructure/ folder
  - [ ] Create Commands/ folder
  - [ ] Create Converters/ folder
  - [ ] Create Helpers/ folder
  - [ ] Create Resources/ folder
  - [ ] Create Tests/ folder
  - [ ] Verify all folders exist per Architecture spec
- [ ] Task 3: Install Core NuGet Packages (AC: #3)
  - [ ] Install Microsoft.Extensions.DependencyInjection (8.0.0)
  - [ ] Install Microsoft.Extensions.Logging (8.0.0)
  - [ ] Install Microsoft.Extensions.Configuration (8.0.0)
  - [ ] Install CommunityToolkit.Mvvm (8.2.2)
  - [ ] Install Microsoft.EntityFrameworkCore.Sqlite (8.0.0)
  - [ ] Verify packages installed via `dotnet list package`
  - [ ] Verify project builds successfully after package installation

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Project structure follows Architecture Specification section "Project Structure" exactly
- Folder hierarchy matches the documented structure in `docs/architecture.md`
- All required folders will be created at root level of project

**Technical Constraints:**
- Target framework: .NET 8.0 (as specified in Architecture ADR-001)
- WPF framework: Microsoft.WindowsDesktop.App (8.0.0)
- Project type: WPF Application (not class library)

**Package Versions:**
- All packages use version 8.0.0 to align with .NET 8.0 framework
- CommunityToolkit.Mvvm uses latest stable version (8.2.2) compatible with .NET 8.0
- Entity Framework Core uses SQLite provider version 8.0.0

**Project Initialization:**
- No standard starter template available for WPF desktop applications
- Project initialization done manually via `dotnet new wpf` command
- First implementation story executes project creation and basic setup

### References

- [Source: docs/architecture.md#Project-Structure] - Project folder structure specification
- [Source: docs/architecture.md#Technology-Stack-Details] - NuGet package requirements
- [Source: docs/epics.md#Story-1.1] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-1.md#Dependencies-and-Integrations] - Detailed package dependencies
- [Source: docs/sprint-artifacts/tech-spec-epic-1.md#Acceptance-Criteria] - AC1, AC2, AC3 from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

