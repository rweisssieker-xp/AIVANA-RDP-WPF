using System.IO;
using Microsoft.Extensions.Logging;
using Aivana_RDP_WPF.Models;
using System.Collections.Concurrent;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service implementation for session recording.
/// </summary>
public class SessionRecordingService : ISessionRecordingService
{
    private readonly ILogger<SessionRecordingService> _logger;
    private readonly ConcurrentDictionary<int, SessionRecording> _activeRecordings = new();
    private int _nextRecordingId = 1;

    public event EventHandler<SessionRecording>? RecordingStatusChanged;

    public SessionRecordingService(ILogger<SessionRecordingService> logger)
    {
        _logger = logger;
    }

    public Task<SessionRecording> StartRecordingAsync(int connectionProfileId, string outputPath, CancellationToken ct = default)
    {
        _logger.LogInformation("Starting session recording for connection {ConnectionId} to {OutputPath}", connectionProfileId, outputPath);
        
        var recording = new SessionRecording
        {
            Id = _nextRecordingId++,
            ConnectionProfileId = connectionProfileId,
            FilePath = outputPath,
            StartedAt = DateTime.UtcNow,
            Status = RecordingStatus.Recording
        };

        _activeRecordings[recording.Id] = recording;
        
        // In real implementation, use Windows Media Foundation to capture RDP session
        // For now, create placeholder file
        try
        {
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            // Create placeholder file
            File.WriteAllText(outputPath, $"Recording started at {recording.StartedAt:yyyy-MM-dd HH:mm:ss}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating recording file {FilePath}", outputPath);
            recording.Status = RecordingStatus.Error;
        }

        RecordingStatusChanged?.Invoke(this, recording);
        return Task.FromResult(recording);
    }

    public Task StopRecordingAsync(int recordingId, CancellationToken ct = default)
    {
        _logger.LogInformation("Stopping recording {RecordingId}", recordingId);
        
        if (_activeRecordings.TryGetValue(recordingId, out var recording))
        {
            recording.Status = RecordingStatus.Stopped;
            recording.StoppedAt = DateTime.UtcNow;
            
            if (File.Exists(recording.FilePath))
            {
                recording.FileSizeBytes = new FileInfo(recording.FilePath).Length;
            }
            
            RecordingStatusChanged?.Invoke(this, recording);
        }
        
        return Task.CompletedTask;
    }

    public Task PauseRecordingAsync(int recordingId, CancellationToken ct = default)
    {
        _logger.LogInformation("Pausing recording {RecordingId}", recordingId);
        
        if (_activeRecordings.TryGetValue(recordingId, out var recording))
        {
            recording.Status = RecordingStatus.Paused;
            RecordingStatusChanged?.Invoke(this, recording);
        }
        
        return Task.CompletedTask;
    }

    public Task ResumeRecordingAsync(int recordingId, CancellationToken ct = default)
    {
        _logger.LogInformation("Resuming recording {RecordingId}", recordingId);
        
        if (_activeRecordings.TryGetValue(recordingId, out var recording))
        {
            recording.Status = RecordingStatus.Recording;
            RecordingStatusChanged?.Invoke(this, recording);
        }
        
        return Task.CompletedTask;
    }

    public Task<List<SessionRecording>> GetRecordingsAsync(int connectionProfileId, CancellationToken ct = default)
    {
        _logger.LogInformation("Retrieving recordings for connection {ConnectionId}", connectionProfileId);
        var recordings = _activeRecordings.Values
            .Where(r => r.ConnectionProfileId == connectionProfileId)
            .OrderByDescending(r => r.StartedAt)
            .ToList();
        return Task.FromResult(recordings);
    }

    public Task DeleteRecordingAsync(int recordingId, CancellationToken ct = default)
    {
        _logger.LogInformation("Deleting recording {RecordingId}", recordingId);
        
        if (_activeRecordings.TryRemove(recordingId, out var recording))
        {
            try
            {
                if (File.Exists(recording.FilePath))
                {
                    File.Delete(recording.FilePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting recording file {FilePath}", recording.FilePath);
            }
        }
        
        return Task.CompletedTask;
    }
}

