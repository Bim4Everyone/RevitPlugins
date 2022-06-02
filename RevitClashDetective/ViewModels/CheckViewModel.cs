using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

using Autodesk.Revit.UI;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.ViewModels.Navigator;
using RevitClashDetective.Views;

namespace RevitClashDetective.ViewModels {
    internal class CheckViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;
        private readonly FiltersConfig _filtersConfig;
        private string _name;
        private string errorText;
        private string _selectedMainDocProviders;
        private string _selectedOtherDocProviders;
        private ObservableCollection<ProvidersViewModel> _providers;
        private ObservableCollection<ProvidersViewModel> _otherDocumentProviders;
        private bool _hasReport;
        private string _messageText;
        private bool _isSelected;

        public CheckViewModel(RevitRepository revitRepository, FiltersConfig filtersConfig, Check check = null) {
            _revitRepository = revitRepository;
            _filtersConfig = filtersConfig;

            Name = check?.Name ?? "Без имени";

            MainDocumentProviders = new ObservableCollection<ProvidersViewModel>();
            OtherDocumentProviders = new ObservableCollection<ProvidersViewModel>();


            if(check == null) {
                InitializeFilterProviders();
                HasReport = false;
            } else {
                InitializeFilterProviders(check);
            }

            SelectMainProviderCommand = new RelayCommand(SelectProvider);
            ShowClashesCommand = new RelayCommand(ShowClashes, CanShowClashes);
        }

        public ICommand SelectMainProviderCommand { get; }
        public ICommand ShowClashesCommand { get; }
        public bool IsFilterSelected => !string.IsNullOrEmpty(SelectedOtherDocProviders) && !string.IsNullOrEmpty(SelectedMainDocProviders);

        public bool IsSelected { 
            get => _isSelected; 
            set => this.RaiseAndSetIfChanged(ref _isSelected, value); 
        }

        public bool HasReport {
            get => _hasReport;
            set => this.RaiseAndSetIfChanged(ref _hasReport, value);
        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        public string ErrorText {
            get => errorText;
            set => this.RaiseAndSetIfChanged(ref errorText, value);
        }

        public string SelectedMainDocProviders {
            get => _selectedMainDocProviders;
            set => this.RaiseAndSetIfChanged(ref _selectedMainDocProviders, value);
        }


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

        private string ReportName => $"{_revitRepository.GetDocumentName()}_{Name}";

        public List<ClashModel> GetClashes() {
            var mainProviders = MainDocumentProviders
                .Where(item => item.IsSelected)
                .SelectMany(item => item.Providers)
                .ToList();
            var otherProviders = OtherDocumentProviders
                .Where(item => item.IsSelected)
                .SelectMany(item => item.Providers)
                .ToList();
            var clashDetector = new ClashDetector(_revitRepository, mainProviders, otherProviders);
            return _revitRepository.GetClashes(clashDetector).ToList();
        }

        public void SaveClashes() {
            var config = ClashesConfig.GetFiltersConfig(_revitRepository.GetObjectName(), ReportName);
            config.Clashes = GetClashes();
            config.SaveProjectConfig();
            HasReport = true;
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

        private void InitializeFilterProviders(Check check) {
            InitializeFilterProviders();

            foreach(var provider in MainDocumentProviders) {
                provider.IsSelected = check.MainFilters.Any(item => item.Equals(provider.Name, StringComparison.CurrentCultureIgnoreCase));
            }

            foreach(var provider in OtherDocumentProviders) {
                provider.IsSelected = check.OtherFilters.Any(item => item.Equals(provider.Name, StringComparison.CurrentCultureIgnoreCase));
            }

            var missedMainFilters = check.MainFilters
                .Where(item => !MainDocumentProviders.Any(p => p.Name.Equals(item)))
                .ToList();

            if(missedMainFilters.Count > 0) {
                ErrorText = $"Не найдены фильтры основного файла: {string.Join(", ", missedMainFilters)}";
            }

            var missedOtherFilters = check.OtherFilters
                .Where(item => !OtherDocumentProviders.Any(p => p.Name.Equals(item)))
                .ToList();

            if(missedOtherFilters.Count > 0) {
                ErrorText += Environment.NewLine + $"Не найдены фильтры связанных файлов: {string.Join(", ", missedOtherFilters)}";
            }
            if(ClashesConfig.GetFiltersConfig(_revitRepository.GetObjectName(), ReportName).Clashes.Count > 0) {
                HasReport = true;
            }

            SelectProvider(null);
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

        private void ShowClashes(object p) {
            var view = new NavigatorView() { DataContext = new ClashesViewModel(_revitRepository, ReportName) };
            view.Show();
        }


        private bool CanShowClashes(object p) {
            return HasReport;
        }
    }
}