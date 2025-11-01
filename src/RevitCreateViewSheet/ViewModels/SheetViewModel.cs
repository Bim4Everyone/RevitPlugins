using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCreateViewSheet.Models;
using RevitCreateViewSheet.Services;

namespace RevitCreateViewSheet.ViewModels {
    internal class SheetViewModel : BaseViewModel {
        private readonly SheetModel _sheetModel;
        private readonly EntitiesTracker _entitiesTracker;
        private readonly SheetItemsFactory _sheetItemsFactory;
        private readonly ILocalizationService _localizationService;
        private readonly ObservableCollection<ViewPortViewModel> _viewPorts;
        private readonly ObservableCollection<ScheduleViewModel> _schedules;
        private readonly ObservableCollection<AnnotationViewModel> _annotations;
        private string _name;
        private string _albumBlueprint;
        private string _sheetNumber;
        private string _sheetCustomNumber;
        private bool _sheetNumberByMask = true;
        private TitleBlockViewModel _titleBlock;
        private ViewPortViewModel _selectedViewPort;
        private ScheduleViewModel _selectedSchedule;
        private AnnotationViewModel _selectedAnnotation;
        private OrientationViewModel _orientation;
        private SheetFormatViewModel _sheetFormat;

        public SheetViewModel(
            SheetModel sheetModel,
            EntitiesTracker entitiesTracker,
            SheetItemsFactory sheetItemsFactory,
            ILocalizationService localizationService) {

            _sheetModel = sheetModel ?? throw new ArgumentNullException(nameof(sheetModel));
            _entitiesTracker = entitiesTracker ?? throw new ArgumentNullException(nameof(entitiesTracker));
            _sheetItemsFactory = sheetItemsFactory ?? throw new ArgumentNullException(nameof(sheetItemsFactory));
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
            _albumBlueprint = _sheetModel.AlbumBlueprint;
            _name = _sheetModel.Name;
            _sheetNumber = _sheetModel.SheetNumber;
            _sheetCustomNumber = _sheetModel.SheetCustomNumber;
            _orientation = new OrientationViewModel(_localizationService, _sheetModel.IsBookOrientation);
            _sheetFormat = new SheetFormatViewModel(_sheetModel.SheetFormat);
            if(_sheetModel.TitleBlockSymbol is not null) {
                _titleBlock = new TitleBlockViewModel(_sheetModel.TitleBlockSymbol);
            }
            IsPlacedStatus = localizationService.GetLocalizedString(
                sheetModel.Exists ? "EntityState.Exist" : "EntityState.New");

            _viewPorts = [.. _sheetModel.ViewPorts.Select(v => new ViewPortViewModel(v, localizationService))
                .OrderBy(v => v.ViewName)];
            _schedules = [.. _sheetModel.Schedules.Select(s => new ScheduleViewModel(s, localizationService))
                .OrderBy(s => s.Name)];
            _annotations = [.. _sheetModel.Annotations.Select(a => new AnnotationViewModel(a, localizationService))
                .OrderBy(a => a.SymbolName)];
            InitializeTrackedEntities();

            ViewPorts = new ReadOnlyObservableCollection<ViewPortViewModel>(_viewPorts);
            Schedules = new ReadOnlyObservableCollection<ScheduleViewModel>(_schedules);
            Annotations = new ReadOnlyObservableCollection<AnnotationViewModel>(_annotations);

            AddViewPortCommand = RelayCommand.Create(AddViewPort);
            RemoveViewPortCommand = RelayCommand.Create<ViewPortViewModel>(RemoveViewPort, CanRemoveViewPort);
            ReplaceViewPortCommand = RelayCommand.Create<ViewPortViewModel>(ReplaceViewPort, CanRemoveViewPort);
            AddScheduleCommand = RelayCommand.Create(AddSchedule);
            RemoveScheduleCommand = RelayCommand.Create<ScheduleViewModel>(RemoveSchedule, CanRemoveSchedule);
            ReplaceScheduleCommand = RelayCommand.Create<ScheduleViewModel>(ReplaceSchedule, CanRemoveSchedule);
            AddAnnotationCommand = RelayCommand.Create(AddAnnotation);
            RemoveAnnotationCommand = RelayCommand.Create<AnnotationViewModel>(RemoveAnnotation, CanRemoveAnnotation);
            ReplaceAnnotationCommand = RelayCommand.Create<AnnotationViewModel>(ReplaceAnnotation, CanRemoveAnnotation);
        }


