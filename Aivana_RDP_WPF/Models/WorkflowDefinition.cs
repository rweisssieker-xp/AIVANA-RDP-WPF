namespace Aivana_RDP_WPF.Models;

/// <summary>
/// Represents a workflow definition with steps and triggers
/// </summary>
public class WorkflowDefinition 
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<WorkflowStep> Steps { get; set; } = new();
    public WorkflowTrigger Trigger { get; set; } = new();
    public WorkflowStatus Status { get; set; } = WorkflowStatus.Draft;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ModifiedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
    public Dictionary<string, object> Variables { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public bool IsEnabled { get; set; } = true;
    public int ExecutionCount { get; set; }
    public TimeSpan AverageExecutionTime { get; set; }
}

/// <summary>
/// Individual step in a workflow
/// </summary>
public class WorkflowStep 
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public WorkflowStepType Type { get; set; }
    public Dictionary<string, object> Parameters { get; set; } = new();
    public List<WorkflowStep> NextSteps { get; set; } = new();
    public WorkflowStepCondition? Condition { get; set; }
    public bool IsOptional { get; set; }
    public int RetryCount { get; set; } = 3;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromMinutes(5);
    public StepExecutionResult? LastResult { get; set; }
}

/// <summary>
/// Types of workflow steps
/// </summary>
public enum WorkflowStepType 
{
    ConnectRemote,
    DisconnectRemote,
    StartApplication,
    StopApplication,
    TransferFile,
    ExecuteCommand,
    SendKeystrokes,
    TakeScreenshot,
    WaitCondition,
    ShowNotification,
    LogMessage,
    SetVariable,
    IfCondition,
    LoopSteps,
    ParallelSteps,
    Delay,
    CustomScript
}

/// <summary>
/// Condition for workflow step execution
/// </summary>
public class WorkflowStepCondition 
{
    public string VariableName { get; set; } = string.Empty;
    public ConditionOperator Operator { get; set; }
    public object ExpectedValue { get; set; } = string.Empty;
    public string LogicalOperator { get; set; } = "AND";
}

/// <summary>
/// Operators for step conditions
/// </summary>
public enum ConditionOperator 
{
    Equals,
    NotEquals,
    GreaterThan,
    LessThan,
    Contains,
    StartsWith,
    EndsWith,
    IsNull,
    IsNotNull
}

/// <summary>
/// Trigger for workflow execution
/// </summary>
public class WorkflowTrigger 
{
    public TriggerType Type { get; set; }
    public Dictionary<string, object> Configuration { get; set; } = new();
    public bool IsEnabled { get; set; } = true;
}

/// <summary>
/// Types of workflow triggers
/// </summary>
public enum TriggerType 
{
    Manual,
    Scheduled,
    OnConnection,
    OnDisconnection,
    OnFileChange,
    OnSystemEvent,
    OnHotkey,
    OnApplicationStart
}

/// <summary>
/// Status of a workflow
/// </summary>
public enum WorkflowStatus 
{
    Draft,
    Active,
    Paused,
    Disabled,
    Error
}

/// <summary>
/// Result of step execution
/// </summary>
public class StepExecutionResult 
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Result { get; set; }
    public DateTime ExecutedAt { get; set; } = DateTime.UtcNow;
    public TimeSpan ExecutionTime { get; set; }
    public Exception? Exception { get; set; }
    public Dictionary<string, object> OutputVariables { get; set; } = new();
}
