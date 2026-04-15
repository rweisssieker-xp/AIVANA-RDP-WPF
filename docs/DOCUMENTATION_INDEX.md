# Aivana RDP WPF - Documentation Index

**Last Updated:** 2026-04-15

---

## 📚 Complete Documentation Overview

This index provides a complete guide to all available documentation for Aivana RDP WPF.

---

## 🚀 Getting Started

### For Users
- **[README.md](../README.md)** - Project overview, quick start, and features
- **[DOCUMENTATION.md](DOCUMENTATION.md)** ⭐ **Complete master documentation** — all features, APIs, configuration, and guides in one place
- **[User Guide](USER_GUIDE.md)** - Complete user documentation with step-by-step guides

### For Developers
- **[DOCUMENTATION.md](DOCUMENTATION.md)** ⭐ **Complete master documentation** — includes developer guide, service API, data models, and testing
- **[Developer Guide](DEVELOPER_GUIDE.md)** - Development setup, workflow, and code guidelines
- **[Architecture](architecture.md)** - Technical architecture and design decisions

---

## 📖 User Documentation

### Guides
- **[User Guide](USER_GUIDE.md)** - Complete user manual
  - Getting Started
  - Connection Management
  - File Transfer
  - Clipboard Synchronization
  - Performance Monitoring
  - Session Recording
  - Settings & Customization
  - Keyboard Shortcuts
  - Troubleshooting
  - FAQ

- **[Configuration Guide](CONFIGURATION_GUIDE.md)** - Configuration options and settings
  - Configuration Files
  - Application Settings
  - Connection Settings
  - Performance Configuration
  - Logging Configuration
  - Environment Variables

- **[Troubleshooting Guide](TROUBLESHOOTING.md)** - Common issues and solutions
  - Connection Issues
  - Application Startup Problems
  - Performance Issues
  - Database Issues
  - Credential Management Issues
  - UI/Display Issues
  - Known Issues

---

## 👨‍💻 Developer Documentation

### Technical Documentation
- **[Architecture Document](architecture.md)** - System architecture
  - Technology Stack
  - Project Structure
  - Design Patterns
  - Architecture Decision Records (ADRs)
  - Security Architecture
  - Performance Considerations

- **[Developer Guide](DEVELOPER_GUIDE.md)** - Development workflow
  - Prerequisites
  - Development Environment Setup
  - Project Structure
  - Building the Project
  - Running Tests
  - Development Workflow
  - Code Organization
  - Architecture Patterns
  - Adding New Features
  - Debugging
  - Code Style Guidelines

- **[Contributing Guide](CONTRIBUTING.md)** - Contribution guidelines
  - Code of Conduct
  - How to Contribute
  - Development Setup
  - Coding Standards
  - Pull Request Process
  - Reporting Issues
  - Feature Requests

---

## 📋 Project Documentation

### Requirements & Planning
- **[PRD (Product Requirements Document)](prd.md)** - Product requirements v2.0
  - Executive Summary & Product Vision
  - Target Users & Personas
  - Product Scope (MVP, Growth, Vision)
  - Functional Requirements (FR-CM, FR-CN, FR-WS, FR-WF, FR-WOL, FR-SSH, FR-FT, FR-CB, FR-PM, FR-SR, FR-SEC, FR-UI, FR-CFG)
  - Non-Functional Requirements (Performance, Security, Scalability, Accessibility, Reliability, Integration)
  - Technical Architecture Overview
  - Roadmap (v1.0 released → v2.0 vision)

- **[Epics & Stories](epics.md)** - Epic and story breakdown
  - 10 Epics
  - 67 Detailed Stories
  - Acceptance Criteria
  - Technical Notes

- **[UX Design Specification](ux-design-specification.md)** - UI/UX design
  - Fluent Design System 2.0
  - Color System
  - Typography
  - Component Library
  - User Journey Flows

### Technical Specifications
- **[Tech Specs](sprint-artifacts/tech-spec-epic-*.md)** - Epic technical specifications
  - Epic 1: Foundation & Infrastructure
  - Epic 2: Core Connection Management
  - Epic 3: Basic RDP Connection & Performance
  - Epic 4: Modern User Interface Foundation
  - Epic 5: Multi-Session Dashboard
  - Epic 6: File Transfer
  - Epic 7: Clipboard Synchronization
  - Epic 8: Multi-Monitor Support
  - Epic 9: Session Recording
  - Epic 10: Settings & Help

