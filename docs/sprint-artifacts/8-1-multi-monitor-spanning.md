# Story 8.1: Multi-Monitor Spanning

Status: drafted

## Story

As a user,
I want to span remote desktop across multiple local monitors,
so that I can use my full multi-monitor setup.

## Acceptance Criteria

1. **AC1:** Remote desktop spans across all monitors
2. **AC2:** Remote desktop resolution matches combined monitor resolution
3. **AC3:** Spanning works with 2-4 monitors (typical setups)

## Tasks / Subtasks

- [ ] Task 1: Detect Available Monitors (AC: #1, #3)
  - [ ] Detect available monitors using System.Windows.Forms.Screen
- [ ] Task 2: Configure Multi-Monitor Spanning (AC: #1, #2)
  - [ ] Use RDP control multi-monitor properties
  - [ ] Configure RDP control to span across monitors
  - [ ] Calculate combined resolution for spanning

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Multi-monitor follows Architecture "Multi-Monitor Support" section

**Technical Constraints:**
- Multi-monitor: Use RDP control multi-monitor properties
- Monitor detection: Detect available monitors using System.Windows.Forms.Screen
- Spanning: Configure RDP control to span across monitors
- Resolution: Calculate combined resolution for spanning

### References

- [Source: docs/epics.md#Story-8.1] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-8.md#Acceptance-Criteria] - AC from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

