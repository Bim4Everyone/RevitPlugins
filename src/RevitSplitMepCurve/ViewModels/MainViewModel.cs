using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitSplitMepCurve.Models;
using RevitSplitMepCurve.Models.Enums;
using RevitSplitMepCurve.Models.Errors;
using RevitSplitMepCurve.Models.Exceptions;
using RevitSplitMepCurve.Models.Splittable;
using RevitSplitMepCurve.Services.Core;
using RevitSplitMepCurve.Services.Providers;
using RevitSplitMepCurve.ViewModels.Providers;

using Autodesk.Revit.DB;

namespace RevitSplitMepCurve.ViewModels;

internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly IErrorsService _errorsService;
    private readonly ILocalizationService _localization;
    private readonly ErrorsWindowService _errorsWindowService;
    private readonly IMessageBoxService _messageBoxService;
    private readonly ICollection<IElementsProvider> _providers;

    private SelectionModeViewModel _selectionMode;
    private ElementsProviderViewModel _elementsProvider;
    private bool _showPlacingErrors;
    private bool _selectAllLevels;
    private string _errorText;

    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        IErrorsService errorsService,
        ILocalizationService localization,
        ErrorsWindowService errorsWindowService,
        IMessageBoxService messageBoxService,
        ICollection<IElementsProvider> providers) {
        _pluginConfig = pluginConfig ?? throw new ArgumentNullException(nameof(pluginConfig));
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _errorsService = errorsService ?? throw new ArgumentNullException(nameof(errorsService));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _errorsWindowService = errorsWindowService ?? throw new ArgumentNullException(nameof(errorsWindowService));
        _messageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
        _providers = providers ?? throw new ArgumentNullException(nameof(providers));

        Levels = [];
        AvailableSelectionModes = [];
        AvailableElementsProviders = [];

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand LoadViewCommand { get; }

    public ICommand AcceptViewCommand { get; }

    public IMessageBoxService MessageBoxService => _messageBoxService;

    public SelectionModeViewModel SelectionMode {
        get => _selectionMode;
        set => RaiseAndSetIfChanged(ref _selectionMode, value);
    }

    public ObservableCollection<SelectionModeViewModel> AvailableSelectionModes { get; }

    public ElementsProviderViewModel ElementsProvider {
        get => _elementsProvider;
        set => RaiseAndSetIfChanged(ref _elementsProvider, value);
    }

    public ObservableCollection<ElementsProviderViewModel> AvailableElementsProviders { get; }

    public ObservableCollection<LevelViewModel> Levels { get; }

    public bool ShowPlacingErrors {
        get => _showPlacingErrors;
        set => RaiseAndSetIfChanged(ref _showPlacingErrors, value);
    }

    public bool SelectAllLevels {
        get => _selectAllLevels;
        set {
            if(_selectAllLevels == value) {
                return;
            }
            RaiseAndSetIfChanged(ref _selectAllLevels, value);
            foreach(var level in Levels) {
                level.IsSelected = value;
            }
        }
    }

    public string ErrorText {
        get => _errorText;
        private set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    private void LoadView() {
        var config = _pluginConfig.GetSettings(_revitRepository.Document);

        double basePointZ = _revitRepository.GetProjectBasePoint().Position.Z;

        var levels = _revitRepository.GetLevels([]).OrderByDescending(l => l.Elevation);
        foreach(var level in levels) {
            bool isSelected = config is null
                || !config.UncheckedLevelNames.Contains(level.Name);
            var vm = new LevelViewModel(level, isSelected, basePointZ, _localization);
            vm.PropertyChanged += OnLevelPropertyChanged;
            Levels.Add(vm);
        }

        bool hasSelectedElements = _revitRepository.HasSelectedElements();
        AvailableSelectionModes.Clear();
        foreach(SelectionMode mode in (SelectionMode[])Enum.GetValues(typeof(SelectionMode))) {
            var modeVm = new SelectionModeViewModel(_localization, mode);
            if(mode == RevitSplitMepCurve.Models.Enums.SelectionMode.SelectedElements && !hasSelectedElements) {
                modeVm.IsEnabled = false;
            }
            AvailableSelectionModes.Add(modeVm);
        }

        var providerVms = InitializeElementsProviders(_providers);
        AvailableElementsProviders.Clear();
        foreach(var vm in providerVms) {
            AvailableElementsProviders.Add(vm);
        }

        SelectionMode = AvailableSelectionModes
            .FirstOrDefault(m => config != null && m.Mode == config.SelectedMode && m.IsEnabled)
            ?? AvailableSelectionModes.FirstOrDefault(m => m.IsEnabled);

        ElementsProvider = AvailableElementsProviders
            .FirstOrDefault(p => config != null && p.MepClass == config.SelectedMepClass)
            ?? AvailableElementsProviders.FirstOrDefault();

        if(config is not null) {
            RestoreSymbolSelections(config);
        }

        ShowPlacingErrors = config?.ShowSplitErrors ?? true;
        _selectAllLevels = Levels.All(l => l.IsSelected);
        OnPropertyChanged(nameof(SelectAllLevels));
    }

    private void RestoreSymbolSelections(RevitSettings config) {
        foreach(var vm in AvailableElementsProviders) {
            if(vm is PipesProviderViewModel pipes) {
                pipes.RoundSymbol.SelectedItem =
                    pipes.RoundSymbol.AvailableItems
                        .FirstOrDefault(s => s.Symbol.Name == config.ConnectorRoundSymbolName)
                    ?? pipes.RoundSymbol.AvailableItems.FirstOrDefault();
            } else if(vm is DuctsProviderViewModel ducts) {
                ducts.RoundSymbol.SelectedItem =
                    ducts.RoundSymbol.AvailableItems
                        .FirstOrDefault(s => s.Symbol.Name == config.ConnectorRoundSymbolName)
                    ?? ducts.RoundSymbol.AvailableItems.FirstOrDefault();
                ducts.RectangleSymbol.SelectedItem =
                    ducts.RectangleSymbol.AvailableItems
                        .FirstOrDefault(s => s.Symbol.Name == config.ConnectorRectangleSymbolName)
                    ?? ducts.RectangleSymbol.AvailableItems.FirstOrDefault();
            }
        }
    }

    private void CheckSyncNecessity(ICollection<SplittableElement> elements) {
        if(_revitRepository.IsSyncRequired(elements)) {
            _messageBoxService.Show(
                _localization.GetLocalizedString("MainWindow.SyncRequired"),
                _localization.GetLocalizedString("MainWindow.Title"),
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            throw new OperationCanceledException();
        }
    }

    private ICollection<SplittableElement> GetSplittableElements(ICollection<Level> selectedLevels) {
        return ElementsProvider.Provider.GetElements(SelectionMode.Mode)
            .Where(e => e.CanBeSplitted(selectedLevels))
            .ToArray();
    }

    private ICollection<Level> GetSelectedLevels() {
        return Levels
            .Where(l => l.IsSelected)
            .Select(l => l.Level)
            .ToArray();
    }

    private void AcceptView() {
        SaveConfig();
        var selectedLevels = GetSelectedLevels();
        var settings = ElementsProvider.GetSplitSettings(selectedLevels);
        var splittable = GetSplittableElements(selectedLevels);

        CheckSyncNecessity(splittable);

        if(splittable.Count == 0) {
            return;
        }

        using(var tGroup = _revitRepository.Document.StartTransactionGroup(
                  _localization.GetLocalizedString("MainWindow.TransactionName"))) {
            foreach(var item in splittable) {
                using var t = _revitRepository.Document.StartTransaction("item");
                try {
                    var result = item.Split(settings);
                    result.UpdateSegments();
                    t.Commit();
                } catch(CannotGetConnectorSymbolException) {
                    _errorsService.AddError(item.Element, "Error.InsufficientSpace");
                    t.RollBack();
                } catch(CannotCreateConnectorException) {
                    _errorsService.AddError(item.Element, "Error.CannotCreateConnector");
                    t.RollBack();
                }
            }

            tGroup.Assimilate();
        }
        ShowErrors();
    }

    private bool CanAcceptView() {
        if(ElementsProvider is null) {
            ErrorText = _localization.GetLocalizedString("MainWindow.Validation.NoProvider");
            return false;
        }
        if(SelectionMode is null) {
            ErrorText = _localization.GetLocalizedString("MainWindow.Validation.NoSelectionMode");
            return false;
        }

        string providerError = ElementsProvider.GetErrorText();
        if(!string.IsNullOrWhiteSpace(providerError)) {
            ErrorText = providerError;
            return false;
        }
        if(!Levels.Any(l => l.IsSelected)) {
            ErrorText = _localization.GetLocalizedString("MainWindow.Validation.NoLevels");
            return false;
        }
        ErrorText = null;
        return true;
    }

    private void ShowErrors() {
        if(ShowPlacingErrors && _errorsService.ContainsErrors()) {
            _errorsWindowService.ShowErrorsWindow();
        }
    }

    private void SaveConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
                      ?? _pluginConfig.AddSettings(_revitRepository.Document);

        setting.SelectedMepClass = ElementsProvider?.MepClass ?? MepClass.Pipes;
        setting.SelectedMode = _selectionMode?.Mode ?? RevitSplitMepCurve.Models.Enums.SelectionMode.ActiveView;
        setting.UncheckedLevelNames = Levels
            .Where(l => !l.IsSelected)
            .Select(l => l.Name)
            .ToList();
        setting.ShowSplitErrors = ShowPlacingErrors;

        if(ElementsProvider is PipesProviderViewModel pipes) {
            setting.ConnectorRoundSymbolName = pipes.RoundSymbol.SelectedItem?.Symbol.Name;
        } else if(ElementsProvider is DuctsProviderViewModel ducts) {
            setting.ConnectorRoundSymbolName = ducts.RoundSymbol.SelectedItem?.Symbol.Name;
            setting.ConnectorRectangleSymbolName = ducts.RectangleSymbol.SelectedItem?.Symbol.Name;
        }

        _pluginConfig.SaveProjectConfig();
    }

    private ObservableCollection<ElementsProviderViewModel> InitializeElementsProviders(
        ICollection<IElementsProvider> providers) {
        var vms = new ObservableCollection<ElementsProviderViewModel>();
        foreach(var p in providers) {
            ElementsProviderViewModel vm = p switch {
                PipesProvider pp => new PipesProviderViewModel(_localization, pp, _revitRepository),
                DuctsProvider dp => new DuctsProviderViewModel(_localization, dp, _revitRepository),
                _ => throw new InvalidOperationException()
            };
            vms.Add(vm);
        }
        return vms;
    }

    private void OnLevelPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName != nameof(LevelViewModel.IsSelected)) {
            return;
        }
        bool allSelected = Levels.All(l => l.IsSelected);
        if(_selectAllLevels != allSelected) {
            _selectAllLevels = allSelected;
            OnPropertyChanged(nameof(SelectAllLevels));
        }
    }
}