- **[Story Files](sprint-artifacts/*-story-*.md)** - Detailed story implementation plans
  - 67 story files with acceptance criteria
  - Task breakdowns
  - Technical notes

---

## 📊 Status & Analysis

### Implementation Status
- **[Complete Implementation Status](COMPLETE_IMPLEMENTATION_STATUS.md)** - Full feature status
- **[Final Status](FINAL_STATUS.md)** - Final implementation summary
- **[Implementation Status](IMPLEMENTATION_STATUS.md)** - Detailed status tracking

### Analysis Documents
- **[Dev Analysis: Missing Components](DEV_ANALYSIS_MISSING_COMPONENTS.md)** - Component gap analysis
- **[Tech Writer: Documentation Gap Analysis](TECH_WRITER_DOCUMENTATION_GAP_ANALYSIS.md)** - Documentation gaps
- **[Implementation Readiness Report](implementation-readiness-report-2025-11-27.md)** - Readiness assessment

### Testing
- **[Test Design](test-design.md)** - Testing strategy
- **[Test Infrastructure Summary](test-infrastructure-summary.md)** - Test framework setup

---

## 📝 Project History

- **[Changelog](../CHANGELOG.md)** - Version history and release notes
- **[Brainstorming Session](bmm-brainstorming-session-2025-11-26.md)** - Initial project brainstorming
- **[Workflow Status](bmm-workflow-status.yaml)** - BMAD workflow tracking

---

## 🔍 Quick Reference

### By Role

**End User:**
1. [README.md](../README.md) - Start here
2. [User Guide](USER_GUIDE.md) - Complete manual
3. [Troubleshooting](TROUBLESHOOTING.md) - Problem solving

**Developer:**
1. [README.md](../README.md) - Project overview
2. [Developer Guide](DEVELOPER_GUIDE.md) - Setup and workflow
3. [Architecture](architecture.md) - Technical details
4. [Contributing Guide](CONTRIBUTING.md) - Contribution process

**Administrator:**
1. [Configuration Guide](CONFIGURATION_GUIDE.md) - Configuration options
2. [Troubleshooting](TROUBLESHOOTING.md) - Common issues
3. [User Guide](USER_GUIDE.md) - Feature reference

**Contributor:**
1. [Contributing Guide](CONTRIBUTING.md) - Contribution guidelines
2. [Developer Guide](DEVELOPER_GUIDE.md) - Development setup
3. [Code Style Guidelines](DEVELOPER_GUIDE.md#code-style-guidelines) - Coding standards

---

## 📁 Documentation Structure

```
docs/
├── DOCUMENTATION.md                 ⭐ Complete master documentation
├── prd.md                           ⭐ Product Requirements Document (v2.0)
├── USER_GUIDE.md                    # Complete user manual
├── DEVELOPER_GUIDE.md               # Developer setup and workflow
├── CONFIGURATION_GUIDE.md           # Configuration reference
├── TROUBLESHOOTING.md               # Troubleshooting guide
├── CONTRIBUTING.md                  # Contribution guidelines
├── DOCUMENTATION_INDEX.md           # This file
│
├── architecture.md                  # Architecture specification
├── ux-design-specification.md       # UX design spec
├── epics.md                         # Epics and stories
│
├── COMPLETE_IMPLEMENTATION_STATUS.md # Feature status
├── FINAL_STATUS.md                  # Final status summary
├── IMPLEMENTATION_STATUS.md         # Status tracking
│
├── sprint-artifacts/                # Technical specs and stories
│   ├── tech-spec-epic-*.md         # Epic tech specs (10 files)
│   └── *-story-*.md                # Story files (67 files)
│
└── [other analysis and planning docs]
```

---

## 🔗 External Resources

### Microsoft Documentation
- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [WPF Documentation](https://docs.microsoft.com/dotnet/desktop/wpf/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [Windows Credential Manager API](https://docs.microsoft.com/windows/win32/api/wincred/)

### Community Resources
- [CommunityToolkit.Mvvm](https://github.com/CommunityToolkit/dotnet)
- [Fluent Design System](https://www.microsoft.com/design/fluent/)

---

## 📞 Getting Help

### Documentation Issues
- Found an error? Open an issue or create a PR
- Missing information? Check [Documentation Gap Analysis](TECH_WRITER_DOCUMENTATION_GAP_ANALYSIS.md)

### Support Channels
- **GitHub Issues**: Report bugs or request features
- **GitHub Discussions**: Ask questions
- **Documentation**: Check relevant guide above

---

**Last Updated:** 2026-04-15