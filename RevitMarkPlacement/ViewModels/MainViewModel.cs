using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitMarkPlacement.Models;

namespace RevitMarkPlacement.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private int _floorCount;
        private string _selectedParameterName;
        private List<SelectionModeViewModel> _selectionModes;
        private SelectionModeViewModel _selectedMode;
        private List<IFloorHeightProvider> _floorHeightProviders;
        private IFloorHeightProvider _selectedFloorHeightProvider;

        public MainViewModel(RevitRepository revitRepository) {
            this._revitRepository = revitRepository;
            InitializeSelectionModes();
            InitializeFloorHeightProvider();
            PlaceAnnotationCommand = new RelayCommand(PlaceAnnotation);
        }

        public int FloorCount {
            get => _floorCount;
            set => this.RaiseAndSetIfChanged(ref _floorCount, value);
        }

        public string SelectedParameterName {
            get => _selectedParameterName;
            set => this.RaiseAndSetIfChanged(ref _selectedParameterName, value);
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

        public List<SelectionModeViewModel> SelectionModes {
            get => _selectionModes;
            set => this.RaiseAndSetIfChanged(ref _selectionModes, value);
        }

        public ICommand PlaceAnnotationCommand { get; set; }

        private void InitializeSelectionModes() {
            SelectionModes = new List<SelectionModeViewModel>() {
                new SelectionModeViewModel(_revitRepository, new AllElementsSelection(), "Создать по всему проекту"),
                new SelectionModeViewModel(_revitRepository, new ElementsSelection(), "Создать по выбранным элементам")
            };
            SelectedMode = SelectionModes[1];
        }

        private void InitializeFloorHeightProvider() {
            FloorHeightProviders = new List<IFloorHeightProvider>() {
                new UserFloorHeightViewModel("Индивидуальная настройка"),
                new GlobalFloorHeightViewModel(_revitRepository, "По глобальному параметру")
            };
            SelectedFloorHeightProvider = FloorHeightProviders[1];
        }

        private void PlaceAnnotation(object p) {
            var spots = _revitRepository.GetSpotDimensions(SelectedMode.SelectionMode);
            AnnotationManager am = null;
            var rtam = new RightTopAnnotationManager(_revitRepository);
            using(Transaction t = _revitRepository.StartTransaction("Обновление и расстановка аннотаций")) {
                foreach(var spot in spots) {
                    var orientation = _revitRepository.GetSpotOrientation(spot);
                    switch(orientation) {
                        case SpotOrientation.RightTop: {
                            am = rtam;
                            break;
                        }
                        case SpotOrientation.RightBottom: {
                            am = null;
                            break;
                        }
                        case SpotOrientation.LeftBottom: {
                            am = null;
                            break;
                        }
                        case SpotOrientation.LeftTop: {
                            am = null;
                            break;
                        }
                    }
                    am?.UpdateAndPlaceAnnotation(spot, null, FloorCount, SelectedFloorHeightProvider.GetFloorHeight());
                }
                t.Commit();
            }
        }
    }

    internal enum ParameterMode {
        [Description("Индивидульная настройка")]
        Individual,
        [Description("По глобальному параметру")]
        GlobalParameter
    }
}
