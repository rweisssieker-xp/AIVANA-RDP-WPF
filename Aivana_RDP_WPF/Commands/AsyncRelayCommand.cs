using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Aivana_RDP_WPF.Commands;

/// <summary>
/// Wrapper for CommunityToolkit.Mvvm AsyncRelayCommand.
/// Provides a consistent interface for asynchronous commands.
/// </summary>
public class AsyncRelayCommand : ICommand
{
    private readonly CommunityToolkit.Mvvm.Input.AsyncRelayCommand _command;

    public AsyncRelayCommand(Func<Task> execute, Func<bool>? canExecute = null)
    {
        _command = canExecute != null
            ? new CommunityToolkit.Mvvm.Input.AsyncRelayCommand(execute, canExecute)
            : new CommunityToolkit.Mvvm.Input.AsyncRelayCommand(execute);
    }

    public event EventHandler? CanExecuteChanged
    {
        add => _command.CanExecuteChanged += value;
        remove => _command.CanExecuteChanged -= value;
    }

    public bool CanExecute(object? parameter) => _command.CanExecute(parameter);

    public void Execute(object? parameter) => _command.Execute(parameter);
}

