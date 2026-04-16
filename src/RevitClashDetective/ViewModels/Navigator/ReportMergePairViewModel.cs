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

    public ReportMergePairViewModel(
        ILocalizationService localization,
        ReportsMergePair reportsMergePair) {
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _reportsMergePair = reportsMergePair ?? throw new ArgumentNullException(nameof(reportsMergePair));

        Items = [];
        if(_reportsMergePair.IntersectionClashes.Conflicted.Any()) {
            Items.Add(
                new ClashMergeCollection(
                _localization.GetLocalizedString("ReportsMerge.ConflictedClashes"),
                _reportsMergePair.IntersectionClashes.Conflicted));
        }

        if(_reportsMergePair.IntersectionClashes.NonConflicted.Any()) {
            Items.Add(
                new ClashMergeCollection(
                _localization.GetLocalizedString("ReportsMerge.AutoMergedClashes"),
                _reportsMergePair.IntersectionClashes.NonConflicted));
        }
        Name = _reportsMergePair.Existing.Name;
    }

    public string Name { get; }

    /// <summary>
    /// Коллекции коллизий, которые надо посмотреть пользователю: конфликты объединения и автообъединенные 
    /// </summary>
    public ObservableCollection<ClashMergeCollection> Items { get; }

    public ReportViewModel GetResultReport() {
        List<ClashViewModel> resultClashes = [
            .._reportsMergePair.ExistingOuterClashes, .._reportsMergePair.ImportingOuterClashes
        ];
        resultClashes.AddRange(_reportsMergePair.IntersectionClashes.Unchanged.Select(c => c.GetResultClash()));
        resultClashes.AddRange(Items.SelectMany(c => c.Items).Select(c => c.GetResultClash()));

        var resultReport = _reportsMergePair.Existing;
        resultReport.ResetClashes(resultClashes);
        return resultReport;
    }
}
