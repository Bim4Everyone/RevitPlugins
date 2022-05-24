using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.UI;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.FilterModel;

namespace RevitClashDetective.ViewModels {
    internal class CheckViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly FiltersConfig _filtersConfig;
        private string _name;
        private ObservableCollection<ProvidersViewModel> _providers;
        private ObservableCollection<ProvidersViewModel> _otherDocumentProviders;
        private string _selectedMainDocProviders;
        private string _selectedOtherDocProviders;

        public CheckViewModel(RevitRepository revitRepository, FiltersConfig filtersConfig) {
            _revitRepository = revitRepository;
            _filtersConfig = filtersConfig;
            Name = "Без имени";
            MainDocumentProviders = new ObservableCollection<ProvidersViewModel>();
            OtherDocumentProviders = new ObservableCollection<ProvidersViewModel>();
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

        public ObservableCollection<ProvidersViewModel> MainDocumentProviders {
            get => _providers;
            set => this.RaiseAndSetIfChanged(ref _providers, value);
        }

        public ObservableCollection<ProvidersViewModel> OtherDocumentProviders {
            get => _otherDocumentProviders;
            set => this.RaiseAndSetIfChanged(ref _otherDocumentProviders, value);
        }

        public List<ClashModel> GetClashes() {
            var mainProviders = MainDocumentProviders
                .Where(item => item.IsSelected)
                .SelectMany(item => item.Providers)
                .ToList();
            var otherProviders = OtherDocumentProviders
                .Where(item => item.IsSelected)
                .SelectMany(item => item.Providers)
                .ToList();
            var clashDetector = new ClashDetector(mainProviders, otherProviders);
            return _revitRepository.GetClashes(clashDetector).ToList();
        }

        private void InitializeFilterProviders() {
            var filters = _filtersConfig.Filters;
            var links = _revitRepository.GetRevitLinkInstances();
            foreach(var filter in filters) {
                var mainProvider = new ProvidersViewModel(_revitRepository, filter);
                MainDocumentProviders.Add(mainProvider);
                var otherProvider = new ProvidersViewModel(_revitRepository, links, filter);
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

