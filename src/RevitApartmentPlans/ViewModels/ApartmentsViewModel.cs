using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitApartmentPlans.Models;

namespace RevitApartmentPlans.ViewModels {
    internal class ApartmentsViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        public ApartmentsViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig ?? throw new System.ArgumentNullException(nameof(pluginConfig));
            _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));

            ShowApartmentCommand = RelayCommand.Create<ApartmentViewModel>(ShowApartment, CanShowApartment);
            Apartments = new ObservableCollection<ApartmentViewModel>();
            Parameters = new ObservableCollection<ParamViewModel>(_revitRepository
                .GetRoomGroupingParameters()
                .OrderBy(x => x.Name)
                .Select(p => new ParamViewModel(p)));
            LoadConfig();
        }


        public ICommand ShowApartmentCommand { get; }

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

        private void LoadConfig() {
            if(_pluginConfig == null) { throw new ArgumentNullException(nameof(_pluginConfig)); }
            if(_revitRepository == null) { throw new ArgumentNullException(nameof(_revitRepository)); }
            if(Parameters == null) { throw new ArgumentNullException(nameof(Parameters)); }

            string paramName = _pluginConfig.GetSettings(_revitRepository.Document)?.ParamName ?? string.Empty;
            SelectedParam = Parameters.FirstOrDefault(p => p.Name == paramName);
        }

        private void ShowApartment(ApartmentViewModel apartmentViewModel) {
            _revitRepository.ActiveUIDocument.Selection.SetElementIds(apartmentViewModel
                .GetApartment()
                .GetRooms()
                .Select(r => r.Id)
                .ToArray());
        }

        private bool CanShowApartment(ApartmentViewModel apartmentViewModel) {
            return apartmentViewModel != null;
        }
    }
}
