using System;
using System.Linq;

using dosymep.SimpleServices;

using RevitLintelsManager.Models;

namespace RevitLintelsManager.ViewModels;

/// <summary>Состояние строки вне диапазона без снимка элемента и объектов представления.</summary>
internal readonly struct ManagerItemState(ManagerGroupKey groupKey, bool isChecked,
    ManagerAction? manualAction, long? openingId, long? lintelId) {
    public ManagerGroupKey GroupKey { get; } = groupKey;
    public bool IsChecked { get; } = isChecked;
    public ManagerAction? ManualAction { get; } = manualAction;
    public long? OpeningId { get; } = openingId;
    public long? LintelId { get; } = lintelId;
}

/// <summary>Индивидуальная строка на независимом снимке без операций модели.</summary>
internal sealed class ManagerItemViewModel : ManagerRowViewModel {
    public event Action<ManagerItemViewModel, bool, ManagerAction> StateChanged;
    private ManagerElementStatus _groupStatus = ManagerElementStatus.NotAnalyzed;
    private bool _isChecked = true;
    private bool _isLastInGroup;
    private ManagerAction? _manualAction;
    private ManagerActionViewModel _selectedAction;

    /// <summary>Создаёт отмеченную строку с допустимым действием по умолчанию.</summary>
    public ManagerItemViewModel(ManagerItemSnapshot snapshot, ManagerElementPresentation opening,
        ManagerElementPresentation lintel, ILocalizationService localization)
        : base(opening, lintel, snapshot.AllowedActions, localization, snapshot.GroupKey,
            correctionTitleKey: GetCorrectionTitleKey(snapshot.Corrections)) {
        Snapshot = snapshot;
        Opening.SetRowDiagnostics(snapshot.Diagnostics);
        Lintel.SetRowDiagnostics(snapshot.Diagnostics);
        _selectedAction = Actions.FirstOrDefault(value => value.Action == snapshot.DefaultAction)
            ?? throw new ArgumentException("Действие по умолчанию должно входить в допустимые действия.",
                nameof(snapshot));
    }

    public ManagerItemSnapshot Snapshot { get; }
    public ManagerElementStatus ResultStatus => Opening.IsAnalysisPending ? ManagerElementStatus.NotAnalyzed
        : Snapshot.GroupKey.IsFixed ? ManagerElementStatus.Fixed
        : Opening.HasWarnings || Lintel.HasWarnings ? ManagerElementStatus.Warning : ManagerElementStatus.Correct;
    public ManagerElementStatus GroupStatus {
        get => _groupStatus;
        internal set => RaiseAndSetIfChanged(ref _groupStatus, value);
    }
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

    /// <summary>Фиксирует введённое пользователем состояние для ключа текущей строки.</summary>
    public ManagerItemState CaptureState() {
        return new ManagerItemState(Snapshot.GroupKey, _isChecked, _manualAction,
            Opening.Element?.Information?.ElementId, Lintel.Element?.Information?.ElementId);
    }

    /// <summary>Восстанавливает чекбокс и допустимое ручное действие по совпавшему ключу.</summary>
    public void RestoreState(ManagerItemState previous) {
        _isChecked = previous.IsChecked;
        var action = Actions.FirstOrDefault(option => option.Action == previous.ManualAction);
        if(action is not null) {
            _manualAction = action.Action;
            _selectedAction = action;
        } else {
            _manualAction = null;
            _selectedAction = Actions.First(option => option.Action == Snapshot.DefaultAction);
        }
    }

    // Уточняет подпись по составу исправления, сохраняя общее действие для смешанных работ.
    private static string GetCorrectionTitleKey(ManagerCorrection corrections) {
        if((corrections & ManagerCorrection.Dimensions) != ManagerCorrection.None
            && corrections is not (ManagerCorrection.Width or ManagerCorrection.Thickness or ManagerCorrection.Bearing)) {
            corrections |= ManagerCorrection.Dimensions;
        }
        return corrections switch {
            ManagerCorrection.Width => "Manager.Action.Correct.Width",
            ManagerCorrection.Thickness => "Manager.Action.Correct.Thickness",
            ManagerCorrection.Bearing => "Manager.Action.Correct.Bearing",
            ManagerCorrection.Attachments => "Manager.Action.Correct.Attachments",
            ManagerCorrection.Position => "Manager.Action.Correct.Position",
            ManagerCorrection.Dimensions => "Manager.Action.Correct.Dimensions",
            ManagerCorrection.Position | ManagerCorrection.Dimensions =>
                "Manager.Action.Correct.PositionAndDimensions",
            ManagerCorrection.Position | ManagerCorrection.Attachments =>
                "Manager.Action.Correct.PositionAndAttachments",
            ManagerCorrection.Dimensions | ManagerCorrection.Attachments =>
                "Manager.Action.Correct.DimensionsAndAttachments",
            _ => "Manager.Action.Correct"
        };
    }
}
