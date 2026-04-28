using System;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitSplitMepCurve.Models.Errors;

namespace RevitSplitMepCurve.ViewModels.Errors;

internal class ErrorViewModel : BaseViewModel {
    private readonly ErrorModel _error;

    public ErrorViewModel(ErrorModel error) {
        _error = error ?? throw new ArgumentNullException(nameof(error));
        ElementName = _error.Element?.Name ?? "—";
    }

    public string Message => _error.Message;

    public string ElementName { get; }

    public MEPCurve Element => _error.Element;
}
