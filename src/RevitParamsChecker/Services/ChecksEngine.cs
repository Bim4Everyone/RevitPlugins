using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using dosymep.SimpleServices;

using RevitParamsChecker.Models.Checks;
using RevitParamsChecker.Models.Filtration;
using RevitParamsChecker.Models.Results;
using RevitParamsChecker.Models.Revit;
using RevitParamsChecker.Models.Rules;

namespace RevitParamsChecker.Services;

internal abstract class ChecksEngine {
    protected readonly RevitRepository _revitRepo;
    protected readonly FiltersRepository _filtersRepo;
    protected readonly RulesRepository _rulesRepo;
    protected readonly CheckResultsRepository _checkResultsRepo;
    protected readonly ILocalizationService _localization;

    protected ChecksEngine(
        RevitRepository revitRepo,
        FiltersRepository filtersRepo,
        RulesRepository rulesRepo,
        CheckResultsRepository checkResultsRepo,
        ILocalizationService localization) {
        _revitRepo = revitRepo ?? throw new ArgumentNullException(nameof(revitRepo));
        _filtersRepo = filtersRepo ?? throw new ArgumentNullException(nameof(filtersRepo));
        _rulesRepo = rulesRepo ?? throw new ArgumentNullException(nameof(rulesRepo));
        _checkResultsRepo = checkResultsRepo ?? throw new ArgumentNullException(nameof(checkResultsRepo));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
    }

    public abstract CheckTargetType TargetType { get; }

    public void Run(Check check, CancellationToken ct = default) {
        if(check == null) {
            throw new ArgumentNullException(nameof(check));
        }

        var elements = GetElements(check);
        var rules = check.Rules.Select(r => _rulesRepo.GetRule(r)).ToArray();
        List<RuleResult> ruleResults = [];
        foreach(var rule in rules) {
            List<ElementResult> elementResults = [];
            foreach(var element in elements) {
                ct.ThrowIfCancellationRequested();
                elementResults.Add(EvaluateElement(element, rule));
            }

            ruleResults.Add(new RuleResult(elementResults, rule.Copy()));
        }

        var result = new CheckResult(ruleResults, check.Copy());
        _checkResultsRepo.AddCheckResult(result);
    }

    protected abstract ElementResult EvaluateElement(ElementModel element, Rule rule);

    private ICollection<ElementModel> GetElements(Check check) {
        var filters = check.Filters.Select(f => _filtersRepo.GetFilter(f)).ToArray();
        var docs = check.Files.Select(c => _revitRepo.GetDocument(c)).ToArray();
        HashSet<ElementModel> elements = []; // фильтры могут пересекаться и надо избежать дублей элементов
        foreach(var doc in docs) {
            foreach(var filter in filters) {
                foreach(var el in _revitRepo.GetElements(doc, filter)) {
                    elements.Add(el);
                }
            }
        }

        return elements;
    }
}
