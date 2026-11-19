using System;
using System.Collections.Generic;
using System.Windows.Input;

using dosymep.WPF.ViewModels;

using RevitLintelsManager.Models;
using RevitLintelsManager.Services;

namespace RevitLintelsManager.ViewModels;

internal sealed class ManagerViewModel : BaseViewModel {
    public event Action CloseRequested;
    private bool _isBlocked;

    public ManagerViewModel(ManagerSessionState state, ISettingsCommandFactory commands) {
        State = state;
        CloseCommand = commands.Create(() => CloseRequested?.Invoke());
    }

    public ManagerSessionState State { get; }
    public IReadOnlyList<ManagerItemSnapshot> Items { get; } = Array.Empty<ManagerItemSnapshot>();
    public ICommand CloseCommand { get; }
    public bool IsBlocked {
        get => _isBlocked;
        private set => RaiseAndSetIfChanged(ref _isBlocked, value);
    }

    /// <summary>Применяет проверенное в API-контексте состояние исходного документа.</summary>
    public void SetBlocked(bool isBlocked) {
        State.IsBlocked = isBlocked;
        IsBlocked = isBlocked;
    }
}
