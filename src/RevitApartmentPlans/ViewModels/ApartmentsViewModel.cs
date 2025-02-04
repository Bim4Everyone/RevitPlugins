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
                UpdateApartments(value?.Name, ProcessLinks);
            }
        }


        public ObservableCollection<ApartmentViewModel> Apartments { get; }

        public ObservableCollection<ParamViewModel> Parameters { get; }


        private bool _processLinks;
        public bool ProcessLinks {
            get => _processLinks;
            set {
                RaiseAndSetIfChanged(ref _processLinks, value);
                UpdateApartments(SelectedParam?.Name, value);
            }
        }


        private void UpdateApartments(string paramName, bool processLinks) {
            Apartments.Clear();

            if(!string.IsNullOrWhiteSpace(paramName)) {
                ICollection<Apartment> apartments = _revitRepository.GetApartments(paramName, processLinks);
                foreach(Apartment item in apartments) {
                    Apartments.Add(new ApartmentViewModel(item));
                }
            }
        }

        private void LoadConfig() {
            if(_pluginConfig == null) { throw new ArgumentNullException(nameof(_pluginConfig)); }
            if(_revitRepository == null) { throw new ArgumentNullException(nameof(_revitRepository)); }
            if(Parameters == null) { throw new ArgumentNullException(nameof(Parameters)); }

            var settings = _pluginConfig.GetSettings(_revitRepository.Document);
            string paramName = settings?.ParamName ?? string.Empty;
            SelectedParam = Parameters.FirstOrDefault(p => p.Name == paramName);
            ProcessLinks = settings?.ProcessLinks ?? false;
        }

        private void ShowApartment(ApartmentViewModel apartmentViewModel) {
            _revitRepository.ShowApartment(apartmentViewModel.GetApartment());
        }

        private bool CanShowApartment(ApartmentViewModel apartmentViewModel) {
            return apartmentViewModel != null;
        }
    }
}
