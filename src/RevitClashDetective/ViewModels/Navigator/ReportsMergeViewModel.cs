using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Services.ReportsMerging;

namespace RevitClashDetective.ViewModels.Navigator;

internal class ReportsMergeViewModel : BaseViewModel {
    private readonly ILocalizationService _localization;
    private readonly ReportSetsIntersectionResult _reportsIntersection;
    private string _errorText;

    public ReportsMergeViewModel(
        ILocalizationService localization,
        ReportSetsIntersectionResult reportsIntersection
    ) {
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _reportsIntersection = reportsIntersection ?? throw new ArgumentNullException(nameof(reportsIntersection));

        ReportsToMerge = [
            .._reportsIntersection.GetMergePairs().Select(p => new ReportMergePairViewModel(_localization, p))
        ];
        AcceptMergeCommand = RelayCommand.Create(() => { }, CanAcceptMerge);
    }

    public ICommand AcceptMergeCommand { get; }

    public ObservableCollection<ReportMergePairViewModel> ReportsToMerge { get; }

    public string ErrorText {
        get => _errorText;
        private set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public ICollection<ReportViewModel> GetMergeResult() {
        if(!CanAcceptMerge()) {
            throw new InvalidOperationException();
        }

        throw new NotImplementedException();
    }

    private bool CanAcceptMerge() {
        // TODO
        return false;
    }
}
