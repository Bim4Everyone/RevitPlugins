using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using dosymep.SimpleServices;

using RevitParamsChecker.Exceptions;
using RevitParamsChecker.Models.Checks;
using RevitParamsChecker.Models.Filtration;
using RevitParamsChecker.Models.Results;
using RevitParamsChecker.Models.Revit;
using RevitParamsChecker.Models.Rules;

namespace RevitParamsChecker.Services;

internal class ChecksEngine {
    private readonly RevitRepository _revitRepo;
    private readonly FiltersRepository _filtersRepo;
    private readonly RulesRepository _rulesRepo;
    private readonly CheckResultsRepository _checkResultsRepo;
    private readonly ILocalizationService _localization;

    public ChecksEngine(
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
                try {
                    bool success = rule.RootRule.Evaluate(element.Element);
                    var status = success ? StatusCode.Valid : StatusCode.Invalid;
                    elementResults.Add(new ElementResult(element, status, rule.Name));
                } catch(ParamNotFoundException exParam) {
                    elementResults.Add(
                        new ElementResult(
                            element,
                            StatusCode.ParamNotFound,
                            _localization.GetLocalizedString("Exceptions.ParamNotFound", exParam.Message)));
                } catch(Autodesk.Revit.Exceptions.ApplicationException exRevit) {
                    elementResults.Add(new ElementResult(element, StatusCode.Error, exRevit.Message));
                }
            }

            ruleResults.Add(new RuleResult(elementResults, rule.Copy()));
        }

        var result = new CheckResult(ruleResults, check.Copy());
        _checkResultsRepo.AddCheckResult(result);
    }

    private ICollection<ElementModel> GetElements(Check check) {
        var filters = check.Filters.Select(f => _filtersRepo.GetFilter(f)).ToArray();
        var docs = check.Files.Select(c => _revitRepo.GetDocument(c)).ToArray();
        List<ElementModel> elements = [];
        foreach(var doc in docs) {
            foreach(var filter in filters) {
                elements.AddRange(_revitRepo.GetElements(doc, filter));
            }
        }

        return elements;
    }
}
