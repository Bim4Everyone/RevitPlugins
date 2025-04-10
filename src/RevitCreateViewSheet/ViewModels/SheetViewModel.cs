using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCreateViewSheet.Models;
using RevitCreateViewSheet.Services;

namespace RevitCreateViewSheet.ViewModels {
    internal class SheetViewModel : BaseViewModel {
        private readonly SheetModel _sheetModel;
        private readonly EntitiesTracker _entitiesTracker;
        private readonly SheetItemsFactory _sheetItemsFactory;
        private readonly ObservableCollection<ViewPortViewModel> _viewPorts;
        private readonly ObservableCollection<ScheduleViewModel> _schedules;
        private readonly ObservableCollection<AnnotationViewModel> _annotations;
        private string _name;
        private string _albumBlueprint;
        private string _sheetNumber;
        private string _sheetCustomNumber;
        private TitleBlockViewModel _titleBlock;
        private ViewPortViewModel _selectedViewPort;
        private ScheduleViewModel _selectedSchedule;
        private AnnotationViewModel _selectedAnnotation;

        public SheetViewModel(
            SheetModel sheetModel,
            EntitiesTracker entitiesTracker,
            SheetItemsFactory sheetItemsFactory) {

            _sheetModel = sheetModel ?? throw new ArgumentNullException(nameof(sheetModel));
            _entitiesTracker = entitiesTracker ?? throw new ArgumentNullException(nameof(entitiesTracker));
            _sheetItemsFactory = sheetItemsFactory ?? throw new ArgumentNullException(nameof(sheetItemsFactory));
            _albumBlueprint = _sheetModel.AlbumBlueprint;
            _name = _sheetModel.Name;
            _sheetNumber = _sheetModel.SheetNumber;
            _sheetCustomNumber = _sheetModel.SheetCustomNumber;
            if(_sheetModel.TitleBlockSymbol is not null) {
                _titleBlock = new TitleBlockViewModel(_sheetModel.TitleBlockSymbol);
            }
            IsPlaced = sheetModel.Exists;

            _viewPorts = [.. _sheetModel.ViewPorts.Select(v => new ViewPortViewModel(v))];
            _schedules = [.. _sheetModel.Schedules.Select(s => new ScheduleViewModel(s))];
            _annotations = [.. _sheetModel.Annotations.Select(a => new AnnotationViewModel(a))];

            ViewPorts = new ReadOnlyObservableCollection<ViewPortViewModel>(_viewPorts);
            Schedules = new ReadOnlyObservableCollection<ScheduleViewModel>(_schedules);
            Annotations = new ReadOnlyObservableCollection<AnnotationViewModel>(_annotations);

            AddViewPortCommand = RelayCommand.Create(AddViewPort);
            RemoveViewPortCommand = RelayCommand.Create<ViewPortViewModel>(RemoveViewPort, CanRemoveViewPort);
            AddScheduleCommand = RelayCommand.Create(AddSchedule);
            RemoveScheduleCommand = RelayCommand.Create<ScheduleViewModel>(RemoveSchedule, CanRemoveSchedule);
            AddAnnotationCommand = RelayCommand.Create(AddAnnotation);
            RemoveAnnotationCommand = RelayCommand.Create<AnnotationViewModel>(RemoveAnnotation, CanRemoveAnnotation);
        }


        public bool IsPlaced { get; }

        public SheetModel SheetModel => _sheetModel;

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
                SheetNumber = GetSheetNumber(value, SheetCustomNumber);
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
                SheetNumber = GetSheetNumber(AlbumBlueprint, value);
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

        public ReadOnlyObservableCollection<ViewPortViewModel> ViewPorts { get; }

        public ReadOnlyObservableCollection<ScheduleViewModel> Schedules { get; }

        public ReadOnlyObservableCollection<AnnotationViewModel> Annotations { get; }

        public ICommand AddViewPortCommand { get; }

        public ICommand RemoveViewPortCommand { get; }

        public ICommand AddScheduleCommand { get; }

        public ICommand RemoveScheduleCommand { get; }

        public ICommand AddAnnotationCommand { get; }

        public ICommand RemoveAnnotationCommand { get; }


        private void AddViewPort() {
            try {
                var viewPort = new ViewPortViewModel(_sheetItemsFactory.CreateViewPort(_sheetModel));
                if(_entitiesTracker.AddAliveViewPort(viewPort.ViewPortModel)) {
                    _viewPorts.Add(viewPort);
                }
            } catch(OperationCanceledException) {
                return;
            }
        }

        private void AddSchedule() {
            try {
                var schedule = new ScheduleViewModel(_sheetItemsFactory.CreateSchedule(_sheetModel));
                if(_entitiesTracker.AddAliveSchedule(schedule.ScheduleModel)) {
                    _schedules.Add(schedule);
                }
            } catch(OperationCanceledException) {
                return;
            }
        }

        private void AddAnnotation() {
            try {
                var annotation = new AnnotationViewModel(_sheetItemsFactory.CreateAnnotation(_sheetModel));
                if(_entitiesTracker.AddAliveAnnotation(annotation.AnnotationModel)) {
                    _annotations.Add(annotation);
                }
            } catch(OperationCanceledException) {
                return;
            }
        }

        private void RemoveViewPort(ViewPortViewModel viewPort) {
            _sheetModel.ViewPorts.Remove(viewPort.ViewPortModel);
            _entitiesTracker.AddToRemovedEntities(viewPort.ViewPortModel);
            _viewPorts.Remove(viewPort);
        }

        private bool CanRemoveViewPort(ViewPortViewModel viewPort) {
            return viewPort is not null;
        }

        private void RemoveSchedule(ScheduleViewModel scheduleView) {
            _sheetModel.Schedules.Remove(scheduleView.ScheduleModel);
            _entitiesTracker.AddToRemovedEntities(scheduleView.ScheduleModel);
            _schedules.Remove(scheduleView);
        }

        private bool CanRemoveSchedule(ScheduleViewModel scheduleView) {
            return scheduleView is not null;
        }

        private void RemoveAnnotation(AnnotationViewModel annotationView) {
            _sheetModel.Annotations.Remove(annotationView.AnnotationModel);
            _entitiesTracker.AddToRemovedEntities(annotationView.AnnotationModel);
            _annotations.Remove(annotationView);
        }

        private bool CanRemoveAnnotation(AnnotationViewModel annotationView) {
            return annotationView is not null;
        }

        private string GetSheetNumber(string albumBlueprint, string sheetCustomNumber) {
            string[] strs = [albumBlueprint, sheetCustomNumber];
            return string.Join("-", strs.Where(s => !string.IsNullOrWhiteSpace(s)));
        }
    }
}
