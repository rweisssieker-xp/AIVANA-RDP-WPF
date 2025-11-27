using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace Aivana_RDP_WPF.Infrastructure.Credentials;

/// <summary>
/// Windows Credential Manager API wrapper for secure credential storage.
/// </summary>
public class WindowsCredentialManager
{
    private readonly ILogger<WindowsCredentialManager> _logger;

    public WindowsCredentialManager(ILogger<WindowsCredentialManager> logger)
    {
        _logger = logger;
    }

    public bool SaveCredential(string targetName, string username, string password)
    {
        try
        {
            var credential = new NativeMethods.CREDENTIAL
            {
                Type = NativeMethods.CREDENTIAL_TYPE.GENERIC,
                TargetName = targetName,
                UserName = username,
                CredentialBlob = Marshal.StringToHGlobalUni(password),
                CredentialBlobSize = (uint)(password.Length * 2),
                Persist = NativeMethods.CREDENTIAL_PERSIST.LOCAL_MACHINE,
                AttributeCount = 0,
                Attributes = IntPtr.Zero,
                TargetAlias = null,
                Comment = null
            };

            var result = NativeMethods.CredWrite(ref credential, 0);
            Marshal.FreeHGlobal(credential.CredentialBlob);

            if (result)
            {
                _logger.LogInformation("Credential saved successfully for target: {TargetName}", targetName);
                return true;
            }
            else
            {
                var error = Marshal.GetLastWin32Error();
                _logger.LogError("Failed to save credential. Error code: {ErrorCode}", error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception saving credential for target: {TargetName}", targetName);
            return false;
        }
    }

    public (string? Username, string? Password) ReadCredential(string targetName)
    {
        try
        {
            IntPtr credentialPtr;
            if (NativeMethods.CredRead(targetName, NativeMethods.CREDENTIAL_TYPE.GENERIC, 0, out credentialPtr))
            {
                var credential = Marshal.PtrToStructure<NativeMethods.CREDENTIAL>(credentialPtr);
                var username = credential.UserName;
                var password = Marshal.PtrToStringUni(credential.CredentialBlob, (int)credential.CredentialBlobSize / 2);
                
                NativeMethods.CredFree(credentialPtr);
                
                _logger.LogInformation("Credential read successfully for target: {TargetName}", targetName);
                return (username, password);
            }
            else
            {
                var error = Marshal.GetLastWin32Error();
                _logger.LogWarning("Credential not found for target: {TargetName}. Error code: {ErrorCode}", targetName, error);
                return (null, null);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception reading credential for target: {TargetName}", targetName);
            return (null, null);
        }
    }

    public bool DeleteCredential(string targetName)
    {
        try
        {
            if (NativeMethods.CredDelete(targetName, NativeMethods.CREDENTIAL_TYPE.GENERIC, 0))
            {
                _logger.LogInformation("Credential deleted successfully for target: {TargetName}", targetName);
                return true;
            }
            else
            {
                var error = Marshal.GetLastWin32Error();
                _logger.LogWarning("Failed to delete credential. Error code: {ErrorCode}", error);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception deleting credential for target: {TargetName}", targetName);
            return false;
        }
    }

    private static class NativeMethods
    {
        [DllImport("advapi32.dll", EntryPoint = "CredWriteW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool CredWrite(ref CREDENTIAL credential, uint flags);

        [DllImport("advapi32.dll", EntryPoint = "CredReadW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool CredRead(string target, CREDENTIAL_TYPE type, int reservedFlag, out IntPtr credentialPtr);

        [DllImport("advapi32.dll", EntryPoint = "CredDeleteW", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool CredDelete(string target, CREDENTIAL_TYPE type, int flags);

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern void CredFree(IntPtr buffer);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct CREDENTIAL
        {
            public uint Flags;
            public CREDENTIAL_TYPE Type;
            public string TargetName;
            public string Comment;
            public System.Runtime.InteropServices.ComTypes.FILETIME LastWritten;
            public uint CredentialBlobSize;
            public IntPtr CredentialBlob;
            public CREDENTIAL_PERSIST Persist;
            public uint AttributeCount;
            public IntPtr Attributes;
            public string TargetAlias;
            public string UserName;
        }

        public enum CREDENTIAL_TYPE
        {
            GENERIC = 1,
            DOMAIN_PASSWORD = 2,
            DOMAIN_CERTIFICATE = 3,
            DOMAIN_VISIBLE_PASSWORD = 4,
            GENERIC_CERTIFICATE = 5,
            DOMAIN_EXTENDED = 6,
            MAXIMUM = 7
        }

        public enum CREDENTIAL_PERSIST
        {
            SESSION = 1,
            LOCAL_MACHINE = 2,
            ENTERPRISE = 3
        }
    }
}

