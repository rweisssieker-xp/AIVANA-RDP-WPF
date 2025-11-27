using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service interface for session recording.
/// </summary>
public interface ISessionRecordingService
{
    Task<SessionRecording> StartRecordingAsync(int connectionProfileId, string outputPath, CancellationToken ct = default);
    Task StopRecordingAsync(int recordingId, CancellationToken ct = default);
    Task PauseRecordingAsync(int recordingId, CancellationToken ct = default);
    Task ResumeRecordingAsync(int recordingId, CancellationToken ct = default);
    Task<List<SessionRecording>> GetRecordingsAsync(int connectionProfileId, CancellationToken ct = default);
    Task DeleteRecordingAsync(int recordingId, CancellationToken ct = default);
    event EventHandler<SessionRecording>? RecordingStatusChanged;
}

