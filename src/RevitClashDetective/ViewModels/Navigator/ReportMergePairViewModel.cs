using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Services.ReportsMerging;

namespace RevitClashDetective.ViewModels.Navigator;

internal class ReportMergePairViewModel : BaseViewModel {
    private readonly ILocalizationService _localization;
    private readonly ReportsMergePair _reportsMergePair;
    private ClashMergePairViewModel _selectedClashMergePairItem;

    public ReportMergePairViewModel(
        ILocalizationService localization,
        ReportsMergePair reportsMergePair) {
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _reportsMergePair = reportsMergePair ?? throw new ArgumentNullException(nameof(reportsMergePair));

        ClashCollections = [
            new ClashMergeCollection(
                _localization.GetLocalizedString("TODO"),
                _reportsMergePair.IntersectionClashes.Conflicted),
            new ClashMergeCollection(
                _localization.GetLocalizedString("TODO"),
                _reportsMergePair.IntersectionClashes.NonConflicted)
        ];
    }

    public ObservableCollection<ClashMergeCollection> ClashCollections { get; }

    public ClashMergePairViewModel SelectedClashMergePairItem {
        get => _selectedClashMergePairItem;
        set => RaiseAndSetIfChanged(ref _selectedClashMergePairItem, value);
    }

    public ReportViewModel GetResultReport() {
        List<ClashViewModel> resultClashes = [
            .._reportsMergePair.ExistingOuterClashes, .._reportsMergePair.ImportingOuterClashes
        ];
        resultClashes.AddRange(_reportsMergePair.IntersectionClashes.Unchanged.Select(c => c.GetResultClash()));
        resultClashes.AddRange(ClashCollections.SelectMany(c => c.Items).Select(c => c.GetResultClash()));

        var resultReport = _reportsMergePair.Existing;
        resultReport.ResetClashes(resultClashes);
        return resultReport;
    }
}
