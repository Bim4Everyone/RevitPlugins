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

        StatusCode worst = StatusCode.Valid;
        var errors = new HashSet<string>(StringComparer.Ordinal);

        foreach(var material in materials) {
            try {
                bool success = rule.RootRule.Evaluate(material);
                var status = success ? StatusCode.Valid : StatusCode.Invalid;
                if((int) status < (int) worst) {
                    worst = status;
                }
            } catch(ParamNotFoundException exParam) {
                if((int) StatusCode.ParamNotFound < (int) worst) {
                    worst = StatusCode.ParamNotFound;
                }

                errors.Add(_localization.GetLocalizedString("Exceptions.ParamNotFound", exParam.Message));
            } catch(Autodesk.Revit.Exceptions.ApplicationException exRevit) {
                worst = StatusCode.Error;
                if(!string.IsNullOrEmpty(exRevit.Message)) {
                    errors.Add(exRevit.Message);
                }
            }
        }

        string error = errors.Count > 0 ? string.Join("; ", errors) : string.Empty;
        return new ElementResult(element, worst, rule.Name, error);
    }
}
