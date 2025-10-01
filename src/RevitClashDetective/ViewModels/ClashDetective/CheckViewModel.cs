using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.ClashDetection;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Models.Interfaces;
using RevitClashDetective.ViewModels.Navigator;
using RevitClashDetective.Views;

namespace RevitClashDetective.ViewModels.ClashDetective;
internal class CheckViewModel : BaseViewModel, INamedEntity {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly FiltersConfig _filtersConfig;
    private string _name;
    private string _errorText;
    private bool _hasReport;
    private bool _isSelected;
    private SelectionViewModel _firstSelection;
    private SelectionViewModel _secondSelection;

    public CheckViewModel(RevitRepository revitRepository,
        ILocalizationService localizationService,
        FiltersConfig filtersConfig,
        Check check = null) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _filtersConfig = filtersConfig ?? throw new ArgumentNullException(nameof(filtersConfig));

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
        set => RaiseAndSetIfChanged(ref _isSelected, value);
    }

    public bool HasReport {
        get => _hasReport;
        set => RaiseAndSetIfChanged(ref _hasReport, value);
    }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public SelectionViewModel FirstSelection {
        get => _firstSelection;
        set => RaiseAndSetIfChanged(ref _firstSelection, value);
    }

    public SelectionViewModel SecondSelection {
        get => _secondSelection;
        set => RaiseAndSetIfChanged(ref _secondSelection, value);
    }

    public string ReportName => $"{_revitRepository.GetDocumentName()}_{Name}";

    /// <summary>
    /// Возвращает провайдеры для поиска коллизий. MainProviders - выборка 1 (слева), OtherProviders - выборка 2 (справа).
    /// </summary>
    /// <returns>Провайдер выборки 1 (слева) и провайдер выборки 2 (справа)</returns>
    public (List<IProvider> MainProviders, List<IProvider> OtherProviders) GetProviders() {
        List<IProvider> mainProviders;
        List<IProvider> otherProviders;
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
        return (mainProviders, otherProviders);
    }

    public List<ClashModel> GetClashes() {
        (var mainProviders, var otherProviders) = GetProviders();

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

        string firstFiles = FirstSelection.GetMissedFiles();
        if(!string.IsNullOrEmpty(firstFiles)) {
            ErrorText = $"Не найдены файлы выборки А: {firstFiles}" + Environment.NewLine;
        }
        string firstFilters = FirstSelection.GetMissedFilters();
        if(!string.IsNullOrEmpty(firstFilters)) {
            ErrorText += $"Не найдены поисковые наборы выборки A: {firstFilters}" + Environment.NewLine;
        }

        string secondFiles = SecondSelection.GetMissedFiles();
        if(!string.IsNullOrEmpty(secondFiles)) {
            ErrorText += $"Не найдены файлы выборки B: {secondFiles}" + Environment.NewLine;
        }
        string secondFilters = SecondSelection.GetMissedFilters();
        if(!string.IsNullOrEmpty(secondFilters)) {
            ErrorText += $"Не найдены поисковые наборы выборки B: {secondFilters}" + Environment.NewLine;
        }

        if(ClashesConfig.GetClashesConfig(_revitRepository.GetObjectName(), ReportName).Clashes.Count > 0) {
            HasReport = true;
        }
    }

    private void ShowClashes() {
        var view = new NavigatorView() {
            DataContext = new ReportsViewModel(
            _revitRepository,
            _localizationService,
            ReportName) { OpenFromClashDetector = true }
        };
        view.Show();
    }

    private bool CanShowClashes() {
        return HasReport;
    }
}
