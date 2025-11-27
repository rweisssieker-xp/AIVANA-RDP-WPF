# Contributing to Aivana RDP WPF

Thank you for your interest in contributing to Aivana RDP WPF! This document provides guidelines and instructions for contributing.

---

## Table of Contents

1. [Code of Conduct](#code-of-conduct)
2. [How to Contribute](#how-to-contribute)
3. [Development Setup](#development-setup)
4. [Coding Standards](#coding-standards)
5. [Pull Request Process](#pull-request-process)
6. [Reporting Issues](#reporting-issues)
7. [Feature Requests](#feature-requests)

---

## Code of Conduct

### Our Pledge

We are committed to providing a welcoming and inclusive environment for all contributors, regardless of background, experience level, gender identity, nationality, personal appearance, race, religion, or sexual identity and orientation.

### Expected Behavior

- Be respectful and inclusive
- Welcome newcomers and help them learn
- Focus on constructive feedback
- Show empathy towards others

### Unacceptable Behavior

- Harassment or discriminatory language
- Personal attacks
- Trolling or inflammatory comments
- Publishing others' private information

---

## How to Contribute

### Types of Contributions

We welcome various types of contributions:

- **Bug Fixes**: Fix issues and improve stability
- **New Features**: Add functionality (discuss first via issue)
- **Documentation**: Improve docs, add examples, fix typos
- **Testing**: Add tests, improve test coverage
- **Code Quality**: Refactoring, performance improvements
- **UI/UX**: Design improvements, accessibility enhancements

---

## Development Setup

### Prerequisites

See [Developer Guide](DEVELOPER_GUIDE.md) for complete setup instructions.

**Quick Setup:**
```bash
# Clone repository
git clone https://github.com/yourusername/Aivana-RDP-WPF.git
cd Aivana-RDP-WPF

# Restore dependencies
dotnet restore

# Build solution
dotnet build

# Run tests
dotnet test
```

---

## Coding Standards

### C# Style Guide

Follow Microsoft C# Coding Conventions:

- **PascalCase** for classes, methods, properties
- **camelCase** for local variables, parameters
- **_camelCase** for private fields
- **UPPER_CASE** for constants

### Code Formatting

- Use 4 spaces for indentation (not tabs)
- Maximum line length: 120 characters
- Use meaningful variable and method names
- Add XML documentation for public APIs

### Example

```csharp
/// <summary>
/// Retrieves connection profiles from the database.
/// </summary>
/// <param name="ct">Cancellation token.</param>
/// <returns>List of connection profiles.</returns>
public async Task<List<ConnectionProfile>> GetAllProfilesAsync(CancellationToken ct = default)
{
    _logger.LogInformation("Retrieving all connection profiles");
    return await _context.ConnectionProfiles
        .OrderBy(p => p.Name)
        .ToListAsync(ct);
}
```

### Async/Await

Always use async/await for I/O operations:

```csharp
// ✅ Good
public async Task<List<ConnectionProfile>> GetProfilesAsync()
{
    return await _context.ConnectionProfiles.ToListAsync();
}

// ❌ Bad
public List<ConnectionProfile> GetProfiles()
{
    return _context.ConnectionProfiles.ToList(); // Blocks thread
}
```

### Error Handling

Use structured exception handling:

```csharp
try
{
    await _service.DoSomethingAsync();
}
catch (SpecificException ex)
{
    _logger.LogError(ex, "Specific error occurred");
    // Handle specific case
}
catch (Exception ex)
{
    _logger.LogError(ex, "Unexpected error occurred");
    throw; // Re-throw if cannot handle
}
```

### Nullable Reference Types

Always check for null:

```csharp
if (profile == null)
{
    _logger.LogWarning("Profile is null");
    return;
}

// Use null-forgiving operator only when certain
var name = profile!.Name;
```

---

## Pull Request Process

### Before Submitting

1. **Check Existing Issues:**
   - Search for similar issues or PRs
   - Comment on existing issues if working on them

2. **Create Feature Branch:**
   ```bash
   git checkout -b feature/your-feature-name
   # or
   git checkout -b fix/bug-description
   ```

3. **Make Changes:**
   - Write code following coding standards
   - Add XML documentation
   - Write or update tests
   - Update documentation if needed

4. **Test Your Changes:**
   ```bash
   # Run all tests
   dotnet test
   
   # Run application
   dotnet run --project Aivana_RDP_WPF/Aivana_RDP_WPF.csproj
   ```

5. **Commit Changes:**
   ```bash
   git add .
   git commit -m "feat: Add your feature description"
   ```

### Commit Message Format

Use conventional commit format:

```
type: description

[optional body]

[optional footer]
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `refactor`: Code refactoring
- `test`: Test additions/changes
- `chore`: Build/config changes
- `style`: Code style changes (formatting)
- `perf`: Performance improvements

**Examples:**
```
feat: Add file transfer progress indicator

fix: Resolve connection timeout issue

docs: Update user guide with new features

refactor: Simplify ConnectionProfileService
```

### Submitting Pull Request

1. **Push Your Branch:**
   ```bash
   git push origin feature/your-feature-name
   ```

2. **Create Pull Request:**
   - Go to GitHub repository
   - Click "New Pull Request"
   - Select your branch
   - Fill out PR template

3. **PR Description Should Include:**
   - What changes were made
   - Why changes were made
   - How to test the changes
   - Screenshots (if UI changes)
   - Related issues (closes #123)

### PR Review Process

1. **Automated Checks:**
   - Build must pass
   - Tests must pass
   - Code analysis must pass

2. **Code Review:**
   - Maintainers review code
   - Address feedback and suggestions
   - Make requested changes

3. **Approval:**
   - At least one maintainer approval required
   - All checks must pass
   - PR is merged

---

## Reporting Issues

### Before Reporting

1. **Search Existing Issues:**
   - Check if issue already reported
   - Comment on existing issue if related

2. **Gather Information:**
   - Error messages
   - Steps to reproduce
   - Expected vs. actual behavior
   - System information

### Issue Template

Use the GitHub issue template and include:

**Bug Report:**
- **Description**: Clear description of the bug
- **Steps to Reproduce**: Step-by-step instructions
- **Expected Behavior**: What should happen
- **Actual Behavior**: What actually happens
- **Screenshots**: If applicable
- **Environment**:
  - Windows version
  - .NET version
  - Application version
- **Log Files**: Relevant log excerpts (remove sensitive info)

**Feature Request:**
- **Description**: Clear description of the feature
- **Use Case**: Why this feature is needed
- **Proposed Solution**: How you envision it working
- **Alternatives**: Other solutions considered

---

## Feature Requests

### Before Requesting

1. **Check Roadmap:**
   - Feature may already be planned
   - Check existing feature requests

2. **Discuss First:**
   - Open a discussion thread
   - Get feedback from community
   - Refine the idea

### Feature Request Process

1. **Create Discussion:**
   - Use "Feature Request" template
   - Describe the feature clearly
   - Explain use cases

2. **Gather Feedback:**
   - Community discusses the feature
   - Refine based on feedback

3. **Implementation:**
   - Feature may be implemented by maintainers
   - Or you can implement it yourself
   - Create PR when ready

---

## Testing Guidelines

### Writing Tests

- Write unit tests for business logic
- Write integration tests for services
- Write UI tests for critical user flows

### Test Structure

```csharp
[Fact]
public async Task GetAllProfilesAsync_ReturnsAllProfiles()
{
    // Arrange
    var service = new ConnectionProfileService(_context, _logger);
    
    // Act
    var result = await service.GetAllProfilesAsync();
    
    // Assert
    Assert.NotEmpty(result);
    Assert.All(result, p => Assert.NotNull(p.Name));
}
```

### Running Tests

```bash
# All tests
dotnet test

# Specific test class
dotnet test --filter FullyQualifiedName~ConnectionProfileServiceTests

# With coverage
dotnet test /p:CollectCoverage=true
```

---

## Documentation

### Code Documentation

- Add XML documentation for all public APIs
- Include parameter descriptions
- Document exceptions
- Provide usage examples

### User Documentation

- Update [User Guide](USER_GUIDE.md) for user-facing changes
- Add screenshots for UI changes
- Update [Troubleshooting](TROUBLESHOOTING.md) for new issues

### Developer Documentation

- Update [Developer Guide](DEVELOPER_GUIDE.md) for development changes
- Document new patterns or conventions
- Update architecture docs if structure changes

---

## Questions?

### Getting Help

- **GitHub Discussions**: Ask questions and get help
- **GitHub Issues**: Report bugs or request features
- **Documentation**: Check [docs/](.) folder

### Communication

- Be respectful and professional
- Provide context when asking questions
- Help others when you can
- Thank contributors for their work

---

## Recognition

Contributors will be recognized in:
- CONTRIBUTORS.md file
- Release notes
- Project documentation

Thank you for contributing to Aivana RDP WPF! 🎉

---

**Last Updated:** 2025-11-27

