using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSleeves.Models;
internal class ErrorModel {
    private readonly ICollection<Element> _elements;

    public ErrorModel(ICollection<Element> elements, string message) {
        _elements = elements ?? Array.Empty<Element>();
        if(string.IsNullOrWhiteSpace(message)) {
            throw new ArgumentException(nameof(message));
        }
        Message = message;
    }

    public string Message { get; set; }

    public ICollection<Element> GetDependentElements() {
        return _elements;
    }
}
