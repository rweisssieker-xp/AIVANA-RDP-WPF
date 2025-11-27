# Story 1.3: Database Setup & Entity Framework Core

Status: done

## Story

As a developer,
I want SQLite database configured with Entity Framework Core,
so that connection profiles and session history can be persisted.

## Acceptance Criteria

1. **AC6:** ApplicationDbContext created in Infrastructure/Database folder, inherits from DbContext
2. **AC7:** ConnectionProfile and SessionHistory entities defined in Models folder with all required properties
3. **AC8:** Initial EF Core migration created and applied, database file created at %AppData%\Aivana_RDP_WPF\aivana.db

## Tasks / Subtasks

- [x] Task 1: Create ApplicationDbContext (AC: #6)
  - [x] Create Infrastructure/Database/ApplicationDbContext.cs
  - [x] Inherit from DbContext
  - [x] Add DbSet<ConnectionProfile> property
  - [x] Add DbSet<SessionHistory> property
  - [x] Configure database connection string
  - [x] Create ApplicationDbContextFactory for design-time migrations
  - [x] Verify ApplicationDbContext inherits from DbContext
- [x] Task 2: Define Data Models (AC: #7)
  - [x] Create Models/ConnectionProfile.cs with properties: Id, Name, ServerAddress, Port, Username, Domain, IsFavorite, GroupName, Tags (JSON), Settings (JSON), CreatedAt, LastConnectedAt, ConnectionCount
  - [x] Create Models/SessionHistory.cs with properties: Id, ConnectionProfileId, ConnectedAt, DisconnectedAt, Duration, Status, ErrorMessage
  - [x] Configure EF Core entity relationships
  - [x] Configure indexes (IsFavorite, GroupName for ConnectionProfile; ConnectionProfileId, ConnectedAt for SessionHistory)
  - [x] Verify entities have all required properties, EF Core recognizes them
- [x] Task 3: Create and Apply Migration (AC: #8)
  - [x] Create initial migration: `dotnet ef migrations add InitialCreate`
  - [x] Apply migration: `dotnet ef database update`
  - [x] Verify database file created at %AppData%\Aivana_RDP_WPF\aivana.db
  - [x] Verify migration applied successfully

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

**Completed:** 2025-11-27
- ApplicationDbContext created with DbSet properties for ConnectionProfile and SessionHistory
- ConnectionProfile model created with all required properties (Tags and Settings as JSON strings)
- SessionHistory model created with all required properties (Duration stored as ticks for SQLite compatibility)
- Entity relationships configured (SessionHistory -> ConnectionProfile with cascade delete)
- Indexes configured for performance (IsFavorite, GroupName, ConnectionProfileId, ConnectedAt)
- ApplicationDbContextFactory created for design-time migrations
- Initial migration created and applied successfully
- Database file created at %AppData%\Aivana_RDP_WPF\aivana.db
- DbContext registered in DI container with SQLite connection

### File List

- `Aivana_RDP_WPF/Models/ConnectionProfile.cs` - Connection profile entity
- `Aivana_RDP_WPF/Models/SessionHistory.cs` - Session history entity
- `Aivana_RDP_WPF/Infrastructure/Database/ApplicationDbContext.cs` - Database context
- `Aivana_RDP_WPF/Infrastructure/Database/ApplicationDbContextFactory.cs` - Design-time factory
- `Aivana_RDP_WPF/Infrastructure/Database/Migrations/` - EF Core migrations folder

