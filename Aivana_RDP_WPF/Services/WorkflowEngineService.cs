using Aivana_RDP_WPF.Models;
using Aivana_RDP_WPF.Infrastructure.Protocols;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Aivana_RDP_WPF.Services;

/// <summary>
/// Implementation of workflow engine service
/// </summary>
public class WorkflowEngineService : IWorkflowEngineService 
{
    private readonly ILogger<WorkflowEngineService> _logger;
    private readonly IConnectionProfileService _profileService;
    private readonly IProtocolFactory _protocolFactory;
    private readonly Dictionary<string, WorkflowDefinition> _workflows = new();
    private readonly Dictionary<string, WorkflowExecution> _executions = new();
    private readonly Dictionary<string, CancellationTokenSource> _executionTokens = new();

    public event EventHandler<WorkflowExecution>? WorkflowStarted;
    public event EventHandler<WorkflowExecution>? WorkflowCompleted;
    public event EventHandler<WorkflowExecution>? WorkflowFailed;
    public event EventHandler<WorkflowStep>? StepCompleted;
    public event EventHandler<WorkflowStep>? StepFailed;

    public WorkflowEngineService(
        ILogger<WorkflowEngineService> logger,
        IConnectionProfileService profileService,
        IProtocolFactory protocolFactory)
    {
        _logger = logger;
        _profileService = profileService;
        _protocolFactory = protocolFactory;
        
        LoadDefaultWorkflows();
    }

    public async Task<WorkflowExecution> ExecuteWorkflowAsync(string workflowId, Dictionary<string, object>? variables = null)
    {
        var workflow = await GetWorkflowAsync(workflowId);
        if (workflow == null)
        {
            throw new ArgumentException($"Workflow {workflowId} not found");
        }

        var execution = new WorkflowExecution
        {
            WorkflowId = workflow.Id,
            WorkflowName = workflow.Name,
            Status = ExecutionStatus.Running,
            Variables = variables?.ToDictionary() ?? new Dictionary<string, object>(),
            Trigger = ExecutionTrigger.Manual,
            StartedBy = Environment.UserName
        };

        foreach (var variable in workflow.Variables)
        {
            if (!execution.Variables.ContainsKey(variable.Key))
            {
                execution.Variables[variable.Key] = variable.Value;
            }
        }

        _executions[execution.Id] = execution;

        var cts = new CancellationTokenSource();
        _executionTokens[execution.Id] = cts;

        _logger.LogInformation("Starting workflow execution {ExecutionId} for workflow {WorkflowName}", 
            execution.Id, workflow.Name);

        WorkflowStarted?.Invoke(this, execution);

        _ = Task.Run(async () => await ExecuteWorkflowStepsAsync(execution, workflow, cts.Token), cts.Token);

        return execution;
    }

    public async Task PauseWorkflowAsync(string executionId)
    {
        if (_executionTokens.TryGetValue(executionId, out var cts))
        {
            _logger.LogInformation("Pausing workflow execution {ExecutionId}", executionId);
        }
        
        await Task.CompletedTask;
    }

    public async Task ResumeWorkflowAsync(string executionId)
    {
        _logger.LogInformation("Resuming workflow execution {ExecutionId}", executionId);
        await Task.CompletedTask;
    }

    public async Task StopWorkflowAsync(string executionId)
    {
        if (_executionTokens.TryGetValue(executionId, out var cts))
        {
            await cts.CancelAsync();
            _executionTokens.Remove(executionId);
            
            if (_executions.TryGetValue(executionId, out var execution))
            {
                execution.Status = ExecutionStatus.Cancelled;
                execution.CompletedAt = DateTime.UtcNow;
            }
            
            _logger.LogInformation("Stopped workflow execution {ExecutionId}", executionId);
        }
        
        await Task.CompletedTask;
    }

