using System;
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
        private string _name;
        private string _albumBlueprint;
        private string _sheetNumber;
        private TitleBlockViewModel _titleBlock;
        private ViewPortViewModel _selectedViewPort;
        private ScheduleViewModel _selectedSchedule;
        private AnnotationViewModel _selectedAnnotation;

        public SheetViewModel(SheetModel sheetModel) {
            _sheetModel = sheetModel ?? throw new System.ArgumentNullException(nameof(sheetModel));
            _albumBlueprint = _sheetModel.AlbumBlueprint;
            _name = _sheetModel.Name;
            _sheetNumber = _sheetModel.SheetNumber;
            _titleBlock = new TitleBlockViewModel(_sheetModel.TitleBlockSymbol);
            IsPlaced = sheetModel.State == EntityState.Unchanged;

            AllViewPorts = [.. _sheetModel.GetViewPorts().Select(v => new ViewPortViewModel(v))];
            AllSchedules = [.. _sheetModel.GetSchedules().Select(s => new ScheduleViewModel(s))];
            AllAnnotations = [.. _sheetModel.GetAnnotations().Select(a => new AnnotationViewModel(a))];

            VisibleViewPorts = new CollectionViewSource() { Source = AllViewPorts };
            VisibleViewPorts.Filter += EntitiesFilterHandler;
            VisibleSchedules = new CollectionViewSource() { Source = AllSchedules };
            VisibleSchedules.Filter += EntitiesFilterHandler;
            VisibleAnnotations = new CollectionViewSource() { Source = AllAnnotations };
            VisibleAnnotations.Filter += EntitiesFilterHandler;

            AddViewCommand = RelayCommand.Create(AddView);
            RemoveViewCommand = RelayCommand.Create<ViewPortViewModel>(RemoveView, CanRemoveView);
            AddScheduleCommand = RelayCommand.Create(AddSchedule);
            RemoveScheduleCommand = RelayCommand.Create<ScheduleViewModel>(RemoveSchedule, CanRemoveSchedule);
            AddAnnotationCommand = RelayCommand.Create(AddAnnotation);
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
            }
        }

        public string SheetNumber {
            get => _sheetNumber;
            set {
                RaiseAndSetIfChanged(ref _sheetNumber, value);
                _sheetModel.SheetNumber = value;
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

        public ObservableCollection<ViewPortViewModel> AllViewPorts { get; }

        public ObservableCollection<ScheduleViewModel> AllSchedules { get; }

        public ObservableCollection<AnnotationViewModel> AllAnnotations { get; }

        public ICommand AddViewCommand { get; }

        public ICommand RemoveViewCommand { get; }

        public ICommand AddScheduleCommand { get; }

        public ICommand RemoveScheduleCommand { get; }

        public ICommand AddAnnotationCommand { get; }

        public ICommand RemoveAnnotationCommand { get; }


        private void AddView() {
            throw new NotImplementedException("TODO");
        }

        private void RemoveView(ViewPortViewModel view) {
            RemoveEntity(view, AllViewPorts, VisibleViewPorts.View);
        }

        private bool CanRemoveView(ViewPortViewModel view) {
            return view is not null;
        }

        private void AddSchedule() {
            throw new NotImplementedException("TODO");

        }

        private void RemoveSchedule(ScheduleViewModel scheduleView) {
            RemoveEntity(scheduleView, AllSchedules, VisibleSchedules.View);
        }

        private bool CanRemoveSchedule(ScheduleViewModel scheduleView) {
            return scheduleView is not null;
        }

        private void AddAnnotation() {
            throw new NotImplementedException("TODO");
        }

        private void RemoveAnnotation(AnnotationViewModel annotationView) {
            RemoveEntity(annotationView, AllAnnotations, VisibleAnnotations.View);
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

        private void RemoveEntity<T>(
            T entityViewModel,
            ObservableCollection<T> allEntities,
            ICollectionView viewToRefresh) where T : IEntityViewModel {

            if(entityViewModel.IsPlaced) {
                entityViewModel.Entity.MarkAsDeleted();
            } else {
                allEntities.Remove(entityViewModel);
            }
            viewToRefresh.Refresh();
        }
    }
}