        public string IsPlacedStatus { get; }

        public SheetModel SheetModel => _sheetModel;

        /// <summary>
        /// Включает/выключает назначение системного номера в формате Альбом-Ш.Номер листа
        /// </summary>
        public bool SheetNumberByMask {
            get => _sheetNumberByMask;
            set {
                _sheetNumberByMask = value;
                if(value) {
                    SheetNumber = GetSheetNumber(AlbumBlueprint, SheetCustomNumber);
                }
            }
        }

        public string Name {
            get => _name;
            set {
                RaiseAndSetIfChanged(ref _name, value);
                _sheetModel.Name = value;
            }
        }

        public string AlbumBlueprint {
            get => _albumBlueprint;
            set {
                RaiseAndSetIfChanged(ref _albumBlueprint, value);
                _sheetModel.AlbumBlueprint = value;
                if(SheetNumberByMask) {
                    SheetNumber = GetSheetNumber(value, SheetCustomNumber);
                }
            }
        }

        /// <summary>
        /// Системный номер листа
        /// </summary>
        public string SheetNumber {
            get => _sheetNumber;
            set {
                RaiseAndSetIfChanged(ref _sheetNumber, value);
                _sheetModel.SheetNumber = value;
            }
        }

        /// <summary>
        /// Ш.Номер листа
        /// </summary>
        public string SheetCustomNumber {
            get => _sheetCustomNumber;
            set {
                RaiseAndSetIfChanged(ref _sheetCustomNumber, value);
                _sheetModel.SheetCustomNumber = value;
                if(SheetNumberByMask) {
                    SheetNumber = GetSheetNumber(AlbumBlueprint, value);
                }
            }
        }

        public TitleBlockViewModel TitleBlock {
            get => _titleBlock;
            set {
                RaiseAndSetIfChanged(ref _titleBlock, value);
                _sheetModel.TitleBlockSymbol = value?.TitleBlockSymbol;
            }
        }

        public ViewPortViewModel SelectedViewPort {
            get => _selectedViewPort;
            set => RaiseAndSetIfChanged(ref _selectedViewPort, value);
        }

        public ScheduleViewModel SelectedSchedule {
            get => _selectedSchedule;
            set => RaiseAndSetIfChanged(ref _selectedSchedule, value);
        }

        public AnnotationViewModel SelectedAnnotation {
            get => _selectedAnnotation;
            set => RaiseAndSetIfChanged(ref _selectedAnnotation, value);
        }

        public OrientationViewModel Orientation {
            get => _orientation;
            set {
                RaiseAndSetIfChanged(ref _orientation, value);
                _sheetModel.IsBookOrientation = value?.IsBookOrientation ?? false;
            }
        }

        public SheetFormatViewModel SheetFormat {
            get => _sheetFormat;
            set {
                RaiseAndSetIfChanged(ref _sheetFormat, value);
                _sheetModel.SheetFormat = value?.SheetFormat ?? new SheetFormat();
            }
        }

        public ReadOnlyObservableCollection<ViewPortViewModel> ViewPorts { get; }

        public ReadOnlyObservableCollection<ScheduleViewModel> Schedules { get; }

        public ReadOnlyObservableCollection<AnnotationViewModel> Annotations { get; }

        public ICommand AddViewPortCommand { get; }

        public ICommand RemoveViewPortCommand { get; }

        public ICommand ReplaceViewPortCommand { get; }

        public ICommand AddScheduleCommand { get; }

        public ICommand RemoveScheduleCommand { get; }

        public ICommand ReplaceScheduleCommand { get; }

        public ICommand AddAnnotationCommand { get; }

        public ICommand RemoveAnnotationCommand { get; }

        public ICommand ReplaceAnnotationCommand { get; }


        private void AddViewPort() {
            try {
                SelectedViewPort = CreateViewPort();
            } catch(OperationCanceledException) {
                return;
            }
        }

        private ViewPortViewModel CreateViewPort() {
            var viewPort = new ViewPortViewModel(
                _sheetItemsFactory.CreateViewPort(_sheetModel), _localizationService);
            _viewPorts.Add(viewPort);
            _sheetModel.ViewPorts.Add(viewPort.ViewPortModel);
            _entitiesTracker.AddAliveViewPort(viewPort.ViewPortModel);
            return viewPort;
        }

