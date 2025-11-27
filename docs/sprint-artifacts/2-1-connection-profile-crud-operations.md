# Story 2.1: Connection Profile CRUD Operations

Status: done

## Story

As a user,
I want to create, edit, and delete connection profiles,
so that I can save my RDP connection settings for future use.

## Acceptance Criteria

1. **AC1:** Users can create connection profiles with all required fields
2. **AC2:** Users can edit existing profiles
3. **AC3:** Users can delete profiles with confirmation

## Tasks / Subtasks

- [ ] Task 1: Implement ConnectionProfileService CRUD (AC: #1, #2, #3)
  - [ ] Implement CreateProfileAsync method
  - [ ] Implement UpdateProfileAsync method
  - [ ] Implement DeleteProfileAsync method
  - [ ] Implement GetProfileByIdAsync method
  - [ ] Use ConnectionProfileRepository for database operations
  - [ ] Unit tests for CRUD operations
- [ ] Task 2: Create Connection Configuration Dialog UI (AC: #1, #2)
  - [ ] Create Views/ConnectionManagement/ConnectionConfigDialog.xaml (ContentDialog, 600px width)
  - [ ] Add form fields: name, server address, port (default 3389), username, domain (optional)
  - [ ] Add display settings: resolution, color depth, audio settings
  - [ ] Form validation: Required fields, port validation (1-65535), server address format
  - [ ] Pre-fill values for edit mode
- [ ] Task 3: Implement Delete Confirmation (AC: #3)
  - [ ] Create confirmation dialog (ContentDialog with destructive action button)
  - [ ] Remove profile from database on confirm
  - [ ] Remove associated credentials from Windows Credential Manager
  - [ ] Error handling: Show InfoBar for errors, log exceptions

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Service follows Architecture "Services" section
- UI follows UX spec section 7.1 (ContentDialog patterns)
- Repository pattern follows Architecture "Data Layer"

**Technical Constraints:**
- Use ConnectionProfile model from Story 1.3
- UI: ContentDialog, 600px width per UX spec section 7.1
- Error handling: InfoBar for errors, log exceptions

### References

- [Source: docs/architecture.md#Services] - Service structure
- [Source: docs/epics.md#Story-2.1] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-2.md#Acceptance-Criteria] - AC1, AC2, AC3 from tech spec
- [Source: docs/ux-design-specification.md#Section-7.1] - ContentDialog patterns

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

