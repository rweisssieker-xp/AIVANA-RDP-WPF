namespace Aivana_RDP_WPF.Tests.TestHelpers;

/// <summary>
/// Constants used across tests
/// </summary>
public static class TestConstants
{
    // Test server addresses
    public const string TestServer1 = "192.168.1.100";
    public const string TestServer2 = "192.168.1.101";
    public const string TestServer3 = "192.168.1.102";
    
    // Test credentials
    public const string TestUsername = "testuser";
    public const string TestPassword = "testpassword";
    public const string TestDomain = "TESTDOMAIN";
    
    // Performance thresholds (NFRs)
    public const int MaxConnectionTimeSeconds = 3; // NFR4
    public const int MaxClipboardSyncLatencyMs = 100; // NFR7
    public const int TargetFrameRate = 60; // NFR8
    public const int MaxConcurrentConnections = 10; // NFR20
    
    // Test data limits
    public const int MaxTestConnections = 100;
    public const int MaxTestSessions = 50;
    public const int MaxTestTransfers = 100;
    
    // Test file paths
    public const string TestDataDirectory = "TestData";
    public const string TestRdpFilesDirectory = "TestData/RdpFiles";
    public const string TestJsonFilesDirectory = "TestData/JsonFiles";
    public const string TestCsvFilesDirectory = "TestData/CsvFiles";
}

