using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;

namespace RevitClashDetective.ViewModels {
    internal class CheckViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private string _name;
        private ObservableCollection<ProviderViewModel> _providers;
        private ObservableCollection<ProviderViewModel> _otherDocumentProviders;
        private string _selectedMainDocProviders;
        private string _selectedOtherDocProviders;

        public CheckViewModel(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
            Name = "Без имени";
            MainDocumentProviders = new ObservableCollection<ProviderViewModel>();
            OtherDocumentProviders = new ObservableCollection<ProviderViewModel>();
            InitializeFilterProviders();
            SelectMainProviderCommand = new RelayCommand(SelectProvider);
        }

        public ICommand SelectMainProviderCommand { get; }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public string SelectedMainDocProviders {
            get => _selectedMainDocProviders;
            set => this.RaiseAndSetIfChanged(ref _selectedMainDocProviders, value);
        }

        public bool IsFilterSelected => !string.IsNullOrEmpty(SelectedOtherDocProviders) && !string.IsNullOrEmpty(SelectedMainDocProviders);

        public string SelectedOtherDocProviders {
            get => _selectedOtherDocProviders;
            set => this.RaiseAndSetIfChanged(ref _selectedOtherDocProviders, value);
        }

        public ObservableCollection<ProviderViewModel> MainDocumentProviders {
            get => _providers;
            set => this.RaiseAndSetIfChanged(ref _providers, value);
        }

        public ObservableCollection<ProviderViewModel> OtherDocumentProviders {
            get => _otherDocumentProviders;
            set => this.RaiseAndSetIfChanged(ref _otherDocumentProviders, value);
        }

        private void InitializeFilterProviders() {
            var filters = _revitRepository.GetFilters();
            foreach(var filter in filters) {
                var mainProvider = new ProviderViewModel(filter);
                MainDocumentProviders.Add(mainProvider);
                var otherProvider = new ProviderViewModel(filter);
                OtherDocumentProviders.Add(otherProvider);
            }
        }

        private void SelectProvider(object p) {
            SelectedMainDocProviders = string.Join(", ",
                MainDocumentProviders
                .Where(item => item.IsSelected)
                .Select(item => item.Name));
            SelectedOtherDocProviders = string.Join(", ",
                OtherDocumentProviders
                .Where(item => item.IsSelected)
                .Select(item => item.Name));
        }
    }
}

