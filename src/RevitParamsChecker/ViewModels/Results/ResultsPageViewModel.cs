using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitParamsChecker.Models.Results;

namespace RevitParamsChecker.ViewModels.Results;

internal class ResultsPageViewModel : BaseViewModel {
    private readonly ILocalizationService _localization;
    private readonly CheckResultsRepository _checkResultsRepo;
    private CheckResultViewModel _selectedCheckResult;

    public ResultsPageViewModel(ILocalizationService localization, CheckResultsRepository checkResultsRepo) {
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _checkResultsRepo = checkResultsRepo ?? throw new ArgumentNullException(nameof(checkResultsRepo));

        CheckResults = [new CheckResultViewModel()]; // TODO
        RemoveCheckResultsCommand = RelayCommand.Create<IList>(RemoveCheckResults, CanRemoveChecksResults);
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
        }
        // TODO
    }

    private bool CanRemoveChecksResults(IList items) {
        return items != null && items.OfType<CheckResultViewModel>().Count() != 0;
    }
}
