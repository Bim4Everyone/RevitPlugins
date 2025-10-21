using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using DevExpress.CodeParser;

using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitMarkPlacement.Models;
using RevitMarkPlacement.Models.SelectionModes;
using RevitMarkPlacement.ViewModels.FloorHeight;

namespace RevitMarkPlacement.ViewModels;

internal class MainViewModel : BaseViewModel {
    private const string _allElementsSelection = "Создать по всему проекту";
    private const string _selectedElements = "Создать по выбранным элементам";
    private readonly AnnotationsConfig _config;
    private readonly IDocumentProvider _documentProvider;
    private readonly RevitRepository _revitRepository;
    private readonly AnnotationsSettings _settings;
    private string _errorText;
    private string _floorCount;
    private List<IFloorHeightProvider> _floorHeights;
    private InfoElementsViewModel _infoElementsViewModel;
    private IFloorHeightProvider _floorHeight;
    private SelectionModeViewModel _selectedMode;
    private string _selectedParameterName;
    private List<SelectionModeViewModel> _selectionModes;

    public MainViewModel(
        RevitRepository revitRepository,
        AnnotationsConfig config,
        IDocumentProvider documentProvider) {
        _revitRepository = revitRepository;
        _config = config;
        _documentProvider = documentProvider;
        _settings = _revitRepository.GetSettings(_config);
        if(_settings == null) {
            _settings = _revitRepository.AddSettings(_config);
        }

        FloorCount = _settings.LevelCount.ToString();
        InitializeSelectionModes();
        InitializeFloorHeightProvider();
        InfoElementsViewModel = new InfoElementsViewModel();
        PlaceAnnotationCommand = new RelayCommand(PlaceAnnotation, CanPlaceAnnotation);
    }

    public string FloorCount {
        get => _floorCount;
        set => RaiseAndSetIfChanged(ref _floorCount, value);
    }

    public string SelectedParameterName {
        get => _selectedParameterName;
        set => RaiseAndSetIfChanged(ref _selectedParameterName, value);
    }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public SelectionModeViewModel SelectedMode {
        get => _selectedMode;
        set => RaiseAndSetIfChanged(ref _selectedMode, value);
    }

    public IFloorHeightProvider FloorHeight {
        get => _floorHeight;
        set => RaiseAndSetIfChanged(ref _floorHeight, value);
    }
    
    public List<IFloorHeightProvider> FloorHeights {
        get => _floorHeights;
        set => RaiseAndSetIfChanged(ref _floorHeights, value);
    }

    public InfoElementsViewModel InfoElementsViewModel {
        get => _infoElementsViewModel;
        set => RaiseAndSetIfChanged(ref _infoElementsViewModel, value);
    }

    public List<SelectionModeViewModel> SelectionModes {
        get => _selectionModes;
        set => RaiseAndSetIfChanged(ref _selectionModes, value);
    }

    public ICommand PlaceAnnotationCommand { get; set; }

    private void InitializeSelectionModes() {
        SelectionModes = new List<SelectionModeViewModel> {
            new(_revitRepository, new DBSelection(_documentProvider), _allElementsSelection),
            new(_revitRepository, new SelectedOnViewSelection(_documentProvider), _selectedElements)
        };
        if(_settings.SelectionMode == SelectionMode.AllElements) {
            SelectedMode = SelectionModes[0];
        } else {
            SelectedMode = SelectionModes[1];
        }
    }

    private void InitializeFloorHeightProvider() {
        FloorHeights = new List<IFloorHeightProvider> {
            new UserFloorHeightViewModel(),
            new GlobalParamsViewModel(new DoubleGlobalParamSelection(_documentProvider))
        };

        foreach(var provider in FloorHeights) {
            provider.LoadConfig(_settings);
        }

        FloorHeight = FloorHeights
                          .FirstOrDefault(item =>
                              item.IsEnabled
                              && item.LevelHeightProvider == _settings.LevelHeightProvider)
                      ?? FloorHeights.FirstOrDefault();
    }

