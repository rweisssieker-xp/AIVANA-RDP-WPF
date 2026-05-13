# Aivana

Aivana is a cross-platform remote access console built with F#, .NET 10, and Avalonia.

This branch replaces the previous WPF/C# implementation with a fresh Avalonia application shell focused on connection profiles, quick connect, health status, and workspace settings.

## Requirements

- .NET 10 SDK
- Windows, macOS, or Linux desktop runtime supported by Avalonia

## Build

```powershell
dotnet restore Aivana.slnx
dotnet build Aivana.slnx
```

## Run

```powershell
dotnet run --project src/Aivana.App/Aivana.App.fsproj
```

## Test

```powershell
dotnet test Aivana.slnx
```

## Project Structure

```text
src/
  Aivana.App/          Avalonia/F# desktop app
tests/
  Aivana.App.Tests/    F# xUnit tests
docs/
  superpowers/         Design and implementation planning notes
```

## Current Scope

- Avalonia desktop shell with sidebar navigation
- Connection profile model with protocol, health, grouping, tags, and favorites
- Search/filter support for connection profiles
- Quick connect panel
- Health summary cards
- JSON profile storage module
