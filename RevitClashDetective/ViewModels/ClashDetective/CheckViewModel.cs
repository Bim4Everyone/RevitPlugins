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
using RevitClashDetective.ViewModels.ClashDetective;
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
        private bool _hasReport;
        private bool _isSelected;
        private SelectionViewModel _firstSelection;
        private SelectionViewModel _secondSelection;

        public CheckViewModel(RevitRepository revitRepository, FiltersConfig filtersConfig, Check check = null) {
            _revitRepository = revitRepository;
            _filtersConfig = filtersConfig;

            Name = check?.Name ?? "Без имени";


            if(check == null) {
                InitializeSelections();
                HasReport = false;
            } else {
                InitializeFilterProviders(check);
            }

            ShowClashesCommand = new RelayCommand(ShowClashes, CanShowClashes);
        }

        public ICommand SelectMainProviderCommand { get; }
        public ICommand ShowClashesCommand { get; }
        public bool IsFilterSelected => !string.IsNullOrEmpty(FirstSelection.SelectedPoviders) && !string.IsNullOrEmpty(SecondSelection.SelectedPoviders);
        public bool IsFilesSelected => !string.IsNullOrEmpty(FirstSelection.SelectedFiles) && !string.IsNullOrEmpty(SecondSelection.SelectedFiles);

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

        public SelectionViewModel FirstSelection {
            get => _firstSelection;
            set => this.RaiseAndSetIfChanged(ref _firstSelection, value);
        }

        public SelectionViewModel SecondSelection { 
            get => _secondSelection; 
            set => this.RaiseAndSetIfChanged(ref _secondSelection, value); 
        }

        private string ReportName => $"{_revitRepository.GetDocumentName()}_{Name}";

        public List<ClashModel> GetClashes() {
            var mainProviders =  FirstSelection
                .GetProviders()
                .ToList();
            var otherProviders = SecondSelection
                .GetProviders()
                .ToList();
            var clashDetector = new ClashDetector(_revitRepository, mainProviders, otherProviders);
            return clashDetector.FindClashes();
        }

        public void SaveClashes() {
            var config = ClashesConfig.GetFiltersConfig(_revitRepository.GetObjectName(), ReportName);
            config.Clashes = GetClashes();
            config.SaveProjectConfig();
            HasReport = true;
        }


        private void InitializeSelections() {
            FirstSelection = new SelectionViewModel(_revitRepository, _filtersConfig);
            SecondSelection = new SelectionViewModel(_revitRepository, _filtersConfig);
        }

        private void InitializeFilterProviders(Check check) {
            InitializeSelections();

            foreach(var provider in FirstSelection.Providers) {
                provider.IsSelected = check.MainFilters.Any(item => item.Equals(provider.Name, StringComparison.CurrentCultureIgnoreCase));
            }

            foreach(var provider in SecondSelection.Providers) {
                provider.IsSelected = check.OtherFilters.Any(item => item.Equals(provider.Name, StringComparison.CurrentCultureIgnoreCase));
            }

            var missedMainFilters = check.MainFilters
                .Where(item => !FirstSelection.Providers.Any(p => p.Name.Equals(item)))
                .ToList();

            if(missedMainFilters.Count > 0) {
                ErrorText = $"Не найдены поисковые набора первой выборки: {string.Join(", ", missedMainFilters)}";
            }

            var missedOtherFilters = check.OtherFilters
                .Where(item => !SecondSelection.Providers.Any(p => p.Name.Equals(item)))
                .ToList();

            if(missedOtherFilters.Count > 0) {
                ErrorText += Environment.NewLine + $"Не найдены поисковые наборы второй выборки: {string.Join(", ", missedOtherFilters)}";
            }
            if(ClashesConfig.GetFiltersConfig(_revitRepository.GetObjectName(), ReportName).Clashes.Count > 0) {
                HasReport = true;
            }

            FirstSelection.SelectProviders(null);
            SecondSelection.SelectProviders(null);
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