    private void PlaceAnnotation(object p) {
        var marks = new TemplateLevelMarkCollection(_revitRepository, SelectedMode.Selection);
        marks.CreateAnnotation(int.Parse(FloorCount), FloorHeight.GetFloorHeight() ?? 0);
        SaveConfig();
    }

    private bool CanPlaceAnnotation(object p) {
        if(!int.TryParse(FloorCount, out int levelCount)) {
            ErrorText = "Количество типовых этажей должно быть числом.";
            return false;
        }

        if(levelCount < 1) {
            ErrorText = "Количество типовых этажей должно быть неотрицательным.";
            return false;
        }

        if(FloorHeight.GetFloorHeight() < 1) {
            ErrorText = "Высота типового этажа должна быть неотрицательной.";
            return false;
        }

        ErrorText = null;
        return true;
    }

    private void SaveConfig() {
        if(SelectedMode.Description == _allElementsSelection) {
            _settings.SelectionMode = SelectionMode.AllElements;
        } else {
            _settings.SelectionMode = SelectionMode.SelectedElements;
        }

        _settings.LevelCount = int.Parse(FloorCount);
        _settings.LevelHeightProvider = FloorHeight.LevelHeightProvider;

        foreach(var provider in FloorHeights) {
            provider.SaveConfig(_settings);
        }

        _config.SaveProjectConfig();
    }

    public bool CanPlaceAnnotation() {
        var families = CheckFamilyAnnotations();
        CheckAnnotationParameters(families);
        CheckElevationSymbols();
        return InfoElementsViewModel.InfoElements.Count == 0;
    }

    private IEnumerable<Family> CheckFamilyAnnotations() {
        var topFamily = _revitRepository.GetTopAnnotaionFamily();
        if(topFamily != null) {
            yield return topFamily;
        } else {
            InfoElementsViewModel.InfoElements.Add(
                new InfoElementViewModel(InfoElement.FamilyAnnotationMissing, RevitRepository.FamilyTop));
        }

        var bottomFamily = _revitRepository.GetBottomAnnotaionFamily();
        if(bottomFamily != null) {
            yield return bottomFamily;
        } else {
            InfoElementsViewModel.InfoElements.Add(
                new InfoElementViewModel(InfoElement.FamilyAnnotationMissing, RevitRepository.FamilyBottom));
        }
    }

    private bool CheckAnnotationParameters(IEnumerable<Family> families) {
        var parameters = new List<string> {
            RevitRepository.LevelCountParam,
            RevitRepository.FirstLevelOnParam,
            RevitRepository.SpotDimensionIdParam,
            RevitRepository.TemplateLevelHeightParam,
            RevitRepository.FirstLevelParam
        };
        bool result = true;
        foreach(var family in families) {
            var document = _revitRepository.GetFamilyDocument(family);
            var familyManager = document.FamilyManager;
            try {
                var notExistedParams = parameters
                    .Except(familyManager.GetParameters().Select(item => item.Definition.Name))
                    .ToList();

                if(notExistedParams.Count > 0) {
                    foreach(string param in notExistedParams) {
                        InfoElementsViewModel.InfoElements.Add(
                            new InfoElementViewModel(InfoElement.AnnotationParameterMissing, family.Name, param));
                    }

                    result = false;
                }
            } finally {
                document.Close(false);
            }
        }

        return result;
    }

    private bool CheckElevationSymbols() {
        var symbols = _revitRepository.GetElevationSymbols();
        bool result = true;
        foreach(var symbol in symbols) {
            if(!symbol.IsExistsParam(RevitRepository.ElevSymbolWidth)) {
                InfoElementsViewModel.InfoElements.Add(
                    new InfoElementViewModel(
                        InfoElement.ElevationParameterMissing,
                        symbol.Name,
                        RevitRepository.ElevSymbolWidth));
                result = false;
            }

            if(!symbol.IsExistsParam(RevitRepository.ElevSymbolHeight)) {
                InfoElementsViewModel.InfoElements.Add(
                    new InfoElementViewModel(
                        InfoElement.ElevationParameterMissing,
                        symbol.Name,
                        RevitRepository.ElevSymbolHeight));
                result = false;
            }
        }

        return result;
    }
}
