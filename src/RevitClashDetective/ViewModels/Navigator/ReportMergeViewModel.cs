using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

namespace RevitClashDetective.ViewModels.Navigator;

internal class ReportMergeViewModel : BaseViewModel {
    private readonly ILocalizationService _localization;
    private readonly ClashMergeCollection _mergeConflictClashes;
    private readonly ClashMergeCollection _autoMergedClashes;
    private readonly ClashViewModel[] _unchangedClashes;
    private readonly ClashViewModel[] _newClashes;
    private ClashMergeViewModel _selectedClashMergeItem;

    public ReportMergeViewModel(
        ILocalizationService localization,
        ReportViewModel existingReport,
        ReportViewModel newReport) {
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        ExistingReport = existingReport ?? throw new ArgumentNullException(nameof(existingReport));
        NewReport = newReport ?? throw new ArgumentNullException(nameof(newReport));

        _mergeConflictClashes = new(
            _localization.GetLocalizedString("TODO"),
            FindMergeConflicts(ExistingReport.Clashes, NewReport.Clashes));
        _autoMergedClashes = new(_localization.GetLocalizedString("TODO"), []);

        Items = new ReadOnlyCollection<ClashMergeCollection>([_mergeConflictClashes, _autoMergedClashes]);
    }

    public ReportViewModel ExistingReport { get; }
    public ReportViewModel NewReport { get; }
    public IReadOnlyCollection<ClashMergeCollection> Items { get; }

    public ClashMergeViewModel SelectedClashMergeItem {
        get => _selectedClashMergeItem;
        set => RaiseAndSetIfChanged(ref _selectedClashMergeItem, value);
    }

    private IList<ClashMergeViewModel> FindMergeConflicts(
        IList<ClashViewModel> existingClashes,
        IList<ClashViewModel> newClashes) {
        // TODO
        return [];
    }
}
