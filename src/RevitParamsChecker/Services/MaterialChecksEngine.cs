using System;
using System.Collections.Generic;

using dosymep.SimpleServices;

using RevitParamsChecker.Exceptions;
using RevitParamsChecker.Models.Checks;
using RevitParamsChecker.Models.Filtration;
using RevitParamsChecker.Models.Results;
using RevitParamsChecker.Models.Revit;
using RevitParamsChecker.Models.Rules;

namespace RevitParamsChecker.Services;

internal class MaterialChecksEngine : ChecksEngine {
    public MaterialChecksEngine(
        RevitRepository revitRepo,
        FiltersRepository filtersRepo,
        RulesRepository rulesRepo,
        CheckResultsRepository checkResultsRepo,
        ILocalizationService localization)
        : base(revitRepo, filtersRepo, rulesRepo, checkResultsRepo, localization) {
    }

    public override CheckTargetType TargetType => CheckTargetType.Material;

    protected override ElementResult EvaluateElement(ElementModel element, Rule rule) {
        var materials = _revitRepo.GetElementMaterials(element.Element);
        if(materials.Count == 0) {
            return new ElementResult(
                element,
                StatusCode.ParamNotFound,
                rule.Name,
                _localization.GetLocalizedString("Exceptions.NoMaterials"));
        }

        StatusCode status = StatusCode.Valid;
        string error = string.Empty;
        foreach(var material in materials) {
            try {
                bool success = rule.RootRule.Evaluate(material);
                if(!success) {
                    status = StatusCode.Invalid;
                    break;
                }
            } catch(ParamNotFoundException exParam) {
                status = StatusCode.ParamNotFound;
                error = _localization.GetLocalizedString("Exceptions.MaterialParamNotFound", exParam.Message);
                break;
            } catch(Autodesk.Revit.Exceptions.ApplicationException exRevit) {
                status = StatusCode.Error;
                error = exRevit.Message;
                break;
            }
        }

        return new ElementResult(element, status, rule.Name, error);
    }
}
