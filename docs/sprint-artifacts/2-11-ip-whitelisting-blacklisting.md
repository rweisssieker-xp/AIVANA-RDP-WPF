# Story 2.11: IP Whitelisting/Blacklisting

Status: drafted

## Story

As a user,
I want to configure IP restrictions for connections,
so that I can control which servers I can connect to.

## Acceptance Criteria

1. **AC1:** Users can add IP addresses or CIDR ranges to whitelist or blacklist
2. **AC2:** Users can enable/disable IP filtering per connection
3. **AC3:** Settings saved in ConnectionProfile.Settings
4. **AC4:** Blacklisted IPs blocked with error message
5. **AC5:** Whitelist-only mode blocks non-whitelisted IPs
6. **AC6:** IP validation before connection attempt

## Tasks / Subtasks

- [ ] Task 1: Add IP Filtering UI (AC: #1, #2, #3)
  - [ ] Add IP filtering section to connection security settings
  - [ ] Add whitelist input field (IP addresses or CIDR ranges)
  - [ ] Add blacklist input field (IP addresses or CIDR ranges)
  - [ ] Add enable/disable toggle per connection
- [ ] Task 2: Implement IP Storage (AC: #3)
  - [ ] Store IP lists in ConnectionProfile.Settings JSON (whitelist: [], blacklist: [])
- [ ] Task 3: Implement IP Validation (AC: #4, #5, #6)
  - [ ] Validate IP addresses and CIDR notation
  - [ ] Check IP before RDP connection attempt
  - [ ] Block blacklisted IPs with clear error message
  - [ ] Block non-whitelisted IPs if whitelist enabled

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- IP filtering follows Architecture "Security Architecture" section
- Validation follows Architecture "Error Handling" patterns

**Technical Constraints:**
- IP validation: Validate IP addresses and CIDR notation
- Check timing: Validate IP before RDP connection attempt
- Error messages: Clear error messages explaining why connection was blocked

### References

- [Source: docs/epics.md#Story-2.11] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-2.md#Acceptance-Criteria] - AC from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

