using System;

using RevitParamsChecker.Models.Revit;

namespace RevitParamsChecker.Models.Results;

internal class ElementResult {
    public ElementResult(ElementModel elementModel, StatusCode status, string ruleName, string error = "") {
        ElementModel = elementModel ?? throw new ArgumentNullException(nameof(elementModel));
        Status = status;
        RuleName = ruleName;
        Error = error;
    }

    public ElementModel ElementModel { get; }

    public StatusCode Status { get; }

    public string Error { get; }

    public string RuleName { get; }
}
