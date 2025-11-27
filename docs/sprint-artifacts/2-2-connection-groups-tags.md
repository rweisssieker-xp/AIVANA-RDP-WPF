# Story 2.2: Connection Groups & Tags

Status: drafted

## Story

As a user,
I want to organize connections into groups and assign tags,
so that I can easily find and filter my connections.

## Acceptance Criteria

1. **AC1:** Users can assign connections to groups (dropdown or create new group)
2. **AC2:** Users can add multiple tags (comma-separated or tag input field)
3. **AC3:** Groups displayed in sidebar navigation (NavigationView per UX spec)
4. **AC4:** Tags displayed as badges on connection cards
5. **AC5:** Users can filter connections by group or tag

## Tasks / Subtasks

- [ ] Task 1: Update ConnectionProfile Model (AC: #1, #2)
  - [ ] Verify GroupName property exists in ConnectionProfile model
  - [ ] Add Tags property (List<string> stored as JSON in database)
  - [ ] Update database migration if needed
- [ ] Task 2: Update ConnectionProfileService (AC: #1, #2)
  - [ ] Add group operations: GetGroupsAsync, GetConnectionsByGroupAsync
  - [ ] Add tag operations: GetConnectionsByTagAsync
  - [ ] Update CreateProfileAsync and UpdateProfileAsync to handle groups/tags
- [ ] Task 3: Implement Sidebar Navigation with Groups (AC: #3)
  - [ ] Create NavigationView with groups in sidebar
  - [ ] Display groups hierarchically
  - [ ] Implement expand/collapse functionality
  - [ ] Implement group filtering on click
- [ ] Task 4: Display Tags on Connection Cards (AC: #4)
  - [ ] Add tag badges/chips to ConnectionCard component
  - [ ] Style tags per UX spec section 6.1
- [ ] Task 5: Implement Filtering (AC: #5)
  - [ ] Implement filter logic in ConnectionListViewModel
  - [ ] Filter by group: ObservableCollection filtering
  - [ ] Filter by tag: ObservableCollection filtering
  - [ ] Update UI when filters change

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Groups and tags follow Architecture "Data Models" section
- UI follows UX spec section 4.1 (Sidebar Navigation) and section 6.1 (Connection Card Component)
- Filtering follows MVVM pattern with ViewModel filtering

**Technical Constraints:**
- Tags stored as JSON string in database (List<string> serialized)
- GroupName already in ConnectionProfile model
- Filtering uses ObservableCollection LINQ filtering

### References

- [Source: docs/epics.md#Story-2.2] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-2.md#Acceptance-Criteria] - AC from tech spec
- [Source: docs/ux-design-specification.md#Section-4.1] - Sidebar Navigation patterns
- [Source: docs/ux-design-specification.md#Section-6.1] - Connection Card Component

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

