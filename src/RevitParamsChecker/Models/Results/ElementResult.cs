using System;
using System.ComponentModel;

using RevitParamsChecker.Models.Revit;

namespace RevitParamsChecker.Models.Results;

internal class ElementResult {
    public ElementResult(ElementModel elementModel, StatusCode status, string error = "") {
        ElementModel = elementModel ?? throw new ArgumentNullException(nameof(elementModel));
        Status = status;
        Error = error;
    }

    public ElementModel ElementModel { get; }

    public StatusCode Status { get; }

    public string Error { get; }
}
