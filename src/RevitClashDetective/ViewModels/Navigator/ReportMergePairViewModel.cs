using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Services.ReportsMerging;

namespace RevitClashDetective.ViewModels.Navigator;

internal class ReportMergePairViewModel : BaseViewModel {
    private readonly ILocalizationService _localization;
    private readonly ClashMergeCollection _mergeConflictClashes;
    private readonly ClashMergeCollection _autoMergedClashes;
    private readonly ClashViewModel[] _unchangedClashes;
    private readonly ClashViewModel[] _newClashes;
    private ClashMergeViewModel _selectedClashMergeItem;

    public ReportMergePairViewModel(
        ILocalizationService localization,
        ReportsMergePair reportsMergePair) {
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        ExistingReport = existingReport ?? throw new ArgumentNullException(nameof(existingReport));
        ImportingReport = importingReport ?? throw new ArgumentNullException(nameof(importingReport));

        _mergeConflictClashes = new(
            _localization.GetLocalizedString("TODO"),
            FindMergeConflicts(ExistingReport.Clashes, ImportingReport.Clashes));
        _autoMergedClashes = new(_localization.GetLocalizedString("TODO"), []);

        ClashesToMerge = [_mergeConflictClashes, _autoMergedClashes];
    }

    public ReportViewModel ExistingReport { get; }
    public ReportViewModel ImportingReport { get; }
    public ObservableCollection<ClashMergeCollection> ClashesToMerge { get; }

    public ClashMergeViewModel SelectedClashMergeItem {
        get => _selectedClashMergeItem;
        set => RaiseAndSetIfChanged(ref _selectedClashMergeItem, value);
    }

}
