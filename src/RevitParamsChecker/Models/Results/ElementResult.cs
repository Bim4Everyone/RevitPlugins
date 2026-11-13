using System;

using RevitParamsChecker.Models.Revit;

namespace RevitParamsChecker.Models.Results;

internal class ElementResult {
    public ElementResult(ElementModel elementModel, StatusCode status, string ruleName, string info = "") {
        ElementModel = elementModel ?? throw new ArgumentNullException(nameof(elementModel));
        Status = status;
        RuleName = ruleName;
        Info = info;
    }

    public ElementModel ElementModel { get; }

    public StatusCode Status { get; }

    public string Info { get; }

    public string RuleName { get; }
}
