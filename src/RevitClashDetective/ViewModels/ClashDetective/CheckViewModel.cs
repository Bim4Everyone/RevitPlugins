using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.ClashDetection;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.ViewModels.Navigator;
using RevitClashDetective.Views;

namespace RevitClashDetective.ViewModels.ClashDetective {
    internal class CheckViewModel : BaseViewModel, INamedEntity {
        private readonly RevitRepository _revitRepository;
        private readonly FiltersConfig _filtersConfig;
        private string _name;
        private string _errorText;
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

            ShowClashesCommand = RelayCommand.Create(ShowClashes, CanShowClashes);
        }

        public ICommand SelectMainProviderCommand { get; }
        public ICommand ShowClashesCommand { get; }
        public bool IsFilterSelected => FirstSelection.SelectedProviders.Any() && SecondSelection.SelectedProviders.Any();
        public bool IsFilesSelected => FirstSelection.SelectedFiles.Any() && SecondSelection.SelectedFiles.Any();

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
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
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
            List<IProvider> mainProviders, otherProviders;

            if(FirstSelection.SelectedFiles.Any(item => item.Name.Equals(_revitRepository.GetDocumentName()))) {
                mainProviders = FirstSelection
                    .GetProviders()
                    .ToList();
                otherProviders = SecondSelection
                    .GetProviders()
                    .ToList();
            } else {
                mainProviders = SecondSelection
                    .GetProviders()
                    .ToList();
                otherProviders = FirstSelection
                    .GetProviders()
                    .ToList();
            }

            var clashDetector = new ClashDetector(_revitRepository, mainProviders, otherProviders);
            return clashDetector.FindClashes();
        }

        public void SaveClashes() {
            var config = ClashesConfig.GetClashesConfig(_revitRepository.GetObjectName(), ReportName);
            var oldClashes = ClashesConfig.GetClashesConfig(_revitRepository.GetObjectName(), $"{_revitRepository.GetDocumentName()}_{Name}").Clashes;
            var newClashes = GetClashes();
            config.Clashes = ClashesMarker.MarkSolvedClashes(newClashes, oldClashes).ToList();
            config.SaveProjectConfig();
            HasReport = true;
        }

        private void InitializeSelections(Check check = null) {
            FirstSelection = new SelectionViewModel(_revitRepository, _filtersConfig, check?.FirstSelection);
            SecondSelection = new SelectionViewModel(_revitRepository, _filtersConfig, check?.SecondSelection);
        }

        private void InitializeFilterProviders(Check check) {
            InitializeSelections(check);

            var firstFiles = FirstSelection.GetMissedFiles();
            if(!string.IsNullOrEmpty(firstFiles)) {
                ErrorText = $"Не найдены файлы выборки А: {firstFiles}" + Environment.NewLine;
            }
            var firstFilters = FirstSelection.GetMissedFilters();
            if(!string.IsNullOrEmpty(firstFilters)) {
                ErrorText += $"Не найдены поисковые наборы выборки A: {firstFilters}" + Environment.NewLine;
            }

            var secondFiles = SecondSelection.GetMissedFiles();
            if(!string.IsNullOrEmpty(secondFiles)) {
                ErrorText += $"Не найдены файлы выборки B: {secondFiles}" + Environment.NewLine;
            }
            var secondFilters = SecondSelection.GetMissedFilters();
            if(!string.IsNullOrEmpty(secondFilters)) {
                ErrorText += $"Не найдены поисковые наборы выборки B: {secondFilters}" + Environment.NewLine;
            }

            if(ClashesConfig.GetClashesConfig(_revitRepository.GetObjectName(), ReportName).Clashes.Count > 0) {
                HasReport = true;
            }
        }

        private void ShowClashes() {
            var view = new NavigatorView() { DataContext = new ReportsViewModel(_revitRepository, ReportName) { OpenFromClashDetector = true } };
            view.Show();
        }

        private bool CanShowClashes() {
            return HasReport;
        }
    }
}
