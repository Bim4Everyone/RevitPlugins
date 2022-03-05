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
            var spots = _revitRepository.GetSpotDimensions(SelectedMode.SelectionMode).ToList();           
            var annotations = _revitRepository.GetAnnotations().ToList();
            List<TemplateLevelMark> marks = new List<TemplateLevelMark>();
            var annotationManagers = GetAnnotationManagers();
            foreach(var annotation in annotations) {
                var spotId = (int) annotation.GetParamValueOrDefault("Id высотной отметки");
                var spot = _revitRepository.GetElement(new ElementId(spotId)) as SpotDimension;
                if(spot == null) {
                    _revitRepository.DeleteElement(annotation);
                }
                if(spots.Contains(spot, new ElementNameEquatable<SpotDimension>())) {
                    marks.Add(new TemplateLevelMark(spot, annotationManagers[_revitRepository.GetSpotOrientation(spot)], annotation));
                }
            }
            marks.AddRange(spots
                .Except(marks.Select(m => m.SpotDimension), new ElementNameEquatable<SpotDimension>())
                .Select(s => new TemplateLevelMark(s, annotationManagers[_revitRepository.GetSpotOrientation(s)])));

            using(TransactionGroup t = _revitRepository.StartTransactionGroup("Обновление и расстановка аннотаций")) {
                foreach(var mark in marks) {
                    if(mark.Annotation == null) {
                        mark.CreateAnnotation(FloorCount, SelectedFloorHeightProvider.GetFloorHeight());
                    } else {
                        mark.UpdateAnnotation(FloorCount, SelectedFloorHeightProvider.GetFloorHeight());
                    }
                }
                t.Assimilate();
            }
        }

        private Dictionary<SpotOrientation, AnnotationManager> GetAnnotationManagers() {
            return new Dictionary<SpotOrientation, AnnotationManager> {
                { SpotOrientation.RightTop, new RightTopAnnotationManager(_revitRepository) },
                { SpotOrientation.LeftTop, new LeftTopAnnotationManager(_revitRepository) },
                { SpotOrientation.LeftBottom, new LeftBottomAnnotationManager(_revitRepository) },
                { SpotOrientation.RightBottom, new RightBottomAnnotationManager(_revitRepository) }
            };
        }
    }
}
