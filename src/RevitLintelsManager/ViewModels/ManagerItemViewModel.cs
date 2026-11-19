using System;
using System.Linq;

using dosymep.SimpleServices;

using RevitLintelsManager.Models;

namespace RevitLintelsManager.ViewModels;

/// <summary>Индивидуальная строка на независимом снимке без операций модели.</summary>
internal sealed class ManagerItemViewModel : ManagerRowViewModel {
    public event Action<ManagerItemViewModel, bool, ManagerAction> StateChanged;
    private bool _isChecked = true;
    private ManagerActionViewModel _selectedAction;

    /// <summary>Создаёт отмеченную строку с допустимым действием по умолчанию.</summary>
    public ManagerItemViewModel(ManagerItemSnapshot snapshot, ManagerElementPresentation opening,
        ManagerElementPresentation lintel, ILocalizationService localization)
        : base(opening, lintel, snapshot.AllowedActions, localization) {
        Snapshot = snapshot;
        _selectedAction = Actions.FirstOrDefault(value => value.Action == snapshot.DefaultAction)
            ?? throw new ArgumentException("Действие по умолчанию должно входить в допустимые действия.",
                nameof(snapshot));
    }

    public ManagerItemSnapshot Snapshot { get; }
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
            RaisePropertyChanged(nameof(IsProcessingSelected));
            StateChanged?.Invoke(this, previous, CurrentAction);
        }
    }
    public override ManagerActionViewModel SelectedAction {
        get => _selectedAction;
        set {
            var action = Actions.FirstOrDefault(option => option.Action == value?.Action);
            if(action is null || action == _selectedAction) {
                return;
            }
            var previous = CurrentAction;
            RaiseAndSetIfChanged(ref _selectedAction, action);
            RaisePropertyChanged(nameof(CurrentAction));
            RaisePropertyChanged(nameof(IsProcessingSelected));
            StateChanged?.Invoke(this, _isChecked, previous);
        }
    }
}
