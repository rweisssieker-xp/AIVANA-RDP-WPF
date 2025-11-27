# Aivana_RDP_WPF Tests

Comprehensive test suite for Aivana_RDP_WPF application.

## Test Structure

```
Aivana_RDP_WPF.Tests/
├── UnitTests/              # Unit tests for business logic
├── IntegrationTests/       # Integration tests for services and database
├── SystemTests/            # End-to-end system tests
├── PerformanceTests/       # Performance and benchmark tests
├── SecurityTests/          # Security-focused tests
└── TestHelpers/            # Test utilities, fixtures, and factories
```

## Running Tests

### Run all tests
```bash
dotnet test
```

### Run specific test category
```bash
# Unit tests only
dotnet test --filter "TestType=Unit"

# Integration tests only
dotnet test --filter "TestType=Integration"

# System tests only
dotnet test --filter "TestType=System"

# Performance tests only
dotnet test --filter "TestType=Performance"

# Security tests only
dotnet test --filter "TestType=Security"
```

### Run by priority
```bash
# Critical tests only
dotnet test --filter "Priority=P0"

# High priority tests
dotnet test --filter "Priority=P1"
```

### Run with code coverage
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Test Categories

### Priority Levels
- **P0 (Critical)**: Must pass before merge - core functionality, security
- **P1 (High)**: Should pass - important features
- **P2 (Medium)**: Nice to have - edge cases
- **P3 (Low)**: Optional - non-critical features

### Test Types
- **Unit**: Fast, isolated tests for business logic
- **Integration**: Service boundaries and database operations
- **System**: End-to-end user workflows
- **Performance**: NFR compliance and benchmarks
- **Security**: Security requirements validation

## Test Helpers

### Fixtures
- `DatabaseFixture`: In-memory database for integration tests
- `RdpServerFixture`: Mock/test RDP server
- `CredentialManagerFixture`: Mock Windows Credential Manager

### Factories
- `ConnectionProfileFactory`: Create test connection profiles
- `SessionHistoryFactory`: Create test session history
- `PerformanceMetricsFactory`: Create test performance metrics
- `FileTransferFactory`: Create test file transfer records

### Custom Assertions
- `CustomAssertions`: Domain-specific assertion helpers

## Best Practices

1. **One assertion per test** - Atomic test design
2. **Given-When-Then format** - Clear test structure
3. **Meaningful test names** - Describe what is being tested
4. **Clean up after tests** - Dispose fixtures and resources
5. **Use factories** - Consistent test data creation
6. **Mock external dependencies** - Isolate unit tests

## CI/CD Integration

Tests run automatically on:
- Every commit (unit tests)
- Pull requests (unit + integration tests)
- Nightly builds (all tests including system tests)
- Weekly builds (performance and security tests)

## Coverage Goals

- **Unit Tests**: 80%+ code coverage for business logic
- **Integration Tests**: 100% coverage for service boundaries
- **System Tests**: 100% coverage for P0 user journeys

