using System;
using System.Linq;

using dosymep.SimpleServices;

using RevitLintelsManager.Models;

namespace RevitLintelsManager.ViewModels;

/// <summary>Индивидуальная строка на независимом снимке без операций модели.</summary>
internal sealed class ManagerItemViewModel : ManagerRowViewModel {
    public event Action<ManagerItemViewModel, bool, ManagerAction> StateChanged;
    private bool _isChecked = true;
    private bool _isLastInGroup;
    private ManagerAction? _manualAction;
    private ManagerActionViewModel _selectedAction;

    /// <summary>Создаёт отмеченную строку с допустимым действием по умолчанию.</summary>
    public ManagerItemViewModel(ManagerItemSnapshot snapshot, ManagerElementPresentation opening,
        ManagerElementPresentation lintel, ILocalizationService localization)
        : base(opening, lintel, snapshot.AllowedActions, localization) {
        Snapshot = snapshot;
        Opening.SetRowDiagnostics(snapshot.Diagnostics);
        Lintel.SetRowDiagnostics(snapshot.Diagnostics);
        _selectedAction = Actions.FirstOrDefault(value => value.Action == snapshot.DefaultAction)
            ?? throw new ArgumentException("Действие по умолчанию должно входить в допустимые действия.",
                nameof(snapshot));
    }

    public ManagerItemSnapshot Snapshot { get; }
    public bool IsLastInGroup {
        get => _isLastInGroup;
        set => RaiseAndSetIfChanged(ref _isLastInGroup, value);
    }
    public ManagerAction CurrentAction => _selectedAction.Action;
    public bool IsProcessingSelected => _isChecked && CurrentAction != ManagerAction.Skip;
    public override bool? IsChecked {
        get => _isChecked;
        set {
            if(!value.HasValue || value.Value == _isChecked) {
                return;
            }
            bool previous = _isChecked;
            RaiseAndSetIfChanged(ref _isChecked, value.Value);
            if(CurrentAction != ManagerAction.Skip) {
                RaisePropertyChanged(nameof(IsProcessingSelected));
            }
            StateChanged?.Invoke(this, previous, CurrentAction);
        }
    }
    public override ManagerActionViewModel SelectedAction {
        get => _selectedAction;
        set {
            var action = Actions.FirstOrDefault(option => option.Action == value?.Action);
            if(action is null) {
                return;
            }
            _manualAction = action.Action;
            if(action == _selectedAction) {
                return;
            }
            var previous = CurrentAction;
            RaiseAndSetIfChanged(ref _selectedAction, action);
            RaisePropertyChanged(nameof(CurrentAction));
            if(_isChecked && (previous == ManagerAction.Skip || CurrentAction == ManagerAction.Skip)) {
                RaisePropertyChanged(nameof(IsProcessingSelected));
            }
            StateChanged?.Invoke(this, _isChecked, previous);
        }
    }

    /// <summary>Переносит выбор и допустимое ручное действие до подключения строки к группе.</summary>
    public void RestoreState(ManagerItemViewModel previous) {
        if(!Snapshot.Key.Equals(previous.Snapshot.Key)) {
            throw new ArgumentException("Состояние относится к другой строке.", nameof(previous));
        }
        _isChecked = previous._isChecked;
        var action = Actions.FirstOrDefault(option => option.Action == previous._manualAction);
        if(action is not null) {
            _manualAction = action.Action;
            _selectedAction = action;
        } else {
            _manualAction = null;
            _selectedAction = Actions.First(option => option.Action == Snapshot.DefaultAction);
        }
    }
}
