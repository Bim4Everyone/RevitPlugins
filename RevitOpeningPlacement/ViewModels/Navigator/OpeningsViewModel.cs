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
        private ObservableCollection<OpeningViewModel> _selectedOpenings;

        public OpeningsViewModel(Models.RevitRepository revitRepository, ICollection<OpeningsGroup> openingsGroups) {
            _revitRepository = revitRepository;

            InitializeOpenings(openingsGroups);

            SelectCommand = new RelayCommand(Select);
            SelectionChangedCommand = new RelayCommand(SelectionChanged, CanSelect);
        }

        public ObservableCollection<OpeningViewModel> Openings {
            get => _openings;
            set => this.RaiseAndSetIfChanged(ref _openings, value);
        }

        public CollectionViewSource OpeningsViewSource { get; set; }

        public ICommand SelectCommand { get; }
        public ICommand SelectionChangedCommand { get; }

        private void InitializeOpenings(ICollection<OpeningsGroup> openingsGroups) {
            var openings = openingsGroups.SelectMany(item => OpeningViewModel.GetOpenings(_revitRepository, item)).ToList();
            var parentsIds = openings.Select(item => item.ParentId).Distinct().ToArray();

            Openings = new ObservableCollection<OpeningViewModel>(openings.OrderByDescending(item => parentsIds.FirstOrDefault(id => id == item.ParentId)));

            OpeningsViewSource = new CollectionViewSource() { Source = Openings };
        }

        private void Select(object p) {
            if(!(p is OpeningViewModel opening))
                return;
            var elements = new[] { _revitRepository.GetElement(new ElementId(opening.Id)) };
            _revitRepository.SelectAndShowElement(elements);
        }

        private void SelectElements(ICollection<OpeningViewModel> openings) {
            var elements = openings
                .Select(item => _revitRepository.GetElement(new ElementId(item.Id)))
                .ToArray();
            _revitRepository.SelectAndShowElement(elements);
        }

        private void SelectionChanged(object p) {
            if(OpeningsViewSource.View.CurrentPosition > -1
                && OpeningsViewSource.View.CurrentPosition < Openings.Count) {
                SelectElements((ObservableCollection<OpeningViewModel>) p);
            }
        }

        private bool CanSelect(object p) {
            return p is ObservableCollection<OpeningViewModel>;
        }
    }
}