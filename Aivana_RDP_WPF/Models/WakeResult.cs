namespace Aivana_RDP_WPF.Models;

/// <summary>
/// Result of Wake-on-LAN operation
/// </summary>
public class WakeResult 
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string MacAddress { get; set; } = string.Empty;
    public string BroadcastAddress { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;

    public static WakeResult Successful(string macAddress, string broadcastAddress, string message = "Wake packet sent successfully")
    {
        return new WakeResult
        {
            Success = true,
            Message = message,
            MacAddress = macAddress,
            BroadcastAddress = broadcastAddress,
            SentAt = DateTime.UtcNow
        };
    }

    public static WakeResult Failed(string message, string macAddress, string broadcastAddress)
    {
        return new WakeResult
        {
            Success = false,
            Message = message,
            MacAddress = macAddress,
            BroadcastAddress = broadcastAddress,
            SentAt = DateTime.UtcNow
        };
    }
}
