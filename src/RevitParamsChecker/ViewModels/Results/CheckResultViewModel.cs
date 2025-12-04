using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitParamsChecker.Models.Results;
using RevitParamsChecker.ViewModels.Rules;

namespace RevitParamsChecker.ViewModels.Results;

internal class CheckResultViewModel : BaseViewModel {
    private readonly ILocalizationService _localization;
    private string _elementsFilter;

    public CheckResultViewModel(ILocalizationService localization, CheckResult checkResult) {
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        CheckResult = checkResult ?? throw new ArgumentNullException(nameof(checkResult));
        Name = CheckResult.CheckCopy.Name;
        ElementResults = new ReadOnlyCollection<ElementResultViewModel>(
            CheckResult.RuleResults.SelectMany(res => res.ElementResults)
                .Select(e => new ElementResultViewModel(_localization, e))
                .ToArray()
        );
        RulesStamp = new ReadOnlyCollection<RuleViewModel>(
            CheckResult.RuleResults
                .Select(r => new RuleViewModel(r.RuleCopy, _localization))
                .ToArray()
        );
        SelectElementsCommand = RelayCommand.Create<IList>(SelectElements, CanSelectElements);
    }

    public ICommand SelectElementsCommand { get; }

    public string Name { get; }

    public CheckResult CheckResult { get; }

    public IReadOnlyCollection<RuleViewModel> RulesStamp { get; }

    public IReadOnlyCollection<ElementResultViewModel> ElementResults { get; }

    /// <summary>
    /// Фильтр для таблицы с элементами в ui
    /// </summary>
    public string ElementsFilter {
        get => _elementsFilter;
        set => RaiseAndSetIfChanged(ref _elementsFilter, value);
    }

    private void SelectElements(IList items) {
        // TODO
    }

    private bool CanSelectElements(IList items) {
        return items != null && items.OfType<ElementResultViewModel>().Count() != 0;
    }
}
