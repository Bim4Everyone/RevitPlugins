using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;

using RevitApartmentPlans.Models;

namespace RevitApartmentPlans.ViewModels {
    internal class ApartmentsViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;

        public ApartmentsViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));
        }


        private ParamViewModel _selectedParam;
        public ParamViewModel SelectedParam {
            get => _selectedParam;
            set {
                RaiseAndSetIfChanged(ref _selectedParam, value);
            }
        }


        public ObservableCollection<ApartmentViewModel> Apartments { get; }


        private void UpdateApartments(string paramName) {

        }
    }
}
