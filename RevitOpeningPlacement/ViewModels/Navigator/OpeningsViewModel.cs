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

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitOpeningPlacement.ViewModels.Navigator {
    internal class OpeningsViewModel : BaseViewModel {
        private readonly Models.RevitRepository _revitRepository;
        private ObservableCollection<OpeningViewModel> _openings;

        public OpeningsViewModel(Models.RevitRepository revitRepository) {
            _revitRepository = revitRepository;

            InitializeOpenings();

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

        private void InitializeOpenings() {
            Openings = new ObservableCollection<OpeningViewModel>(
                _revitRepository.GetOpenings()
                .Select(item => new OpeningViewModel() {
                    Id = item.Id.IntegerValue,
                    Level = _revitRepository.GetLevel(item),
                    TypeName = item.Name,
                    FamilyName = _revitRepository.GetFamilyName(item)
                }));

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
    }
}