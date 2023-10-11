using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitMarkPlacement.Models;

namespace RevitMarkPlacement.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private const string _allElementsSelection = "Создать по всему проекту";
        private const string _selectedElements = "Создать по выбранным элементам";
        private readonly RevitRepository _revitRepository;
        private readonly AnnotationsConfig _config;
        private string _floorCount;
        private string _selectedParameterName;
        private string _errorText;
        private IFloorHeightProvider _selectedFloorHeightProvider;
        private SelectionModeViewModel _selectedMode;
        private AnnotationsSettings _settings;
        private List<SelectionModeViewModel> _selectionModes;
        private List<IFloorHeightProvider> _floorHeightProviders;
        private InfoElementsViewModel _infoElementsViewModel;

        public MainViewModel(RevitRepository revitRepository, AnnotationsConfig config) {
            _revitRepository = revitRepository;
            _config = config;
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
            set => this.RaiseAndSetIfChanged(ref _floorCount, value);
        }

        public string SelectedParameterName {
            get => _selectedParameterName;
            set => this.RaiseAndSetIfChanged(ref _selectedParameterName, value);
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public SelectionModeViewModel SelectedMode {
            get => _selectedMode;
            set => this.RaiseAndSetIfChanged(ref _selectedMode, value);
        }

        public List<IFloorHeightProvider> FloorHeightProviders {
            get => _floorHeightProviders;
            set => this.RaiseAndSetIfChanged(ref _floorHeightProviders, value);
        }

        public IFloorHeightProvider SelectedFloorHeightProvider {
            get => _selectedFloorHeightProvider;
            set => this.RaiseAndSetIfChanged(ref _selectedFloorHeightProvider, value);
        }

        public InfoElementsViewModel InfoElementsViewModel { 
            get => _infoElementsViewModel; 
            set => this.RaiseAndSetIfChanged(ref _infoElementsViewModel, value); 
        }

        public List<SelectionModeViewModel> SelectionModes {
            get => _selectionModes;
            set => this.RaiseAndSetIfChanged(ref _selectionModes, value);
        }

        public ICommand PlaceAnnotationCommand { get; set; }

        private void InitializeSelectionModes() {
            SelectionModes = new List<SelectionModeViewModel>() {
                new SelectionModeViewModel(_revitRepository, new AllElementsSelection(), _allElementsSelection),
                new SelectionModeViewModel(_revitRepository, new ElementsSelection(), _selectedElements)
            };
            if(_settings.SelectionMode == SelectionMode.AllElements) {
                SelectedMode = SelectionModes[0];
            } else {
                SelectedMode = SelectionModes[1];
            }
        }

        private void InitializeFloorHeightProvider() {
            FloorHeightProviders = new List<IFloorHeightProvider>() {
                new UserFloorHeightViewModel("Индивидуальная настройка", _settings),
                new GlobalFloorHeightViewModel(_revitRepository, "По глобальному параметру", _settings)
            };
            if(_settings.LevelHeightProvider == LevelHeightProvider.UserSettings || !FloorHeightProviders[1].IsEnabled) {
                SelectedFloorHeightProvider = FloorHeightProviders[0];
            } else {
                SelectedFloorHeightProvider = FloorHeightProviders[1];
            }
        }

        private void PlaceAnnotation(object p) {
            var marks = new TemplateLevelMarkCollection(_revitRepository, SelectedMode.SelectionMode);
            marks.CreateAnnotation(int.Parse(FloorCount), double.Parse(SelectedFloorHeightProvider.GetFloorHeight()));
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
            if(!double.TryParse(SelectedFloorHeightProvider.GetFloorHeight(), out double floorHeight)) {
                ErrorText = "Высота типового этажа должна быть числом.";
                return false;
            }
            if(floorHeight < 1) {
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
            if(SelectedFloorHeightProvider is UserFloorHeightViewModel) {
                _settings.LevelHeightProvider = LevelHeightProvider.UserSettings;
            } else {
                _settings.LevelHeightProvider = LevelHeightProvider.GlobalParameter;
            }
            foreach(var provider in FloorHeightProviders) {
                if(provider is UserFloorHeightViewModel userFloorHeight) {
                    _settings.LevelHeight = double.Parse(userFloorHeight.FloorHeight);
                }
                if(provider is GlobalFloorHeightViewModel globalFloorHeight) {
                    _settings.GlobalParameterId = globalFloorHeight?.SelectedGlobalParameter?.ElementId;
                }
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
                InfoElementsViewModel.InfoElements.Add(new InfoElementViewModel(InfoElement.FamilyAnnotationMissing, RevitRepository.FamilyTop));
            }
            var bottomFamily = _revitRepository.GetBottomAnnotaionFamily();
            if(bottomFamily != null) {
                yield return bottomFamily;
            } else {
                InfoElementsViewModel.InfoElements.Add(new InfoElementViewModel(InfoElement.FamilyAnnotationMissing, RevitRepository.FamilyBottom));
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
                        foreach(var param in notExistedParams) {
                            InfoElementsViewModel.InfoElements.Add(new InfoElementViewModel(InfoElement.AnnotationParameterMissing, family.Name, param));
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
            var result = true;
            foreach(var symbol in symbols) {
                if(!symbol.IsExistsParam(RevitRepository.ElevSymbolWidth)) {
                    InfoElementsViewModel.InfoElements.Add(new InfoElementViewModel(InfoElement.ElevationParameterMissing, symbol.Name, RevitRepository.ElevSymbolWidth));
                    result = false;
                }
                if(!symbol.IsExistsParam(RevitRepository.ElevSymbolHeight)) {
                    InfoElementsViewModel.InfoElements.Add(new InfoElementViewModel(InfoElement.ElevationParameterMissing, symbol.Name, RevitRepository.ElevSymbolHeight));
                    result = false;
                }
            }
            return result;
        }
    }
}