        private void ReplaceViewPort(ViewPortViewModel existingViewPort) {
            try {
                SelectedViewPort = CreateViewPort();
                SelectedViewPort.ViewPortModel.Location = existingViewPort.ViewPortModel.Location;
                SelectedViewPort.ViewPortType = existingViewPort.ViewPortType;

                RemoveViewPort(existingViewPort);
            } catch(OperationCanceledException) {
                return;
            }
        }

        private void AddSchedule() {
            try {
                SelectedSchedule = CreateSchedule();
            } catch(OperationCanceledException) {
                return;
            }
        }

        private ScheduleViewModel CreateSchedule() {
            var schedule = new ScheduleViewModel(
                _sheetItemsFactory.CreateSchedule(_sheetModel), _localizationService);
            _schedules.Add(schedule);
            _sheetModel.Schedules.Add(schedule.ScheduleModel);
            _entitiesTracker.AddAliveSchedule(schedule.ScheduleModel);
            return schedule;
        }

        private void ReplaceSchedule(ScheduleViewModel existingSchedule) {
            try {
                SelectedSchedule = CreateSchedule();
                SelectedSchedule.ScheduleModel.Location = existingSchedule.ScheduleModel.Location;

                RemoveSchedule(existingSchedule);
            } catch(OperationCanceledException) {
                return;
            }
        }

        private void AddAnnotation() {
            try {
                SelectedAnnotation = CreateAnnotation();
            } catch(OperationCanceledException) {
                return;
            }
        }

        private AnnotationViewModel CreateAnnotation() {
            var annotation = new AnnotationViewModel(
                _sheetItemsFactory.CreateAnnotation(_sheetModel), _localizationService);
            _annotations.Add(annotation);
            _sheetModel.Annotations.Add(annotation.AnnotationModel);
            _entitiesTracker.AddAliveAnnotation(annotation.AnnotationModel);
            return annotation;
        }

        private void ReplaceAnnotation(AnnotationViewModel existingAnnotation) {
            try {
                SelectedAnnotation = CreateAnnotation();
                SelectedAnnotation.AnnotationModel.Location = existingAnnotation.AnnotationModel.Location;

                RemoveAnnotation(existingAnnotation);
            } catch(OperationCanceledException) {
                return;
            }
        }

        private void RemoveViewPort(ViewPortViewModel viewPort) {
            _viewPorts.Remove(viewPort);
            _sheetModel.ViewPorts.Remove(viewPort.ViewPortModel);
            _entitiesTracker.AddToRemovedEntities(viewPort.ViewPortModel);
        }

        private bool CanRemoveViewPort(ViewPortViewModel viewPort) {
            return viewPort is not null;
        }

        private void RemoveSchedule(ScheduleViewModel scheduleView) {
            _schedules.Remove(scheduleView);
            _sheetModel.Schedules.Remove(scheduleView.ScheduleModel);
            _entitiesTracker.AddToRemovedEntities(scheduleView.ScheduleModel);
        }

        private bool CanRemoveSchedule(ScheduleViewModel scheduleView) {
            return scheduleView is not null;
        }

        private void RemoveAnnotation(AnnotationViewModel annotationView) {
            _annotations.Remove(annotationView);
            _sheetModel.Annotations.Remove(annotationView.AnnotationModel);
            _entitiesTracker.AddToRemovedEntities(annotationView.AnnotationModel);
        }

        private bool CanRemoveAnnotation(AnnotationViewModel annotationView) {
            return annotationView is not null;
        }

        private string GetSheetNumber(string albumBlueprint, string sheetCustomNumber) {
            string[] strs = [albumBlueprint, sheetCustomNumber];
            return string.Join("-", strs.Where(s => !string.IsNullOrWhiteSpace(s)));
        }

        private void InitializeTrackedEntities() {
            _entitiesTracker.AddAliveSheet(_sheetModel);
            foreach(var viewPort in _sheetModel.ViewPorts) {
                _entitiesTracker.AddAliveViewPort(viewPort);
            }
            foreach(var schedule in _sheetModel.Schedules) {
                _entitiesTracker.AddAliveSchedule(schedule);
            }
            foreach(var annotation in _sheetModel.Annotations) {
                _entitiesTracker.AddAliveAnnotation(annotation);
            }
        }
    }
}
