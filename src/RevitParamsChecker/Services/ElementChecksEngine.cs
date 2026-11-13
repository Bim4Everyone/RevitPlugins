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
            var result = rule.RootRule.Evaluate(element.Element);
            return result.Success
                ? new ElementResult(element, StatusCode.Valid, rule.Name)
                : new ElementResult(element, StatusCode.Invalid, rule.Name, FormatFailures(result.Failures));
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
