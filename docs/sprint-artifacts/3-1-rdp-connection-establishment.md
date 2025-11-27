# Story 3.1: RDP Connection Establishment

Status: drafted

## Story

As a user,
I want to establish an RDP connection to a remote desktop,
so that I can work on remote machines.

## Acceptance Criteria

1. **AC1:** Connection progress indicator appears (ProgressRing per UX spec)
2. **AC2:** Connection status shows "Connecting..." with amber color
3. **AC3:** RDP session established using MSTSC ActiveX control
4. **AC4:** Remote desktop appears in main content area when connection succeeds
5. **AC5:** Connection status updates to "Connected" with green color
6. **AC6:** Error dialog appears with retry option if connection fails
7. **AC7:** Connection completes in under 3 seconds for local network (NFR4)

## Tasks / Subtasks

- [ ] Task 1: Implement IRdpConnectionService (AC: #3, #7)
  - [ ] Create IRdpConnectionService interface
  - [ ] Implement ConnectAsync method
  - [ ] Use MSTSC ActiveX control (mstscax.dll) wrapped in RdpClientWrapper
  - [ ] Connection flow: Resolve server → Authenticate → Establish session → Display remote desktop
- [ ] Task 2: Create RDP Control Wrapper (AC: #3, #4)
  - [ ] Create RdpClientWrapper class
  - [ ] Wrap MSTSC ActiveX control
  - [ ] Embed RDP control in WPF via WindowsFormsHost
- [ ] Task 3: Implement Connection UI (AC: #1, #2, #4, #5)
  - [ ] Add ProgressRing (indeterminate) during connection
  - [ ] Show connection status: "Connecting..." (amber #FFB900), "Connected" (green #107C10)
  - [ ] Display remote desktop in main content area
- [ ] Task 4: Error Handling (AC: #6)
  - [ ] Catch connection exceptions
  - [ ] Show ContentDialog with error message and retry button

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- RDP implementation follows Architecture ADR-004
- Status colors follow UX spec section 3.1 Connection Status Colors

**Technical Constraints:**
- Use MSTSC ActiveX control (mstscax.dll)
- UI: RDP control embedded in WPF via WindowsFormsHost
- Status colors: Connecting: #FFB900 Amber, Connected: #107C10 Green

### References

- [Source: docs/epics.md#Story-3.1] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-3.md#Acceptance-Criteria] - AC from tech spec
- [Source: docs/architecture.md#ADR-004] - RDP implementation details

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

