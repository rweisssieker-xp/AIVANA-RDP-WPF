# Story 1.3: Database Setup & Entity Framework Core

Status: drafted

## Story

As a developer,
I want SQLite database configured with Entity Framework Core,
so that connection profiles and session history can be persisted.

## Acceptance Criteria

1. **AC6:** ApplicationDbContext created in Infrastructure/Database folder, inherits from DbContext
2. **AC7:** ConnectionProfile and SessionHistory entities defined in Models folder with all required properties
3. **AC8:** Initial EF Core migration created and applied, database file created at %AppData%\Aivana_RDP_WPF\aivana.db

## Tasks / Subtasks

- [ ] Task 1: Create ApplicationDbContext (AC: #6)
  - [ ] Create Infrastructure/Database/ApplicationDbContext.cs
  - [ ] Inherit from DbContext
  - [ ] Add DbSet<ConnectionProfile> property
  - [ ] Add DbSet<SessionHistory> property
  - [ ] Configure database connection string
  - [ ] Verify ApplicationDbContext inherits from DbContext
- [ ] Task 2: Define Data Models (AC: #7)
  - [ ] Create Models/ConnectionProfile.cs with properties: Id, Name, ServerAddress, Port, Username, Domain, IsFavorite, GroupName, Tags (JSON), Settings (JSON), CreatedAt, LastConnectedAt, ConnectionCount
  - [ ] Create Models/SessionHistory.cs with properties: Id, ConnectionProfileId, ConnectedAt, DisconnectedAt, Duration, Status, ErrorMessage
  - [ ] Configure EF Core entity relationships
  - [ ] Verify entities have all required properties, EF Core recognizes them
- [ ] Task 3: Create and Apply Migration (AC: #8)
  - [ ] Create initial migration: `dotnet ef migrations add InitialCreate`
  - [ ] Apply migration: `dotnet ef database update`
  - [ ] Verify database file created at %AppData%\Aivana_RDP_WPF\aivana.db
  - [ ] Verify migration applied successfully

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Database setup follows Architecture ADR-003 (SQLite with EF Core)
- ApplicationDbContext location matches Architecture "Project Structure"
- Data models follow Architecture "Data Models" section

**Technical Constraints:**
- Database location: %AppData%\Aivana_RDP_WPF\aivana.db
- Use Entity Framework Core 8.0
- ConnectionProfile.Tags stored as JSON string
- ConnectionProfile.Settings stored as JSON string

### References

- [Source: docs/architecture.md#Data-Persistence] - SQLite and EF Core specification
- [Source: docs/epics.md#Story-1.3] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-1.md#Data-Models-and-Contracts] - Data model specifications
- [Source: docs/sprint-artifacts/tech-spec-epic-1.md#Acceptance-Criteria] - AC6, AC7, AC8 from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

