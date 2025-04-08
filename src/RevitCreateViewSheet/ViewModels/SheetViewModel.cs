using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.ViewModels {
    internal class SheetViewModel : BaseViewModel, IEntityViewModel {
        private readonly SheetModel _sheetModel;
        private readonly ObservableCollection<ViewPortViewModel> _allViewPorts;
        private readonly ObservableCollection<ScheduleViewModel> _allSchedules;
        private readonly ObservableCollection<AnnotationViewModel> _allAnnotations;
        private string _name;
        private string _albumBlueprint;
        private string _sheetNumber;
        private string _sheetCustomNumber;
        private TitleBlockViewModel _titleBlock;
        private ViewPortViewModel _selectedViewPort;
        private ScheduleViewModel _selectedSchedule;
        private AnnotationViewModel _selectedAnnotation;

        public SheetViewModel(SheetModel sheetModel) {
            _sheetModel = sheetModel ?? throw new System.ArgumentNullException(nameof(sheetModel));
            _albumBlueprint = _sheetModel.AlbumBlueprint;
            _name = _sheetModel.Name;
            _sheetNumber = _sheetModel.SheetNumber;
            _sheetCustomNumber = _sheetModel.SheetCustomNumber;
            _titleBlock = new TitleBlockViewModel(_sheetModel.TitleBlockSymbol);
            IsPlaced = sheetModel.State == EntityState.Unchanged;

            _allViewPorts = [.. _sheetModel.GetViewPorts().Select(v => new ViewPortViewModel(v))];
            _allSchedules = [.. _sheetModel.GetSchedules().Select(s => new ScheduleViewModel(s))];
            _allAnnotations = [.. _sheetModel.GetAnnotations().Select(a => new AnnotationViewModel(a))];

            VisibleViewPorts = new CollectionViewSource() { Source = _allViewPorts };
            VisibleViewPorts.Filter += EntitiesFilterHandler;
            VisibleSchedules = new CollectionViewSource() { Source = _allSchedules };
            VisibleSchedules.Filter += EntitiesFilterHandler;
            VisibleAnnotations = new CollectionViewSource() { Source = _allAnnotations };
            VisibleAnnotations.Filter += EntitiesFilterHandler;

            RemoveViewCommand = RelayCommand.Create<ViewPortViewModel>(RemoveView, CanRemoveView);
            RemoveScheduleCommand = RelayCommand.Create<ScheduleViewModel>(RemoveSchedule, CanRemoveSchedule);
            RemoveAnnotationCommand = RelayCommand.Create<AnnotationViewModel>(RemoveAnnotation, CanRemoveAnnotation);
        }


        public bool IsPlaced { get; }

        public IEntity Entity => SheetModel;

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

        public CollectionViewSource VisibleViewPorts { get; }

        public CollectionViewSource VisibleSchedules { get; }

        public CollectionViewSource VisibleAnnotations { get; }

        public ICommand RemoveViewCommand { get; }

        public ICommand RemoveScheduleCommand { get; }

        public ICommand RemoveAnnotationCommand { get; }


        public void AddView(ViewPortViewModel view) {
            if(view is null) {
                throw new ArgumentNullException(nameof(view));
            }
            if(view.ViewPortModel.State != EntityState.Deleted) {
                _allViewPorts.Add(view);
            }
        }

        public void AddSchedule(ScheduleViewModel schedule) {
            if(schedule is null) {
                throw new ArgumentNullException(nameof(schedule));
            }
            if(schedule.ScheduleModel.State != EntityState.Deleted) {
                _allSchedules.Add(schedule);
            }
        }

        public void AddAnnotation(AnnotationViewModel annotation) {
            if(annotation is null) {
                throw new ArgumentNullException(nameof(annotation));
            }
            if(annotation.AnnotationModel.State != EntityState.Deleted) {
                _allAnnotations.Add(annotation);
            }
        }

        /// <summary>
        /// Возвращает коллекцию видовых экранов с листа, которые не удалены
        /// </summary>
        /// <returns>Коллекция не удаленных видовых экранов</returns>
        public IReadOnlyCollection<ViewPortViewModel> GetViewPorts() {
            return [.. _allViewPorts.Where(v => v.ViewPortModel.State != EntityState.Deleted)];
        }

        private void RemoveView(ViewPortViewModel view) {
            _sheetModel.RemoveViewPort(view.ViewPortModel);
            RemoveEntityViewModel(view, _allViewPorts, VisibleViewPorts.View);
        }

        private bool CanRemoveView(ViewPortViewModel view) {
            return view is not null;
        }

        private void RemoveSchedule(ScheduleViewModel scheduleView) {
            _sheetModel.RemoveSchedule(scheduleView.ScheduleModel);
            RemoveEntityViewModel(scheduleView, _allSchedules, VisibleSchedules.View);
        }

        private bool CanRemoveSchedule(ScheduleViewModel scheduleView) {
            return scheduleView is not null;
        }

        private void RemoveAnnotation(AnnotationViewModel annotationView) {
            _sheetModel.RemoveAnnotation(annotationView.AnnotationModel);
            RemoveEntityViewModel(annotationView, _allAnnotations, VisibleAnnotations.View);
        }

        private bool CanRemoveAnnotation(AnnotationViewModel annotationView) {
            return annotationView is not null;
        }

        private void EntitiesFilterHandler(object sender, FilterEventArgs e) {
            if(e.Item is IEntityViewModel sheetViewModel) {
                if(sheetViewModel.Entity.State == EntityState.Deleted) {
                    e.Accepted = false;
                    return;
                }
            }
        }

        private void RemoveEntityViewModel<T>(
            T entityViewModel,
            ObservableCollection<T> allEntities,
            ICollectionView viewToRefresh) where T : IEntityViewModel {

            if(!entityViewModel.IsPlaced) {
                allEntities.Remove(entityViewModel);
            }
            viewToRefresh.Refresh();
        }

        private string GetSheetNumber(string albumBlueprint, string sheetCustomNumber) {
            string[] strs = [albumBlueprint, sheetCustomNumber];
            return string.Join("-", strs.Where(s => !string.IsNullOrWhiteSpace(s)));
        }
    }
}
