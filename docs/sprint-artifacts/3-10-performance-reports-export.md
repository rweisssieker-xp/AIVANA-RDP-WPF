# Story 3.10: Performance Reports Export

Status: drafted

## Story

As a user,
I want to export performance reports,
so that I can analyze connection quality and share reports with my team.

## Acceptance Criteria

1. **AC1:** Users can choose export format: CSV, PDF, or HTML
2. **AC2:** Report includes: summary statistics, graphs/charts, detailed metrics table
3. **AC3:** Report includes: connection profile name, date range, export timestamp
4. **AC4:** Report saved to selected location
5. **AC5:** Success notification appears
6. **AC6:** CSV file contains: timestamp, latency, bandwidth, frame rate, packet loss columns

## Tasks / Subtasks

- [ ] Task 1: Implement Export Formats (AC: #1, #2, #3, #4, #6)
  - [ ] CSV export: Simple CSV format with headers (timestamp, latency, bandwidth, frame rate, packet loss)
  - [ ] PDF export: Use library like PdfSharp with formatted graphs and summary statistics
  - [ ] HTML export: Formatted report with graphs embedded as images
  - [ ] Include report metadata: connection profile name, date range, export timestamp
- [ ] Task 2: Create Export UI (AC: #1, #4, #5)
  - [ ] Use SaveFileDialog for export location
  - [ ] Show success notification: InfoBar with success style (green, auto-dismiss 3s per UX spec)

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Export follows Architecture "Data Export" patterns
- Notifications follow UX spec section 7.1 InfoBar patterns

**Technical Constraints:**
- Export formats: CSV (comma-separated), PDF (using library like PdfSharp), HTML (formatted report)
- Report content: Summary stats, graphs (as images), detailed metrics table
- Success notification: InfoBar with success style (green, auto-dismiss 3s per UX spec)

### References

- [Source: docs/epics.md#Story-3.10] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-3.md#Acceptance-Criteria] - AC from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

