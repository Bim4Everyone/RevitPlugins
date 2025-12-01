using System;
using System.Collections.ObjectModel;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitParamsChecker.Models.Results;

namespace RevitParamsChecker.ViewModels.Results;

internal class ResultsPageViewModel : BaseViewModel {
    private readonly ILocalizationService _localization;
    private readonly CheckResultsRepository _checkResultsRepo;

    public ResultsPageViewModel(ILocalizationService localization, CheckResultsRepository checkResultsRepo) {
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _checkResultsRepo = checkResultsRepo ?? throw new ArgumentNullException(nameof(checkResultsRepo));

        CheckResults = []; // TODO
    }

    public ObservableCollection<CheckResultViewModel> CheckResults { get; }
}