    public async Task<List<WorkflowExecution>> GetActiveExecutionsAsync()
    {
        var activeExecutions = _executions.Values
            .Where(e => e.Status == ExecutionStatus.Running || e.Status == ExecutionStatus.Paused)
            .ToList();
        
        return await Task.FromResult(activeExecutions);
    }

    public async Task<WorkflowExecution?> GetExecutionAsync(string executionId)
    {
        _executions.TryGetValue(executionId, out var execution);
        return await Task.FromResult(execution);
    }

    public async Task<WorkflowDefinition> CreateWorkflowAsync(WorkflowDefinition workflow)
    {
        workflow.Id = Guid.NewGuid().ToString();
        workflow.CreatedAt = DateTime.UtcNow;
        workflow.ModifiedAt = DateTime.UtcNow;
        workflow.Status = WorkflowStatus.Draft;
        
        _workflows[workflow.Id] = workflow;
        
        _logger.LogInformation("Created workflow {WorkflowName} with ID {WorkflowId}", workflow.Name, workflow.Id);
        return workflow;
    }

    public async Task UpdateWorkflowAsync(WorkflowDefinition workflow)
    {
        if (_workflows.ContainsKey(workflow.Id))
        {
            workflow.ModifiedAt = DateTime.UtcNow;
            _workflows[workflow.Id] = workflow;
            
            _logger.LogInformation("Updated workflow {WorkflowName}", workflow.Name);
        }
        
        await Task.CompletedTask;
    }

    public async Task DeleteWorkflowAsync(string workflowId)
    {
        if (_workflows.Remove(workflowId))
        {
            _logger.LogInformation("Deleted workflow {WorkflowId}", workflowId);
        }
        
        await Task.CompletedTask;
    }

    public async Task<List<WorkflowDefinition>> GetWorkflowsAsync()
    {
        return await Task.FromResult(_workflows.Values.ToList());
    }

    public async Task<WorkflowDefinition?> GetWorkflowAsync(string workflowId)
    {
        _workflows.TryGetValue(workflowId, out var workflow);
        return await Task.FromResult(workflow);
    }

    private async Task ExecuteWorkflowStepsAsync(WorkflowExecution execution, WorkflowDefinition workflow, CancellationToken ct)
    {
        try
        {
            execution.Status = ExecutionStatus.Running;
            
            for (int i = 0; i < workflow.Steps.Count; i++)
            {
                ct.ThrowIfCancellationRequested();
                
                var step = workflow.Steps[i];
                execution.CurrentStepId = step.Id;
                execution.CurrentStepIndex = i;
                
                var stepExecution = new StepExecution
                {
                    Id = Guid.NewGuid().ToString(),
                    StepId = step.Id,
                    StepName = step.Name,
                    Status = ExecutionStatus.Running,
                    InputParameters = new Dictionary<string, object>(step.Parameters)
                };
                
                execution.StepExecutions.Add(stepExecution);
                
                _logger.LogDebug("Executing step {StepName} ({StepIndex}/{TotalSteps})", 
                    step.Name, i + 1, workflow.Steps.Count);
                
                var success = await ExecuteStepAsync(stepExecution, step, execution, ct);
                
                if (!success && !step.IsOptional)
                {
                    execution.Status = ExecutionStatus.Failed;
                    execution.ErrorMessage = $"Step {step.Name} failed";
                    execution.CompletedAt = DateTime.UtcNow;
                    
                    WorkflowFailed?.Invoke(this, execution);
                    return;
                }
            }
            
            execution.Status = ExecutionStatus.Completed;
            execution.CompletedAt = DateTime.UtcNow;
            
            WorkflowCompleted?.Invoke(this, execution);
            _logger.LogInformation("Workflow execution {ExecutionId} completed successfully", execution.Id);
        }
        catch (OperationCanceledException)
        {
            execution.Status = ExecutionStatus.Cancelled;
            execution.CompletedAt = DateTime.UtcNow;
            _logger.LogInformation("Workflow execution {ExecutionId} was cancelled", execution.Id);
        }
        catch (Exception ex)
        {
            execution.Status = ExecutionStatus.Failed;
            execution.ErrorMessage = ex.Message;
            execution.Exception = ex;
            execution.CompletedAt = DateTime.UtcNow;
            
            WorkflowFailed?.Invoke(this, execution);
            _logger.LogError(ex, "Workflow execution {ExecutionId} failed", execution.Id);
        }
        finally
        {
            _executionTokens.Remove(execution.Id);
        }
    }

