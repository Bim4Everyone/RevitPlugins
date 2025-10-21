using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    private InfoElementsViewModel _infoElementsViewModel;
    
    private string _errorText;
    private string _floorCount;
    
    private SelectionModeViewModel _selection;
    private ObservableCollection<SelectionModeViewModel> _selections;
    
    private IFloorHeightProvider _floorHeight;
    private ObservableCollection<IFloorHeightProvider> _floorHeights;

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
        LoadSelections();
        LoadFloorHeights();
        InfoElementsViewModel = new InfoElementsViewModel();
        PlaceAnnotationCommand = new RelayCommand(PlaceAnnotation, CanPlaceAnnotation);
    }

    public string FloorCount {
        get => _floorCount;
        set => RaiseAndSetIfChanged(ref _floorCount, value);
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

    public IFloorHeightProvider FloorHeight {
        get => _floorHeight;
        set => RaiseAndSetIfChanged(ref _floorHeight, value);
    }

    public ObservableCollection<IFloorHeightProvider> FloorHeights {
        get => _floorHeights;
        set => RaiseAndSetIfChanged(ref _floorHeights, value);
    }

    public InfoElementsViewModel InfoElementsViewModel {
        get => _infoElementsViewModel;
        set => RaiseAndSetIfChanged(ref _infoElementsViewModel, value);
    }

    public ICommand PlaceAnnotationCommand { get; set; }

    private void LoadSelections() {
        Selections = [
            new SelectionModeViewModel(new DBSelection(_documentProvider), _revitRepository),
            new SelectionModeViewModel(new SelectedOnViewSelection(_documentProvider), _revitRepository)
        ];
        
        foreach(var selection in Selections) {
            selection.LoadSpotDimensionTypes();
        }

        Selection = Selections
                        .FirstOrDefault(item => item.Selections == _settings.SelectionMode)
                    ?? Selections.FirstOrDefault();
    }

    private void LoadFloorHeights() {
        FloorHeights = [
            new UserFloorHeightViewModel(), 
            new GlobalParamsViewModel(new DoubleGlobalParamSelection(_documentProvider))
        ];

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
        var marks = new TemplateLevelMarkCollection(_revitRepository, Selection.Selection);
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
        _settings.LevelCount = int.Parse(FloorCount);
        _settings.SelectionMode = Selection?.Selections;
        _settings.LevelHeightProvider = FloorHeight?.LevelHeightProvider;

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
