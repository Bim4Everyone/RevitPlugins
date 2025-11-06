using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitMarkPlacement.Models;
using RevitMarkPlacement.Models.AnnotationTemplates;
using RevitMarkPlacement.Models.SelectionModes;
using RevitMarkPlacement.Services;
using RevitMarkPlacement.Services.AnnotationServices;
using RevitMarkPlacement.ViewModels.FloorHeight;

namespace RevitMarkPlacement.ViewModels;

internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly SystemPluginConfig _systemPluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private readonly IUnitProvider _unitProvider;
    private readonly IDocumentProvider _documentProvider;
    private readonly IGlobalParamSelection _globalSelection;
    private readonly ISpotDimensionSelection[] _spotSelections;

    private InfoElementsViewModel _infoElementsViewModel;

    private string _errorText;
    private string _levelCount;

    private SelectionModeViewModel _selection;
    private ObservableCollection<SelectionModeViewModel> _selections;

    private IFloorHeightProvider _levelHeight;
    private ObservableCollection<IFloorHeightProvider> _floorHeights;

    public MainViewModel(
        PluginConfig pluginConfig,
        SystemPluginConfig systemPluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        IUnitProvider unitProvider,
        IDocumentProvider documentProvider,
        IGlobalParamSelection globalSelection,
        ISpotDimensionSelection[] spotSelections) {
        _pluginConfig = pluginConfig;
        _systemPluginConfig = systemPluginConfig;
        
        _revitRepository = revitRepository;
        _localizationService = localizationService;

        _unitProvider = unitProvider;
        _documentProvider = documentProvider;

        _globalSelection = globalSelection;
        _spotSelections = spotSelections;

        InfoElementsViewModel = new InfoElementsViewModel();

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand LoadViewCommand { get; set; }
    public ICommand AcceptViewCommand { get; set; }

    public string LevelCount {
        get => _levelCount;
        set => RaiseAndSetIfChanged(ref _levelCount, value);
    }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public SelectionModeViewModel Selection {
        get => _selection;
        set => RaiseAndSetIfChanged(ref _selection, value);
    }

    public ObservableCollection<SelectionModeViewModel> Selections {
        get => _selections;
        set => RaiseAndSetIfChanged(ref _selections, value);
    }

    public IFloorHeightProvider LevelHeight {
        get => _levelHeight;
        set => RaiseAndSetIfChanged(ref _levelHeight, value);
    }

    public ObservableCollection<IFloorHeightProvider> FloorHeights {
        get => _floorHeights;
        set => RaiseAndSetIfChanged(ref _floorHeights, value);
    }

    public InfoElementsViewModel InfoElementsViewModel {
        get => _infoElementsViewModel;
        set => RaiseAndSetIfChanged(ref _infoElementsViewModel, value);
    }

    private void LoadView() {
        var settings = _pluginConfig.GetSettings(_documentProvider.GetDocument());

        LoadSelections(settings);
        LoadFloorHeights(settings);

        LevelCount = settings?.LevelCount.ToString();
    }

    private void LoadSelections(RevitSettings settings) {
        Selections = [.._spotSelections.Select(item => new SelectionModeViewModel(item, _revitRepository))];

        foreach(var selection in Selections) {
            selection.LoadSpotDimensionTypes();
        }

        Selection = Selections
                        .FirstOrDefault(item => item.Selections == settings?.SelectionMode)
                    ?? Selections.FirstOrDefault();
    }

    private void LoadFloorHeights(RevitSettings settings) {
        FloorHeights = [
            new UserFloorHeightViewModel(_localizationService),
            new GlobalParamsViewModel(_unitProvider, _globalSelection, _localizationService)
        ];

        foreach(var provider in FloorHeights) {
            provider.LoadConfig(settings);
        }

        LevelHeight = FloorHeights
                          .FirstOrDefault(item =>
                              item.IsEnabled
                              && item.LevelHeightProvider == settings?.LevelHeightProvider)
                      ?? FloorHeights.FirstOrDefault();
    }

    private void AcceptView() {
        SaveConfig();

        using Transaction transaction = _revitRepository.StartTransaction(
            _localizationService.GetLocalizedString("MainWindow.CreateAnnotationsTransactionName"));
        
        var service = new CreateAnnotationService(_revitRepository, _systemPluginConfig);

        service.LoadAnnotations(Selection.Selection.GetElements().ToArray());
      
        service.ProcessAnnotations(
            new CreateAnnotationTemplateOptions() {
                LevelCount = int.Parse(LevelCount),
                LevelHeightMm = LevelHeight.GetFloorHeight() ?? 0
            });

        transaction.Commit();
    }

    private bool CanAcceptView() {
        if(Selection is null) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.EmptySelectionMode");
            return false;
        }

        if(Selection?.SpotDimensionTypes.Count == 0) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.EmptySpotDimensionTypes");
            return false;
        }

        if(string.IsNullOrEmpty(LevelCount)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.EmptyFloorCount");
            return false;
        }

        if(!int.TryParse(LevelCount, out int levelCount)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.TextFloorCount");
            return false;
        }

        if(levelCount < 1) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.NegativeFloorCount");
            return false;
        }

        if(LevelHeight is null) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.EmptyFloorHeight");
            return false;
        }

        string errorText = LevelHeight.GetErrorText();
        if(!string.IsNullOrEmpty(errorText)) {
            ErrorText = errorText;
            return false;
        }

        ErrorText = null;
        return true;
    }

    private void SaveConfig() {
        Document document = _documentProvider.GetDocument();
        RevitSettings setting = _pluginConfig.GetSettings(document)
                                ?? _pluginConfig.AddSettings(document);

        setting.LevelCount = int.Parse(LevelCount);
        setting.SelectionMode = Selection?.Selections;
        setting.LevelHeightProvider = LevelHeight?.LevelHeightProvider;

        foreach(var provider in FloorHeights) {
            provider.SaveConfig(setting);
        }

        _pluginConfig.SaveProjectConfig();
    }
}