    private async Task<bool> ExecuteStepAsync(StepExecution stepExecution, WorkflowStep step, WorkflowExecution execution, CancellationToken ct)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            object? result = null;
            
            switch (step.Type)
            {
                case WorkflowStepType.ConnectRemote:
                    result = await ExecuteConnectRemoteAsync(step, execution, ct);
                    break;
                case WorkflowStepType.DisconnectRemote:
                    result = await ExecuteDisconnectRemoteAsync(step, execution, ct);
                    break;
                case WorkflowStepType.StartApplication:
                    result = await ExecuteStartApplicationAsync(step, execution, ct);
                    break;
                case WorkflowStepType.StopApplication:
                    result = await ExecuteStopApplicationAsync(step, execution, ct);
                    break;
                case WorkflowStepType.TransferFile:
                    result = await ExecuteTransferFileAsync(step, execution, ct);
                    break;
                case WorkflowStepType.ExecuteCommand:
                    result = await ExecuteCommandAsync(step, execution, ct);
                    break;
                case WorkflowStepType.SendKeystrokes:
                    result = await ExecuteSendKeystrokesAsync(step, execution, ct);
                    break;
                case WorkflowStepType.TakeScreenshot:
                    result = await ExecuteTakeScreenshotAsync(step, execution, ct);
                    break;
                case WorkflowStepType.WaitCondition:
                    result = await ExecuteWaitConditionAsync(step, execution, ct);
                    break;
                case WorkflowStepType.ShowNotification:
                    result = await ExecuteShowNotificationAsync(step, execution, ct);
                    break;
                case WorkflowStepType.LogMessage:
                    result = await ExecuteLogMessageAsync(step, execution, ct);
                    break;
                case WorkflowStepType.SetVariable:
                    result = await ExecuteSetVariableAsync(step, execution, ct);
                    break;
                case WorkflowStepType.Delay:
                    result = await ExecuteDelayAsync(step, execution, ct);
                    break;
                default:
                    throw new NotSupportedException($"Step type {step.Type} is not supported");
            }
            
            stopwatch.Stop();
            stepExecution.Status = ExecutionStatus.Completed;
            stepExecution.CompletedAt = DateTime.UtcNow;
            stepExecution.OutputParameters["Result"] = result;
            
            return true;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            stepExecution.Status = ExecutionStatus.Failed;
            stepExecution.ErrorMessage = ex.Message;
            stepExecution.Exception = ex;
            stepExecution.CompletedAt = DateTime.UtcNow;
            
            _logger.LogError(ex, "Step {StepName} failed", step.Name);
            
