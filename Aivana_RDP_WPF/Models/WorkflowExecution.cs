namespace Aivana_RDP_WPF.Models;

/// <summary>
/// Represents a workflow execution instance
/// </summary>
public class WorkflowExecution 
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string WorkflowId { get; set; } = string.Empty;
    public string WorkflowName { get; set; } = string.Empty;
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Pending;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? Duration => CompletedAt.HasValue ? CompletedAt.Value - StartedAt : null;
    public List<StepExecution> StepExecutions { get; set; } = new();
    public Dictionary<string, object> Variables { get; set; } = new();
    public string? CurrentStepId { get; set; }
    public int CurrentStepIndex { get; set; }
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }
    public ExecutionTrigger Trigger { get; set; }
    public string StartedBy { get; set; } = string.Empty;
}

/// <summary>
/// Status of workflow execution
/// </summary>
public enum ExecutionStatus 
{
    Pending,
    Running,
    Paused,
    Completed,
    Failed,
    Cancelled,
    Timeout
}

/// <summary>
/// Execution details for a single step
/// </summary>
public class StepExecution 
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string StepId { get; set; } = string.Empty;
    public string StepName { get; set; } = string.Empty;
    public ExecutionStatus Status { get; set; } = ExecutionStatus.Pending;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public TimeSpan? Duration => CompletedAt.HasValue ? CompletedAt.Value - StartedAt : null;
    public string? ErrorMessage { get; set; }
    public Exception? Exception { get; set; }
    public Dictionary<string, object> InputParameters { get; set; } = new();
    public Dictionary<string, object> OutputParameters { get; set; } = new();
    public int AttemptCount { get; set; } = 1;
}

/// <summary>
/// What triggered the workflow execution
/// </summary>
public enum ExecutionTrigger 
{
    Manual,
    Scheduled,
    Automatic,
    Api
}
