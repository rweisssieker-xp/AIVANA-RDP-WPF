# Story 10.6: Feedback & Issue Reporting

Status: drafted

## Story

As a user,
I want to provide feedback and report issues,
so that I can help improve the application.

## Acceptance Criteria

1. **AC1:** Feedback dialog opens
2. **AC2:** Users can enter: feedback type (bug report, feature request, general feedback), description, contact email (optional)
3. **AC3:** Users can attach screenshots or logs
4. **AC4:** Feedback sent (email or web service) when submitted

## Tasks / Subtasks

- [ ] Task 1: Create Feedback Dialog UI (AC: #1, #2, #3)
  - [ ] Create FeedbackDialog (ContentDialog)
  - [ ] Feedback form: Type, description, email, attachments
  - [ ] Option to attach application logs
- [ ] Task 2: Implement Feedback Submission (AC: #4)
  - [ ] Send via email or web API (can be post-MVP)
  - [ ] Log attachment: Option to attach application logs

## Dev Notes

### Project Structure Notes

**Alignment with Architecture Specification:**
- Feedback follows Architecture "User Feedback" patterns

**Technical Constraints:**
- Feedback UI: FeedbackDialog (ContentDialog)
- Feedback form: Type, description, email, attachments
- Feedback submission: Send via email or web API (can be post-MVP)
- Log attachment: Option to attach application logs

### References

- [Source: docs/epics.md#Story-10.6] - Story acceptance criteria and technical notes
- [Source: docs/sprint-artifacts/tech-spec-epic-10.md#Acceptance-Criteria] - AC from tech spec

## Dev Agent Record

### Context Reference

<!-- Path(s) to story context XML will be added here by context workflow -->

### Agent Model Used

{{agent_model_name_version}}

### Debug Log References

### Completion Notes List

### File List

