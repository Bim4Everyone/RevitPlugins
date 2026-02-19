using System;

using Autodesk.Revit.DB;

namespace RevitParamsChecker.Models.Revit;

internal class ElementModel : IEquatable<ElementModel> {
    private readonly ElementId _id;
    private readonly string _docTitle;

    public ElementModel(Element element, Reference reference) {
        Element = element ?? throw new ArgumentNullException(nameof(element));
        Reference = reference ?? throw new ArgumentNullException(nameof(reference));
        _id = element.Id;
        _docTitle = element.Document.Title;
    }

    public Element Element { get; }

    public Reference Reference { get; }

    public bool Equals(ElementModel other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return Equals(_id, other._id) && _docTitle == other._docTitle;
    }

    public override bool Equals(object obj) {
        if(obj is null) {
            return false;
        }

        if(ReferenceEquals(this, obj)) {
            return true;
        }

        if(obj.GetType() != GetType()) {
            return false;
        }

        return Equals((ElementModel) obj);
    }

    public override int GetHashCode() {
        unchecked {
            return ((_id != null ? _id.GetHashCode() : 0) * 397) ^ (_docTitle != null ? _docTitle.GetHashCode() : 0);
        }
    }
}
