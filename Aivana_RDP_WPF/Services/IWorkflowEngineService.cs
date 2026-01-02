using Aivana_RDP_WPF.Models;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Service interface for workflow engine functionality
/// </summary>
public interface IWorkflowEngineService 
{
    Task<WorkflowExecution> ExecuteWorkflowAsync(string workflowId, Dictionary<string, object>? variables = null);
    Task PauseWorkflowAsync(string executionId);
    Task ResumeWorkflowAsync(string executionId);
    Task StopWorkflowAsync(string executionId);
    Task<List<WorkflowExecution>> GetActiveExecutionsAsync();
    Task<WorkflowExecution?> GetExecutionAsync(string executionId);
    Task<WorkflowDefinition> CreateWorkflowAsync(WorkflowDefinition workflow);
    Task UpdateWorkflowAsync(WorkflowDefinition workflow);
    Task DeleteWorkflowAsync(string workflowId);
    Task<List<WorkflowDefinition>> GetWorkflowsAsync();
    Task<WorkflowDefinition?> GetWorkflowAsync(string workflowId);
    event EventHandler<WorkflowExecution>? WorkflowStarted;
    event EventHandler<WorkflowExecution>? WorkflowCompleted;
    event EventHandler<WorkflowExecution>? WorkflowFailed;
    event EventHandler<WorkflowStep>? StepCompleted;
    event EventHandler<WorkflowStep>? StepFailed;
}
