using dosymep.SimpleServices;

using RevitParamsChecker.Exceptions;
using RevitParamsChecker.Models.Checks;
using RevitParamsChecker.Models.Filtration;
using RevitParamsChecker.Models.Results;
using RevitParamsChecker.Models.Revit;
using RevitParamsChecker.Models.Rules;

namespace RevitParamsChecker.Services;

internal class ElementChecksEngine : ChecksEngine {
    public ElementChecksEngine(
        RevitRepository revitRepo,
        FiltersRepository filtersRepo,
        RulesRepository rulesRepo,
        CheckResultsRepository checkResultsRepo,
        ILocalizationService localization)
        : base(revitRepo, filtersRepo, rulesRepo, checkResultsRepo, localization) {
    }

    public override CheckTargetType TargetType => CheckTargetType.Element;

    protected override ElementResult EvaluateElement(ElementModel element, Rule rule) {
        try {
            bool success = rule.RootRule.Evaluate(element.Element);
            var status = success ? StatusCode.Valid : StatusCode.Invalid;
            return new ElementResult(element, status, rule.Name);
        } catch(ParamNotFoundException exParam) {
            return new ElementResult(
                element,
                StatusCode.ParamNotFound,
                rule.Name,
                _localization.GetLocalizedString("Exceptions.ParamNotFound", exParam.Message));
        } catch(Autodesk.Revit.Exceptions.ApplicationException exRevit) {
            return new ElementResult(element, StatusCode.Error, rule.Name, exRevit.Message);
        }
    }
}
