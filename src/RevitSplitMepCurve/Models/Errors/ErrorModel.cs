using System;

using Autodesk.Revit.DB;

namespace RevitSplitMepCurve.Models.Errors;

internal class ErrorModel : IEquatable<ErrorModel> {
    private readonly ElementId _id;

    public ErrorModel(Element element, string message) {
        if(string.IsNullOrWhiteSpace(message)) {
            throw new ArgumentException(nameof(message));
        }
        Message = message;
        Element = element ?? throw new ArgumentNullException(nameof(element));
        _id = Element.Id;
    }

    public string Message { get; }

    public Element Element { get; }

    public bool Equals(ErrorModel other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return Equals(_id, other._id);
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

        return Equals((ErrorModel) obj);
    }

    public override int GetHashCode() {
        return _id.GetHashCode();
    }
}
