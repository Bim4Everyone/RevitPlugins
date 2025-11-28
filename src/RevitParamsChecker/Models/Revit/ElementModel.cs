using System;

using Autodesk.Revit.DB;

namespace RevitParamsChecker.Models.Revit;

internal class ElementModel {
    public ElementModel(Element element, Reference reference) {
        Element = element ?? throw new ArgumentNullException(nameof(element));
        Reference = reference ?? throw new ArgumentNullException(nameof(reference));
    }

    public Element Element { get; }

    public Reference Reference { get; }
}
