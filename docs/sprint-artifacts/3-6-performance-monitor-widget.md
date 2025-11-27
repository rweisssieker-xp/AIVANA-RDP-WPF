# Story 3.6: Performance Monitor Widget

Status: drafted

## Story

As a user,
I want a detailed performance monitor widget,
so that I can analyze connection performance over time.

## Acceptance Criteria

1. **AC1:** Detailed metrics display: bandwidth (Mbps), latency (ms), frame rate (FPS), packet loss (%)
2. **AC2:** Mini graphs show historical trends (last 5 minutes)
3. **AC3:** Metrics update in real-time (every second)
4. **AC4:** Widget can be collapsed to icon-only view
5. **AC5:** Widget can be detached as floating window (optional)
6. **AC6:** Widget highlights in alert color when metrics exceed thresholds

## Tasks / Subtasks

- [ ] Task 1: Create Performance Monitor Widget UI (AC: #1, #3, #4, #5)
  - [ ] Create PerformanceMonitorWidget component (UX spec section 6.1)
  - [ ] Display metrics: bandwidth, latency, frame rate, packet loss
  - [ ] Widget states: Collapsed (icon-only), Expanded (full metrics), Floating (detached window)
- [ ] Task 2: Implement Historical Data (AC: #2)
  - [ ] Store last 5 minutes of metrics (300 data points at 1s intervals)
  - [ ] Create mini graphs using WPF Chart control or custom drawing
- [ ] Task 3: Implement Alert Highlighting (AC: #6)
  - [ ] Check alert thresholds (configurable in settings)
  - [ ] Highlight widget in alert color (amber/red per UX spec)

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Widget follows UX spec section 6.1 Performance Monitor Widget
- Historical data storage follows Architecture "Data Storage" patterns

**Technical Constraints:**
- Historical data: Store last 5 minutes of metrics (300 data points at 1s intervals)
- Mini graphs: Use WPF Chart control or OxyPlot library
- Alert thresholds: Configurable in settings (high latency > 100ms, low bandwidth < 1 Mbps)

### References

- [Source: docs/epics.md#Story-3.6] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-3.md#Acceptance-Criteria] - AC from tech spec
- [Source: docs/ux-design-specification.md#Section-6.1] - Performance Monitor Widget

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

