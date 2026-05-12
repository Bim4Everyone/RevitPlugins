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
    public IProgressDialogFactory ProgressFactory { get; }
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
        IProgressDialogFactory progressFactory,
        ICollection<IElementsProvider> providers) {
        ProgressFactory = progressFactory ?? throw new ArgumentNullException(nameof(progressFactory));
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
        LoadConfig();
    }

    private PipesProviderViewModel CreatePipesProvider(PipesProvider provider, RevitSettings config) {
        var providerVm = new PipesProviderViewModel(_localization, provider, _revitRepository);
        providerVm.RoundSymbol.SelectedItem =
            providerVm.RoundSymbol.AvailableItems.FirstOrDefault(s =>
                config?.RoundConnector?.Equals(s.Symbol) ?? false);
        return providerVm;
    }

    private DuctsProviderViewModel CreateDuctsProvider(DuctsProvider provider, RevitSettings config) {
        var providerVm = new DuctsProviderViewModel(_localization, provider, _revitRepository);
        providerVm.RoundSymbol.SelectedItem =
            providerVm.RoundSymbol.AvailableItems
                .FirstOrDefault(s => config?.RoundConnector?.Equals(s.Symbol) ?? false);
        providerVm.RectangleSymbol.SelectedItem =
            providerVm.RectangleSymbol.AvailableItems.FirstOrDefault(s =>
                config?.RectangleConnector?.Equals(s.Symbol) ?? false);
        return providerVm;
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

        using(var dialogService = ProgressFactory.CreateDialog()) {
            dialogService.MaxValue = splittable.Count;
            var progress = dialogService.CreateProgress();
            var ct = dialogService.CreateCancellationToken();
            int i = 0;
            dialogService.Show();
            using(var t = _revitRepository.Document.StartTransaction(
                      _localization.GetLocalizedString("MainWindow.TransactionName"))) {
                foreach(var item in splittable) {
                    ct.ThrowIfCancellationRequested();
                    progress.Report(++i);
                    using(var subT = new SubTransaction(_revitRepository.Document)) {
                        subT.Start();
                        try {
                            var result = item.Split(settings);
                            result.UpdateSegments();
                            subT.Commit();
                        } catch(CannotGetConnectorSymbolException) {
                            _errorsService.AddError(item.Element, "Error.InsufficientSpace");
                            subT.RollBack();
                        } catch(CannotCreateConnectorException) {
                            _errorsService.AddError(item.Element, "Error.CannotCreateConnector");
                            subT.RollBack();
                        }
                    }
                }

                t.Commit();
            }
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

        setting.SelectedMepClass = ElementsProvider?.MepClass ?? RevitSettings.DefaultMepClass;
        setting.SelectedMode = _selectionMode?.Mode ?? RevitSettings.DefaultSelectionMode;
        setting.UncheckedLevelNames = Levels
            .Where(l => !l.IsSelected)
            .Select(l => l.Name)
            .ToList();
        setting.ShowSplitErrors = ShowPlacingErrors;

        if(ElementsProvider is PipesProviderViewModel pipes) {
            setting.RoundConnector = new ConnectorConfig() {
                SymbolName = pipes.RoundSymbol.SelectedItem?.Symbol.Name,
                FamilyName = pipes.RoundSymbol.SelectedItem?.Symbol.FamilyName
            };
            setting.RectangleConnector = null;
        } else if(ElementsProvider is DuctsProviderViewModel ducts) {
            setting.RoundConnector = new ConnectorConfig() {
                SymbolName = ducts.RoundSymbol.SelectedItem?.Symbol.Name,
                FamilyName = ducts.RoundSymbol.SelectedItem?.Symbol.FamilyName
            };
            setting.RectangleConnector = new ConnectorConfig() {
                SymbolName = ducts.RectangleSymbol.SelectedItem?.Symbol.Name,
                FamilyName = ducts.RectangleSymbol.SelectedItem?.Symbol.FamilyName
            };
        }

        _pluginConfig.SaveProjectConfig();
    }

    private void InitializeElementsProviders(RevitSettings config) {
        foreach(var p in _providers) {
            ElementsProviderViewModel vm = p switch {
                PipesProvider pp => CreatePipesProvider(pp, config),
                DuctsProvider dp => CreateDuctsProvider(dp, config),
                _ => throw new InvalidOperationException()
            };
            AvailableElementsProviders.Add(vm);
        }

        ElementsProvider = AvailableElementsProviders
                               .FirstOrDefault(p => config != null && p.MepClass == config.SelectedMepClass)
                           ?? AvailableElementsProviders.First();
    }

    private void InitializeLevels(RevitSettings config) {
        var levels = _revitRepository.GetLevels().OrderByDescending(l => l.Elevation);
        foreach(var level in levels) {
            var vm = new LevelViewModel(level, _localization);
            vm.IsSelected = config is null
                            || !config.UncheckedLevelNames.Contains(level.Name, StringComparer.OrdinalIgnoreCase);
            vm.PropertyChanged += OnLevelPropertyChanged;
            Levels.Add(vm);
        }

        _selectAllLevels = Levels.All(l => l.IsSelected);
        OnPropertyChanged(nameof(SelectAllLevels));
    }

    private void InitializeSelectionModes(RevitSettings config) {
        bool hasSelectedElements = _revitRepository.HasSelectedElements();
        foreach(var mode in Enum.GetValues(typeof(SelectionMode)).OfType<SelectionMode>()) {
            var modeVm = new SelectionModeViewModel(_localization, mode);
            if(mode == RevitSplitMepCurve.Models.Enums.SelectionMode.SelectedElements
               && !hasSelectedElements) {
                continue;
            }

            AvailableSelectionModes.Add(modeVm);
        }

        SelectionMode = hasSelectedElements
            ? AvailableSelectionModes.First(m =>
                m.Mode == RevitSplitMepCurve.Models.Enums.SelectionMode.SelectedElements)
            : (AvailableSelectionModes.FirstOrDefault(m => config != null && m.Mode == config.SelectedMode)
               ?? AvailableSelectionModes.First());
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

    private void LoadConfig() {
        var config = _pluginConfig.GetSettings(_revitRepository.Document);

        InitializeLevels(config);
        InitializeSelectionModes(config);
        InitializeElementsProviders(config);

        ShowPlacingErrors = config?.ShowSplitErrors ?? true;
    }
}
