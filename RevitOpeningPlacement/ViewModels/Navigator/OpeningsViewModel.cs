using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

using Autodesk.Revit.DB;

using DevExpress.Mvvm;
using DevExpress.Mvvm.Xpf;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models.OpeningUnion;

namespace RevitOpeningPlacement.ViewModels.Navigator {
    internal class OpeningsViewModel : BaseViewModel {
        private readonly Models.RevitRepository _revitRepository;
        private ObservableCollection<OpeningViewModel> _openings;

        public OpeningsViewModel(Models.RevitRepository revitRepository, ICollection<OpeningsGroup> openingsGroups) {
            _revitRepository = revitRepository;

            InitializeOpenings(openingsGroups);

            SelectCommand = new RelayCommand(Select);
            SelectionChangedCommand = new RelayCommand(SelectionChanged);

        }

        public ObservableCollection<OpeningViewModel> Openings {
            get => _openings;
            set => this.RaiseAndSetIfChanged(ref _openings, value);
        }

        public CollectionViewSource OpeningsViewSource { get; set; }

        public ICommand SelectCommand { get; }
        public ICommand SelectionChangedCommand { get; }
        public ICommand<RowSortArgs> CustomColumnSortCommand { get; }

        private void InitializeOpenings(ICollection<OpeningsGroup> openingsGroups) {
            var openings = openingsGroups.SelectMany(item => OpeningViewModel.GetOpenings(_revitRepository, item)).ToList();
            var parentsIds = openings.Select(item => item.ParentId).Distinct();

            Openings = new ObservableCollection<OpeningViewModel>(openings.OrderByDescending(item => parentsIds.FirstOrDefault(id => id == item.Id)));

            OpeningsViewSource = new CollectionViewSource() { Source = Openings };
        }

        private void Select(object p) {
            if(!(p is OpeningViewModel opening))
                return;
            var elements = new[] { _revitRepository.GetElement(new ElementId(opening.Id)) };
            _revitRepository.SelectAndShowElement(elements);
        }

        private void SelectionChanged(object p) {
            if(OpeningsViewSource.View.CurrentPosition > -1
                && OpeningsViewSource.View.CurrentPosition < Openings.Count) {
                Select(OpeningsViewSource.View.CurrentItem);
            }
        }

        public void CustomSort(object p) {
            var args = (RowSortArgs) p;
            args.Result = ((OpeningViewModel) args.FirstItem).ParentId.CompareTo((OpeningViewModel) args.SecondItem);
        }

        public bool CanSort(object p) {
            return p != null && p is RowSortArgs;
        }
    }
}