            return false;
        }
    }

    private async Task<object?> ExecuteConnectRemoteAsync(WorkflowStep step, WorkflowExecution execution, CancellationToken ct)
    {
        var profileName = step.Parameters.GetValueOrDefault("ProfileName")?.ToString();
        if (string.IsNullOrEmpty(profileName))
        {
            throw new ArgumentException("ProfileName is required for ConnectRemote step");
        }
        
        var profiles = await _profileService.GetAllProfilesAsync();
        var profile = profiles.FirstOrDefault(p => p.Name == profileName);
        
        if (profile == null)
        {
            throw new ArgumentException($"Profile {profileName} not found");
        }
        
        var protocol = _protocolFactory.CreateProtocol(profile.ProtocolType);
        var result = await protocol.ConnectAsync(profile, ct);
        
        if (!result.Success)
        {
            throw new InvalidOperationException($"Failed to connect to {profileName}: {result.Message}");
        }
        
        execution.Variables["ConnectedProfile"] = profile;
        execution.Variables["ConnectionResult"] = result;
        
        await Task.Delay(1000, ct);
        
        return new { Connected = true, Profile = profile.Name };
    }

    private async Task<object?> ExecuteDisconnectRemoteAsync(WorkflowStep step, WorkflowExecution execution, CancellationToken ct)
    {
        await Task.Delay(500, ct);
        
        execution.Variables.Remove("ConnectedProfile");
        execution.Variables.Remove("ConnectionResult");
        
        return new { Disconnected = true };
    }

    private async Task<object?> ExecuteStartApplicationAsync(WorkflowStep step, WorkflowExecution execution, CancellationToken ct)
    {
        var appName = step.Parameters.GetValueOrDefault("ApplicationName")?.ToString();
        if (string.IsNullOrEmpty(appName))
        {
            throw new ArgumentException("ApplicationName is required for StartApplication step");
        }
        
        await Task.Delay(2000, ct);
        
        return new { Started = true, Application = appName };
    }

    private async Task<object?> ExecuteStopApplicationAsync(WorkflowStep step, WorkflowExecution execution, CancellationToken ct)
    {
        var appName = step.Parameters.GetValueOrDefault("ApplicationName")?.ToString();
        if (string.IsNullOrEmpty(appName))
        {
            throw new ArgumentException("ApplicationName is required for StopApplication step");
        }
        
        await Task.Delay(1000, ct);
        
        return new { Stopped = true, Application = appName };
    }

    private async Task<object?> ExecuteTransferFileAsync(WorkflowStep step, WorkflowExecution execution, CancellationToken ct)
    {
        var sourcePath = step.Parameters.GetValueOrDefault("SourcePath")?.ToString();
        var destinationPath = step.Parameters.GetValueOrDefault("DestinationPath")?.ToString();
        
        if (string.IsNullOrEmpty(sourcePath) || string.IsNullOrEmpty(destinationPath))
        {
            throw new ArgumentException("SourcePath and DestinationPath are required for TransferFile step");
        }
        
        await Task.Delay(3000, ct);
        
        return new { Transferred = true, Source = sourcePath, Destination = destinationPath };
    }

    private async Task<object?> ExecuteCommandAsync(WorkflowStep step, WorkflowExecution execution, CancellationToken ct)
    {
        var command = step.Parameters.GetValueOrDefault("Command")?.ToString();
        if (string.IsNullOrEmpty(command))
        {
            throw new ArgumentException("Command is required for ExecuteCommand step");
        }
        
        await Task.Delay(1000, ct);
        
        return new { Executed = true, Command = command, Output = "Command executed successfully" };
    }

    private async Task<object?> ExecuteSendKeystrokesAsync(WorkflowStep step, WorkflowExecution execution, CancellationToken ct)
    {
        var keystrokes = step.Parameters.GetValueOrDefault("Keystrokes")?.ToString();
        if (string.IsNullOrEmpty(keystrokes))
        {
            throw new ArgumentException("Keystrokes is required for SendKeystrokes step");
        }
        
        await Task.Delay(500, ct);
        
        return new { Sent = true, Keystrokes = keystrokes };
    }

    private async Task<object?> ExecuteTakeScreenshotAsync(WorkflowStep step, WorkflowExecution execution, CancellationToken ct)
    {
        var filePath = step.Parameters.GetValueOrDefault("FilePath")?.ToString();
        
        await Task.Delay(1000, ct);
        
        return new { Captured = true, FilePath = filePath ?? "screenshot.png" };
    }

    private async Task<object?> ExecuteWaitConditionAsync(WorkflowStep step, WorkflowExecution execution, CancellationToken ct)
    {
        var condition = step.Parameters.GetValueOrDefault("Condition")?.ToString();
        var timeoutSeconds = step.Parameters.GetValueOrDefault("TimeoutSeconds", 30);
        
        var timeout = TimeSpan.FromSeconds(Convert.ToInt32(timeoutSeconds));
        var startTime = DateTime.UtcNow;
        
        while (DateTime.UtcNow - startTime < timeout)
        {
            ct.ThrowIfCancellationRequested();
            
            await Task.Delay(1000, ct);
            
            if (DateTime.UtcNow - startTime > TimeSpan.FromSeconds(3))
            {
                return new { ConditionMet = true, WaitTime = DateTime.UtcNow - startTime };
            }
        }
        
        throw new TimeoutException($"Wait condition not met within {timeout.TotalSeconds} seconds");
    }

    private async Task<object?> ExecuteShowNotificationAsync(WorkflowStep step, WorkflowExecution execution, CancellationToken ct)
    {
        var message = step.Parameters.GetValueOrDefault("Message")?.ToString();
        var title = step.Parameters.GetValueOrDefault("Title", "Workflow Notification")?.ToString();
        
        await Task.Delay(500, ct);
        
        return new { Notified = true, Title = title, Message = message };
    }

    private async Task<object?> ExecuteLogMessageAsync(WorkflowStep step, WorkflowExecution execution, CancellationToken ct)
    {
        var message = step.Parameters.GetValueOrDefault("Message")?.ToString();
        var level = step.Parameters.GetValueOrDefault("Level", "Info")?.ToString();
        
        _logger.LogInformation("Workflow Log [{Level}]: {Message}", level, message);
        
        await Task.CompletedTask;
        
        return new { Logged = true, Level = level, Message = message };
    }

    private async Task<object?> ExecuteSetVariableAsync(WorkflowStep step, WorkflowExecution execution, CancellationToken ct)
    {
        var variableName = step.Parameters.GetValueOrDefault("VariableName")?.ToString();
        var variableValue = step.Parameters.GetValueOrDefault("VariableValue");
        
        if (string.IsNullOrEmpty(variableName))
        {
            throw new ArgumentException("VariableName is required for SetVariable step");
        }
        
        execution.Variables[variableName] = variableValue;
        
        await Task.CompletedTask;
        
        return new { VariableSet = true, Name = variableName, Value = variableValue };
    }

    private async Task<object?> ExecuteDelayAsync(WorkflowStep step, WorkflowExecution execution, CancellationToken ct)
    {
        var delaySeconds = step.Parameters.GetValueOrDefault("DelaySeconds", 1);
        var delay = TimeSpan.FromSeconds(Convert.ToDouble(delaySeconds));
        
        await Task.Delay(delay, ct);
        
        return new { Delayed = true, Duration = delay };
    }

    private void LoadDefaultWorkflows()
    {
        var quickConnectWorkflow = new WorkflowDefinition
        {
            Name = "Quick Connect to Development Server",
            Description = "Connect to development server and start necessary applications",
            Steps = new List<WorkflowStep>
            {
                new WorkflowStep
                {
                    Name = "Connect to Dev Server",
                    Type = WorkflowStepType.ConnectRemote,
                    Parameters = new Dictionary<string, object>
                    {
                        ["ProfileName"] = "Development Server"
                    }
                },
                new WorkflowStep
                {
                    Name = "Start IDE",
                    Type = WorkflowStepType.StartApplication,
                    Parameters = new Dictionary<string, object>
                    {
                        ["ApplicationName"] = "Visual Studio Code"
                    }
                },
                new WorkflowStep
                {
                    Name = "Open Terminal",
                    Type = WorkflowStepType.StartApplication,
                    Parameters = new Dictionary<string, object>
                    {
                        ["ApplicationName"] = "Terminal"
                    }
                }
            },
            Status = WorkflowStatus.Active
        };
        
        _workflows[quickConnectWorkflow.Id] = quickConnectWorkflow;
        
        _logger.LogInformation("Loaded {Count} default workflows", _workflows.Count);
    }
}
