using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Services.ReportsMerging;

namespace RevitClashDetective.ViewModels.Navigator;

internal class ReportsMergeViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localization;
    private readonly ReportSetsIntersectionResult _reportsMerging;
    private readonly ReportMergePairViewModel[] _allMergePairs;
    private ClashMergePairViewModel _selectedClashMergePairItem;

    public ReportsMergeViewModel(
        RevitRepository revitRepository,
        ILocalizationService localization,
        ReportSetsIntersectionResult reportsMerging
    ) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _reportsMerging = reportsMerging ?? throw new ArgumentNullException(nameof(reportsMerging));

        _allMergePairs = [
            .._reportsMerging.GetMergePairs().Select(p => new ReportMergePairViewModel(_localization, p))
        ];
        VisibleReports = [.._allMergePairs.Where(p => p.ClashesCount > 0)];
        AcceptMergeCommand = RelayCommand.Create(() => { }, CanAcceptMerge);
        PickElementCommand = RelayCommand.Create<ElementModel>(PickElement, CanPickElement);
    }

    public ICommand AcceptMergeCommand { get; }

    public ICommand PickElementCommand { get; }

    public ICollection<ReportMergePairViewModel> VisibleReports { get; }

    public ClashMergePairViewModel SelectedClashMergePairItem {
        get => _selectedClashMergePairItem;
        set => RaiseAndSetIfChanged(ref _selectedClashMergePairItem, value);
    }

    public ICollection<ReportViewModel> GetMergeResult() {
        if(!CanAcceptMerge()) {
            throw new InvalidOperationException();
        }

        List<ReportViewModel> result = [
            .._reportsMerging.LeftOuterSet.Reports, .. _reportsMerging.RightOuterSet.Reports
        ];
        result.AddRange(_allMergePairs.Select(p => p.GetResultReport()));
        return result;
    }

    private bool CanAcceptMerge() {
        return true;
    }

    private void PickElement(ElementModel element) {
        _revitRepository.SelectElements([element]);
    }

    private bool CanPickElement(ElementModel element) {
        return element is not null;
    }
}
