using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;

namespace RevitOpeningPlacement.ViewModels.Navigator {
    internal class OpeningsViewModel : BaseViewModel {
        private readonly Models.RevitRepository _revitRepository;
        private readonly RevitRepository _clashRevitRepository;
        private ObservableCollection<OpeningViewModel> _openings;

        public OpeningsViewModel(Models.RevitRepository revitRepository, RevitRepository clashRevitRepository) {
            _revitRepository = revitRepository;
            _clashRevitRepository = clashRevitRepository;

            InitializeOpenings();

            SelectCommand = new RelayCommand(Select);
        }

        public ObservableCollection<OpeningViewModel> Openings {
            get => _openings;
            set => this.RaiseAndSetIfChanged(ref _openings, value);
        }

        public ICommand SelectCommand { get; }

        private void InitializeOpenings() {
            Openings = new ObservableCollection<OpeningViewModel>(
                _revitRepository.GetOpenings()
                .Select(item => new OpeningViewModel() {
                    Id = item.Id.IntegerValue,
                    Level = _clashRevitRepository.GetLevel(item),
                    TypeName = item.Name,
                    FamilyName = _revitRepository.GetFamilyName(item)
                }));
        }

        private void Select(object p) {
            var opening = p as OpeningViewModel;
            if(opening == null)
                return;
            var element = _clashRevitRepository.GetElement(new ElementId(opening.Id));
            _clashRevitRepository.SelectAndShowElement(new[] { element.Id }, element.get_BoundingBox(null));
        }
    }
}