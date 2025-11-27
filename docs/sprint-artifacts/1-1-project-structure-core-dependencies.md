# Story 1.1: Project Structure & Core Dependencies

Status: done

## Story

As a developer,
I want a properly structured WPF project with core dependencies installed,
so that I can build the application foundation.

## Acceptance Criteria

1. **AC1:** WPF project created with .NET 8.0 target framework, builds successfully without errors
2. **AC2:** Project folder structure matches Architecture Specification: Views/, ViewModels/, Models/, Services/, Infrastructure/, Commands/, Converters/, Helpers/, Resources/, Tests/
3. **AC3:** Core NuGet packages installed: Microsoft.Extensions.DependencyInjection, Microsoft.Extensions.Logging, Microsoft.Extensions.Configuration, CommunityToolkit.Mvvm, Microsoft.EntityFrameworkCore.Sqlite

## Tasks / Subtasks

- [x] Task 1: Create WPF Project (AC: #1)
  - [x] Create new WPF project using `dotnet new wpf -n Aivana_RDP_WPF -f net8.0`
  - [x] Verify project builds successfully without errors
  - [x] Verify target framework is net8.0 in .csproj file
- [x] Task 2: Set Up Project Folder Structure (AC: #2)
  - [x] Create Views/ folder
  - [x] Create ViewModels/ folder
  - [x] Create Models/ folder
  - [x] Create Services/ folder
  - [x] Create Infrastructure/ folder
  - [x] Create Commands/ folder
  - [x] Create Converters/ folder
  - [x] Create Helpers/ folder
  - [x] Create Resources/ folder
  - [x] Create Tests/ folder (already exists)
  - [x] Verify all folders exist per Architecture spec
- [x] Task 3: Install Core NuGet Packages (AC: #3)
  - [x] Install Microsoft.Extensions.DependencyInjection (8.0.0)
  - [x] Install Microsoft.Extensions.Logging (8.0.0)
  - [x] Install Microsoft.Extensions.Configuration (8.0.0)
  - [x] Install CommunityToolkit.Mvvm (8.2.2)
  - [x] Install Microsoft.EntityFrameworkCore.Sqlite (8.0.0)
  - [x] Verify packages installed via `dotnet list package`
  - [x] Verify project builds successfully after package installation

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

**Completed:** 2025-11-27
- WPF project created successfully with .NET 8.0 target framework
- All required folders created: Views/, ViewModels/, Models/, Services/, Infrastructure/, Commands/, Converters/, Helpers/, Resources/
- All core NuGet packages installed and verified
- Project builds successfully without errors
- Solution file created and both projects added

### File List

- `Aivana_RDP_WPF/Aivana_RDP_WPF.csproj` - Main project file
- `Aivana_RDP_WPF/App.xaml` - Application entry point
- `Aivana_RDP_WPF/MainWindow.xaml` - Main window
- `Aivana_RDP_WPF.sln` - Solution file

