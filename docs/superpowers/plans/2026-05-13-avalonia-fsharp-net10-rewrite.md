# Avalonia F# .NET 10 Rewrite Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the legacy WPF/C# RDP client with a fresh Avalonia/F# desktop application targeting .NET 10.

**Architecture:** Create a new F# Avalonia MVVM application with focused domain models, an in-memory/sample-backed shell, and JSON persistence for connection profiles. Keep the first version small: connection overview, quick connect, health/status, settings, and tests.

**Tech Stack:** .NET 10 SDK, F#, Avalonia MVVM, xUnit/F# test project.

---

### Task 1: Replace Legacy Project Structure

**Files:**
- Delete: `Aivana_RDP_WPF`
- Delete: `Aivana_RDP_WPF.Tests`
- Delete: `Aivana_RDP_WPF.sln`
- Create: `src/Aivana.App`
- Create: `tests/Aivana.App.Tests`
- Create: `Aivana.sln`

- [ ] Remove the old WPF app and test project with PowerShell `Remove-Item` after verifying the paths are inside `C:\tmp\AIVANA-RDP-WPF`.
- [ ] Scaffold `src/Aivana.App` using `dotnet new avalonia.mvvm -lang F#`.
- [ ] Change the generated app target framework to `net10.0`.
- [ ] Create `tests/Aivana.App.Tests` as an F# xUnit project targeting `net10.0`.
- [ ] Add both projects to `Aivana.sln`.
- [ ] Run `dotnet restore Aivana.sln`.

### Task 2: Build the New MVP UI

**Files:**
- Modify: `src/Aivana.App/Models.fs`
- Modify: `src/Aivana.App/ViewModels/MainWindowViewModel.fs`
- Modify: `src/Aivana.App/Views/MainWindow.axaml`

- [ ] Add connection, health, and settings models.
- [ ] Add a shell view model with selected navigation, sample connection profiles, quick connect fields, and computed dashboard values.
- [ ] Replace the starter window with a practical desktop layout: left navigation, connection list, quick connect panel, health cards, and settings.
- [ ] Keep the UI Avalonia-native and avoid dependencies on WPF/Windows-only controls.

### Task 3: Add Persistence and Tests

**Files:**
- Create: `src/Aivana.App/Storage.fs`
- Modify: `src/Aivana.App/Aivana.App.fsproj`
- Modify: `tests/Aivana.App.Tests/Tests.fs`

- [ ] Add a small JSON serializer module for loading/saving connection profiles.
- [ ] Add focused tests for profile defaults, search/filter behavior, and JSON round-trip persistence.
- [ ] Run `dotnet test Aivana.sln`.

### Task 4: Verify and Clean Up

**Files:**
- Modify: `README.md`
- Modify: `.gitignore` if generated artifacts need excluding.

- [ ] Update README with .NET 10, F#, Avalonia run/test commands.
- [ ] Run `dotnet build Aivana.sln`.
- [ ] Run `dotnet test Aivana.sln`.
- [ ] Check `git status --short` and summarize the rewrite.
