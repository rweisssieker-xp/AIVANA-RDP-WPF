using Aivana_RDP_WPF.Models.AI;
using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Services.AI;

/// <summary>
/// Service interface for machine learning operations
/// </summary>
public interface IMachineLearningService 
{
    Task InitializeModelAsync();
    Task TrainModelAsync(List<TrainingData> trainingData);
    Task<double> PredictConnectionSuccessAsync(ConnectionProfile profile, DateTime time);
    Task<double> PredictPerformanceAsync(ConnectionProfile profile);
    Task<List<UserBehaviorPattern>> DetectPatternsAsync(List<UserActivity> activities);
    Task<SecurityRiskLevel> AssessSecurityRiskAsync(ConnectionProfile profile);
    Task<Dictionary<string, double>> GetFeatureImportanceAsync(string profileId);
    Task<bool> IsAnomalyDetectedAsync(PerformanceMetrics metrics);
    event EventHandler<ModelTrainingProgress>? TrainingProgress;
    event EventHandler<string>? ModelUpdated;
}

/// <summary>
/// Model training progress information
/// </summary>
public class ModelTrainingProgress 
{
    public int Epoch { get; set; }
    public int TotalEpochs { get; set; }
    public double Loss { get; set; }
    public double Accuracy { get; set; }
    public TimeSpan ElapsedTime { get; set; }
    public TimeSpan EstimatedTimeRemaining { get; set; }
}
