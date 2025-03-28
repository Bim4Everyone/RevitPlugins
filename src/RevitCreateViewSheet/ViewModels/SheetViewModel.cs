using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.ViewModels {
    internal class SheetViewModel : BaseViewModel {
        private readonly SheetModel _sheetModel;
        private string _name;
        private string _albumBlueprint;
        private string _sheetNumber;
        private TitleBlockViewModel _titleBlock;

        public SheetViewModel(SheetModel sheetModel) {
            _sheetModel = sheetModel ?? throw new System.ArgumentNullException(nameof(sheetModel));
            _albumBlueprint = _sheetModel.AlbumBlueprint;
            _name = _sheetModel.Name;
            _sheetNumber = _sheetModel.SheetNumber;
            _titleBlock = new TitleBlockViewModel(_sheetModel.TitleBlockSymbol);
            IsPlaced = sheetModel.State == EntityState.Unchanged;

            ViewPorts = [.. _sheetModel.GetViewPorts().Select(v => new ViewPortViewModel(v))];
            Schedules = [.. _sheetModel.GetSchedules().Select(s => new ScheduleViewModel(s))];
            Annotations = [.. _sheetModel.GetAnnotations().Select(a => new AnnotationViewModel(a))];

            AddViewCommand = RelayCommand.Create(AddView);
            RemoveViewCommand = RelayCommand.Create<ViewPortViewModel>(RemoveView, CanRemoveView);
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

        public ObservableCollection<ViewPortViewModel> ViewPorts { get; }


        public ObservableCollection<ScheduleViewModel> Schedules { get; }


        public ObservableCollection<AnnotationViewModel> Annotations { get; }


        public ICommand AddViewCommand { get; }

        public ICommand RemoveViewCommand { get; }

        public ICommand AddScheduleCommand { get; }

        public ICommand RemoveScheduleCommand { get; }

        public ICommand AddAnnotationCommand { get; }

        public ICommand RemoveAnnotationCommand { get; }


        private void AddView() {

        }

        private void RemoveView(ViewPortViewModel view) {
            ViewPorts.Remove(view);
        }

        private bool CanRemoveView(ViewPortViewModel view) {
            return view is not null;
        }

        private void AddSchedule() {

        }

        private void RemoveSchedule(ScheduleViewModel scheduleView) {
            Schedules.Remove(scheduleView);
        }

        private bool CanRemoveSchedule(ScheduleViewModel scheduleView) {
            return scheduleView is not null;
        }

        private void AddAnnotation() {

        }

        private void RemoveAnnotation(AnnotationViewModel annotationView) {
            Annotations.Remove(annotationView);
        }

        private bool CanRemoveAnnotation(AnnotationViewModel annotationView) {
            return annotationView is not null;
        }
    }
}
