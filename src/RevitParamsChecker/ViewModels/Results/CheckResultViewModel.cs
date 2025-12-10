using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitParamsChecker.Models.Results;
using RevitParamsChecker.Models.Revit;
using RevitParamsChecker.Services;
using RevitParamsChecker.ViewModels.Rules;

namespace RevitParamsChecker.ViewModels.Results;

internal class CheckResultViewModel : BaseViewModel {
    private readonly ILocalizationService _localization;
    private readonly DelayAction _refreshViewDelay;
    private readonly RevitRepository _revitRepo;
    private readonly ObservableCollection<ElementResultViewModel> _allElementResults;
    private string _elementsFilter;

    public CheckResultViewModel(ILocalizationService localization, CheckResult checkResult, RevitRepository revitRepo) {
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        _revitRepo = revitRepo ?? throw new ArgumentNullException(nameof(revitRepo));
        CheckResult = checkResult ?? throw new ArgumentNullException(nameof(checkResult));
        Name = CheckResult.CheckCopy.Name;
        RulesStamp = GetRulesStamp(CheckResult, _localization);
        _allElementResults = GetElementResults(CheckResult, _localization);
        ElementResults = new CollectionViewSource() { Source = _allElementResults };
        ElementResults.Filter += ElementResultsFilterHandler;
        SelectElementsCommand = RelayCommand.Create<IList>(SelectElements, CanSelectElements);
        _refreshViewDelay = new DelayAction(
            300,
            () => Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Background,
                () => ElementResults.View.Refresh())
        );
        SetGrouping(ElementResults);
        PropertyChanged += ElementsFilterPropertyChanged;
    }

    public ICommand SelectElementsCommand { get; }
    public string Name { get; }
    public CheckResult CheckResult { get; }
    public IReadOnlyCollection<RuleViewModel> RulesStamp { get; }
    public CollectionViewSource ElementResults { get; }

    /// <summary>
    /// Фильтр для таблицы с элементами в ui
    /// </summary>
    public string ElementsFilter {
        get => _elementsFilter;
        set => RaiseAndSetIfChanged(ref _elementsFilter, value);
    }

    private void SelectElements(IList items) {
        var elements = items.OfType<ElementResultViewModel>()
            .Select(vm => vm.ElementResult.ElementModel)
            .ToArray();
        _revitRepo.SelectElements(elements);
    }

    private bool CanSelectElements(IList items) {
        return items != null && items.OfType<ElementResultViewModel>().Count() != 0;
    }

    private void ElementsFilterPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(ElementsFilter)) {
            _refreshViewDelay.Action();
        }
    }

    private void ElementResultsFilterHandler(object sender, FilterEventArgs e) {
        string filter = ElementsFilter?.Trim() ?? string.Empty;
        if(string.IsNullOrWhiteSpace(filter)) {
            e.Accepted = true;
            return;
        }

        if(e.Item is ElementResultViewModel result) {
            e.Accepted = result.FileName?.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                         || result.FamilyTypeName?.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                         || result.RuleName?.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                         || result.Status?.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                         || result.Error?.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                         || result.UserMark?.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }

    private PropertyGroupDescription[] GetGroupDescriptions() {
        return [
            new PropertyGroupDescription(
                nameof(ElementResultViewModel.ChunkName)),
            new PropertyGroupDescription(
                nameof(ElementResultViewModel.Status)),
            new PropertyGroupDescription(
                nameof(ElementResultViewModel.FileName)),
            new PropertyGroupDescription(
                nameof(ElementResultViewModel.RuleName))
        ];
    }

    private void SetGrouping(CollectionViewSource elementResults) {
        ElementResults.GroupDescriptions.Clear();
        foreach(var p in GetGroupDescriptions()) {
            ElementResults.GroupDescriptions.Add(p);
        }
    }

    private ObservableCollection<ElementResultViewModel> GetElementResults(
        CheckResult checkResult,
        ILocalizationService localization) {
        ElementResultViewModel[] elements = checkResult.RuleResults
            .SelectMany(res => res.ElementResults)
            .Select(e => new ElementResultViewModel(localization, e))
            .OrderBy(e => e.ElementResult.Status)
            .ThenBy(e => e.FileName)
            .ThenBy(e => e.RuleName)
            .ThenBy(e => e.FamilyTypeName)
            .ToArray();
        const int chunkSize = 500;
        for(int i = 0; i < elements.Length; i++) {
            int groupStart = (i / chunkSize) * chunkSize + 1;
            int groupEnd = Math.Min(groupStart + chunkSize - 1, elements.Length);
            elements[i].ItemNumber = i + 1;
            elements[i].ChunkName = $"[{groupStart}...{groupEnd}]";
        }

        return new ObservableCollection<ElementResultViewModel>(elements);
    }

    private ReadOnlyCollection<RuleViewModel> GetRulesStamp(
        CheckResult checkResult,
        ILocalizationService localization) {
        return new ReadOnlyCollection<RuleViewModel>(
            checkResult.RuleResults
                .Select(r => new RuleViewModel(r.RuleCopy, localization))
                .ToArray()
        );
    }
}
