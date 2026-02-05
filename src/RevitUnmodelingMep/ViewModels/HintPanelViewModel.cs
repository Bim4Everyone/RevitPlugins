using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitUnmodelingMep.ViewModels;

internal sealed class HintPanelViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;
    private readonly Func<ConsumableTypeItem, int?> _resolveCategoryId;

    private bool _isFormulaEditing;
    private bool _isNameEditing;
    private bool _isNoteEditing;
    private ConsumableTypeItem _activeItem;
    private ObservableCollection<string> _activeHintItems = new ObservableCollection<string>();

    public HintPanelViewModel(ILocalizationService localizationService, Func<ConsumableTypeItem, int?> resolveCategoryId) {
        _localizationService = localizationService;
        _resolveCategoryId = resolveCategoryId;

        BeginFormulaEditCommand = RelayCommand.Create<ConsumableTypeItem>(BeginFormulaEdit);
        EndFormulaEditCommand = RelayCommand.Create<ConsumableTypeItem>(EndFormulaEdit);
        BeginNameEditCommand = RelayCommand.Create<ConsumableTypeItem>(BeginNameEdit);
        EndNameEditCommand = RelayCommand.Create<ConsumableTypeItem>(EndNameEdit);
        BeginNoteEditCommand = RelayCommand.Create<ConsumableTypeItem>(BeginNoteEdit);
        EndNoteEditCommand = RelayCommand.Create<ConsumableTypeItem>(EndNoteEdit);
    }

    public ICommand BeginFormulaEditCommand { get; }
    public ICommand EndFormulaEditCommand { get; }
    public ICommand BeginNameEditCommand { get; }
    public ICommand EndNameEditCommand { get; }
    public ICommand BeginNoteEditCommand { get; }
    public ICommand EndNoteEditCommand { get; }

    public bool IsFormulaEditing {
        get => _isFormulaEditing;
        private set {
            RaiseAndSetIfChanged(ref _isFormulaEditing, value);
            RaisePropertyChanged(nameof(IsHintVisible));
            RaisePropertyChanged(nameof(HintTitle));
            RaisePropertyChanged(nameof(IsHintInfoVisible));
            RaisePropertyChanged(nameof(HintInfoText));
        }
    }

    public bool IsNameEditing {
        get => _isNameEditing;
        private set {
            RaiseAndSetIfChanged(ref _isNameEditing, value);
            RaisePropertyChanged(nameof(IsHintVisible));
            RaisePropertyChanged(nameof(HintTitle));
            RaisePropertyChanged(nameof(IsHintInfoVisible));
            RaisePropertyChanged(nameof(HintInfoText));
        }
    }

    public bool IsNoteEditing {
        get => _isNoteEditing;
        private set {
            RaiseAndSetIfChanged(ref _isNoteEditing, value);
            RaisePropertyChanged(nameof(IsHintVisible));
            RaisePropertyChanged(nameof(HintTitle));
            RaisePropertyChanged(nameof(IsHintInfoVisible));
            RaisePropertyChanged(nameof(HintInfoText));
        }
    }

    public bool IsHintVisible => IsFormulaEditing || IsNameEditing || IsNoteEditing;

    public string HintTitle => _localizationService.GetLocalizedString("MainWindow.FormulaVariables");

    public bool IsHintInfoVisible => IsNameEditing || IsNoteEditing;

    public string HintInfoText => IsHintInfoVisible
        ? _localizationService.GetLocalizedString("MainWindow.PlaceholderBracesHint")
        : string.Empty;

    public ObservableCollection<string> ActiveHintItems {
        get => _activeHintItems;
        private set => RaiseAndSetIfChanged(ref _activeHintItems, value);
    }

    private void BeginFormulaEdit(ConsumableTypeItem item) {
        IsFormulaEditing = item != null;
        IsNameEditing = false;
        IsNoteEditing = false;
        SetActiveItem(item);
    }

    private void EndFormulaEdit(ConsumableTypeItem item) {
        if(_activeItem != item) {
            return;
        }

        IsFormulaEditing = false;
        if(!IsNameEditing && !IsNoteEditing) {
            SetActiveItem(null);
        }
    }

    private void BeginNameEdit(ConsumableTypeItem item) {
        IsNameEditing = item != null;
        IsFormulaEditing = false;
        IsNoteEditing = false;
        SetActiveItem(item);
    }

    private void EndNameEdit(ConsumableTypeItem item) {
        if(_activeItem != item) {
            return;
        }

        IsNameEditing = false;
        if(!IsFormulaEditing && !IsNoteEditing) {
            SetActiveItem(null);
        }
    }

    private void BeginNoteEdit(ConsumableTypeItem item) {
        IsNoteEditing = item != null;
        IsFormulaEditing = false;
        IsNameEditing = false;
        SetActiveItem(item);
    }

    private void EndNoteEdit(ConsumableTypeItem item) {
        if(_activeItem != item) {
            return;
        }

        IsNoteEditing = false;
        if(!IsFormulaEditing && !IsNameEditing) {
            SetActiveItem(null);
        }
    }

    private void SetActiveItem(ConsumableTypeItem item) {
        if(ReferenceEquals(_activeItem, item)) {
            UpdateActiveHintItems();
            return;
        }

        if(_activeItem != null) {
            _activeItem.PropertyChanged -= ActiveItemOnPropertyChanged;
        }

        _activeItem = item;

        if(_activeItem != null) {
            _activeItem.PropertyChanged += ActiveItemOnPropertyChanged;
        }

        UpdateActiveHintItems();
    }

    private void ActiveItemOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(ConsumableTypeItem.SelectedCategory)
           || e.PropertyName == nameof(ConsumableTypeItem.CategoryId)) {
            UpdateActiveHintItems();
        }
    }

    private void UpdateActiveHintItems() {
        if(IsNoteEditing) {
            ActiveHintItems = new ObservableCollection<string>(FormulaValidator.GetAllowedNoteTokens());
            return;
        }

        if(_activeItem == null) {
            ActiveHintItems = new ObservableCollection<string>();
            return;
        }

        int? categoryId = _resolveCategoryId?.Invoke(_activeItem);
        if(categoryId == null) {
            ActiveHintItems = new ObservableCollection<string>();
            return;
        }

        var props = FormulaValidator.GetAllowedPropertyNames(categoryId.Value)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToList();

        ActiveHintItems = new ObservableCollection<string>(props);
    }
}

