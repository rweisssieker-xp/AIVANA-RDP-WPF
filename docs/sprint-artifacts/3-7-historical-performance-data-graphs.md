# Story 3.7: Historical Performance Data & Graphs

Status: drafted

## Story

As a user,
I want to view historical performance data in graphs,
so that I can analyze connection quality trends over time.

## Acceptance Criteria

1. **AC1:** Graphs showing: bandwidth over time, latency over time, frame rate over time
2. **AC2:** Graphs show data for current session or selected time range
3. **AC3:** Users can zoom in/out on graphs
4. **AC4:** Users can export performance data as CSV or image
5. **AC5:** Historical data stored for last 30 days (configurable)
6. **AC6:** Summary statistics displayed: average latency, peak bandwidth, average frame rate

## Tasks / Subtasks

- [ ] Task 1: Store Performance Metrics (AC: #5)
  - [ ] Store PerformanceMetrics in SQLite database (see Architecture Data Models)
  - [ ] Historical retention: 30 days (configurable)
- [ ] Task 2: Create Performance History View (AC: #1, #2, #6)
  - [ ] Create performance history view in Settings or dedicated panel
  - [ ] Render graphs using WPF Chart control or OxyPlot library
  - [ ] Add time range selection (Date/time picker)
  - [ ] Display summary statistics: averages, peaks, minimums
- [ ] Task 3: Implement Graph Interactions (AC: #3)
  - [ ] Add zoom in/out functionality
- [ ] Task 4: Implement Export (AC: #4)
  - [ ] CSV export (comma-separated values)
  - [ ] Image export (PNG/JPEG)

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Data storage follows Architecture "Data Models" section
- Graph rendering follows Architecture "UI Components" patterns

**Technical Constraints:**
- Historical retention: 30 days (configurable)
- Graph rendering: Use WPF Chart control or OxyPlot library
- Export: CSV export, image export (PNG/JPEG)

### References

- [Source: docs/epics.md#Story-3.7] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-3.md#Acceptance-Criteria] - AC from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

