using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using dosymep.WPF.ViewModels;

using RevitApartmentPlans.Models;

namespace RevitApartmentPlans.ViewModels {
    internal class ApartmentsViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;

        public ApartmentsViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));

            Apartments = new ObservableCollection<ApartmentViewModel>();
            Parameters = new ObservableCollection<ParamViewModel>(_revitRepository
                .GetRoomGroupingParameters()
                .OrderBy(x => x.Name)
                .Select(p => new ParamViewModel(p)));
        }


        private ParamViewModel _selectedParam;
        public ParamViewModel SelectedParam {
            get => _selectedParam;
            set {
                RaiseAndSetIfChanged(ref _selectedParam, value);
                UpdateApartments(value?.Name);
            }
        }


        public ObservableCollection<ApartmentViewModel> Apartments { get; }

        public ObservableCollection<ParamViewModel> Parameters { get; }


        private void UpdateApartments(string paramName) {
            Apartments.Clear();

            if(!string.IsNullOrWhiteSpace(paramName)) {
                ICollection<Apartment> apartments = _revitRepository.GetApartments(paramName);
                foreach(Apartment item in apartments) {
                    Apartments.Add(new ApartmentViewModel(item));
                }
            }
        }
    }
}
