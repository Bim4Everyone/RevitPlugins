using System;

using Autodesk.Revit.DB;

namespace RevitSplitMepCurve.Models.Errors;

internal class ErrorModel {
    public ErrorModel(MEPCurve element, string message) {
        if(string.IsNullOrWhiteSpace(message)) {
            throw new ArgumentException(nameof(message));
        }
        Element = element;
        Message = message;
    }

    public string Message { get; }

    public MEPCurve Element { get; }
}
