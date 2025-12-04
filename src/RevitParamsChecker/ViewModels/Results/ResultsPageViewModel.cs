using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitParamsChecker.Models.Results;
using RevitParamsChecker.Models.Revit;

namespace RevitParamsChecker.ViewModels.Results;

internal class ResultsPageViewModel : BaseViewModel {
    private readonly ILocalizationService _localization;
    private readonly CheckResultsRepository _checkResultsRepo;
    private readonly RevitRepository _revitRepo;
    private CheckResultViewModel _selectedCheckResult;

    public ResultsPageViewModel(
        ILocalizationService localization,
        CheckResultsRepository checkResultsRepo,
        RevitRepository revitRepo) {
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _checkResultsRepo = checkResultsRepo ?? throw new ArgumentNullException(nameof(checkResultsRepo));
        _revitRepo = revitRepo ?? throw new ArgumentNullException(nameof(revitRepo));

        CheckResults = [
            .._checkResultsRepo.GetCheckResults().Select(c => new CheckResultViewModel(_localization, c, _revitRepo))
        ];
        SelectedCheckResult = CheckResults.FirstOrDefault();
        RemoveCheckResultsCommand = RelayCommand.Create<IList>(RemoveCheckResults, CanRemoveChecksResults);
        _checkResultsRepo.ResultsAdded += ResultsAddedHandler;
    }

    public ICommand RemoveCheckResultsCommand { get; }

    public ObservableCollection<CheckResultViewModel> CheckResults { get; }

    public CheckResultViewModel SelectedCheckResult {
        get => _selectedCheckResult;
        set => RaiseAndSetIfChanged(ref _selectedCheckResult, value);
    }

    private void RemoveCheckResults(IList items) {
        var checkResults = items.OfType<CheckResultViewModel>().ToArray();
        foreach(var checkResult in checkResults) {
            CheckResults.Remove(checkResult);
            _checkResultsRepo.RemoveCheckResult(checkResult.CheckResult);
        }
    }

    private bool CanRemoveChecksResults(IList items) {
        return items != null && items.OfType<CheckResultViewModel>().Count() != 0;
    }

    private void ResultsAddedHandler(object sender, ResultsChangedEventArgs e) {
        CheckResults.Clear();
        foreach(var result in e.NewCheckResults) {
            CheckResults.Add(new CheckResultViewModel(_localization, result, _revitRepo));
        }

        SelectedCheckResult = CheckResults.FirstOrDefault();
    }
}
