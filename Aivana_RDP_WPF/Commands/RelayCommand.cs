using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace Aivana_RDP_WPF.Commands;

/// <summary>
/// Wrapper for CommunityToolkit.Mvvm RelayCommand.
/// Provides a consistent interface for synchronous commands.
/// </summary>
public class RelayCommand : ICommand
{
    private readonly CommunityToolkit.Mvvm.Input.RelayCommand _command;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _command = canExecute != null
            ? new CommunityToolkit.Mvvm.Input.RelayCommand(execute, canExecute)
            : new CommunityToolkit.Mvvm.Input.RelayCommand(execute);
    }

    public event EventHandler? CanExecuteChanged
    {
        add => _command.CanExecuteChanged += value;
        remove => _command.CanExecuteChanged -= value;
    }

    public bool CanExecute(object? parameter) => _command.CanExecute(parameter);

    public void Execute(object? parameter) => _command.Execute(parameter);
}

