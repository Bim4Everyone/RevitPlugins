using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSleeves.Models.Placing;
internal class ErrorModel {
    private readonly ICollection<Element> _elements;

    public ErrorModel(ICollection<Element> elements, string message) {
        _elements = elements ?? throw new ArgumentNullException(nameof(elements));
        if(_elements.Count == 0) {
            throw new ArgumentOutOfRangeException(nameof(elements));
        }
        if(string.IsNullOrWhiteSpace(message)) {
            throw new ArgumentException(nameof(message));
        }
        Message = message;
    }

    public string Message { get; set; }
}
