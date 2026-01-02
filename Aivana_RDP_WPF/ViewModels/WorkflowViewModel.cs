using Aivana_RDP_WPF.Models;
using Aivana_RDP_WPF.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Aivana_RDP_WPF.ViewModels;

/// <summary>
/// ViewModel for workflow management
/// </summary>
public partial class WorkflowViewModel : ObservableObject
{
    private readonly IWorkflowEngineService _workflowEngine;

    [ObservableProperty]
    private ObservableCollection<WorkflowDefinition> _workflows = new();

    [ObservableProperty]
    private ObservableCollection<WorkflowExecution> _activeExecutions = new();

    [ObservableProperty]
    private WorkflowDefinition? _selectedWorkflow;

    [ObservableProperty]
    private WorkflowExecution? _selectedExecution;

    [ObservableProperty]
    private bool _isExecuting;

    public WorkflowViewModel(IWorkflowEngineService workflowEngine)
    {
        _workflowEngine = workflowEngine;

        _workflowEngine.WorkflowStarted += OnWorkflowStarted;
        _workflowEngine.WorkflowCompleted += OnWorkflowCompleted;
        _workflowEngine.WorkflowFailed += OnWorkflowFailed;

        _ = Task.Run(LoadWorkflowsAsync);
        _ = Task.Run(RefreshActiveExecutionsAsync);
    }

    [RelayCommand]
    private async Task ExecuteWorkflowAsync(WorkflowDefinition workflow)
    {
        if (workflow == null) return;

        try
        {
            IsExecuting = true;
            var execution = await _workflowEngine.ExecuteWorkflowAsync(workflow.Id);
            
            await RefreshActiveExecutionsAsync();
        }
        catch (Exception ex)
        {
            // TODO: Show error message
        }
        finally
        {
            IsExecuting = false;
        }
    }

    [RelayCommand]
    private async Task StopExecutionAsync(WorkflowExecution execution)
    {
        if (execution == null) return;

        try
        {
            await _workflowEngine.StopWorkflowAsync(execution.Id);
            await RefreshActiveExecutionsAsync();
        }
        catch (Exception ex)
        {
            // TODO: Show error message
        }
    }

    [RelayCommand]
    private async Task CreateWorkflowAsync()
    {
        try
        {
            var newWorkflow = new WorkflowDefinition
            {
                Name = "New Workflow",
                Description = "Describe your workflow here",
                Status = WorkflowStatus.Draft,
                CreatedBy = Environment.UserName
            };

            var createdWorkflow = await _workflowEngine.CreateWorkflowAsync(newWorkflow);
            await LoadWorkflowsAsync();
            
            SelectedWorkflow = createdWorkflow;
        }
        catch (Exception ex)
        {
            // TODO: Show error message
        }
    }

    [RelayCommand]
    private async Task DeleteWorkflowAsync(WorkflowDefinition workflow)
    {
        if (workflow == null) return;

        try
        {
            await _workflowEngine.DeleteWorkflowAsync(workflow.Id);
            await LoadWorkflowsAsync();
            
            if (SelectedWorkflow?.Id == workflow.Id)
            {
                SelectedWorkflow = null;
            }
        }
        catch (Exception ex)
        {
            // TODO: Show error message
        }
    }

    private async Task LoadWorkflowsAsync()
    {
        try
        {
            var workflows = await _workflowEngine.GetWorkflowsAsync();
            
            App.Current.Dispatcher.Invoke(() =>
            {
                Workflows.Clear();
                foreach (var workflow in workflows)
                {
                    Workflows.Add(workflow);
                }
            });
        }
        catch (Exception ex)
        {
            // TODO: Handle error
        }
    }

    private async Task RefreshActiveExecutionsAsync()
    {
        try
        {
            var executions = await _workflowEngine.GetActiveExecutionsAsync();
            
            App.Current.Dispatcher.Invoke(() =>
            {
                ActiveExecutions.Clear();
                foreach (var execution in executions)
                {
                    ActiveExecutions.Add(execution);
                }
            });
        }
        catch (Exception ex)
        {
            // TODO: Handle error
        }
    }

    private void OnWorkflowStarted(object? sender, WorkflowExecution execution)
    {
        App.Current.Dispatcher.Invoke(async () =>
        {
            await RefreshActiveExecutionsAsync();
        });
    }

    private void OnWorkflowCompleted(object? sender, WorkflowExecution execution)
    {
        App.Current.Dispatcher.Invoke(async () =>
        {
            await RefreshActiveExecutionsAsync();
        });
    }

    private void OnWorkflowFailed(object? sender, WorkflowExecution execution)
    {
        App.Current.Dispatcher.Invoke(async () =>
        {
            await RefreshActiveExecutionsAsync();
        });
    }
}
