using System;
using System.Runtime.InteropServices;

namespace Aivana_RDP_WPF.Tests.TestHelpers.Fixtures;

/// <summary>
/// Mock fixture for Windows Credential Manager testing
/// </summary>
public class CredentialManagerFixture : IDisposable
{
    private readonly List<string> _testCredentials = new();

    public void AddTestCredential(string target, string username, string password)
    {
        _testCredentials.Add(target);
        // In real implementation, this would use CredWrite API
        // For testing, we'll mock this behavior
    }

    public bool CredentialExists(string target)
    {
        return _testCredentials.Contains(target);
    }

    public void RemoveCredential(string target)
    {
        _testCredentials.Remove(target);
    }

    public void ClearAll()
    {
        _testCredentials.Clear();
    }

    public void Dispose()
    {
        ClearAll();
    }
}

