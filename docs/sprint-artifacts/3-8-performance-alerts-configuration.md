# Story 3.8: Performance Alerts Configuration

Status: drafted

## Story

As a user,
I want to configure performance alerts,
so that I am notified when connection quality degrades.

## Acceptance Criteria

1. **AC1:** Users can set alert thresholds: high latency (default 100ms), low bandwidth (default 1 Mbps), high packet loss (default 5%)
2. **AC2:** Alerts can be enabled/disabled per connection or globally
3. **AC3:** Alert settings saved
4. **AC4:** Alert notification appears when metrics exceed thresholds
5. **AC5:** Notification shows: which metric exceeded threshold, current value, threshold value
6. **AC6:** Notification auto-dismisses after 5 seconds or can be manually dismissed
7. **AC7:** Alert logged for audit purposes

## Tasks / Subtasks

- [ ] Task 1: Create Alert Configuration UI (AC: #1, #2, #3)
  - [ ] Add alert configuration in Settings > Performance > Alerts
  - [ ] Configure thresholds: high latency, low bandwidth, high packet loss
  - [ ] Enable/disable alerts per connection or globally
  - [ ] Store configuration in ApplicationSettings or ConnectionProfile.Settings
- [ ] Task 2: Implement Alert Monitoring (AC: #4, #5, #7)
  - [ ] Monitor metrics in PerformanceMonitorService
  - [ ] Check thresholds every metric update (1 second)
  - [ ] Show InfoBar with warning style (amber background, warning icon per UX spec section 7.1)
  - [ ] Log alert events to audit log (see Story 2.10)
- [ ] Task 3: Notification Dismissal (AC: #6)
  - [ ] Auto-dismiss after 5 seconds
  - [ ] Manual dismiss option

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Alert configuration follows Architecture "Configuration" patterns
- Notifications follow UX spec section 7.1 InfoBar patterns

**Technical Constraints:**
- Alert triggers: Check thresholds every metric update (1 second)
- Notifications: Use InfoBar with warning style (amber background, warning icon per UX spec section 7.1)
- Alert logging: Log alert events to audit log (see Story 2.10)

### References

- [Source: docs/epics.md#Story-3.8] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-3.md#Acceptance-Criteria] - AC from tech spec
- [Source: docs/ux-design-specification.md#Section-7.1] - InfoBar patterns

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

