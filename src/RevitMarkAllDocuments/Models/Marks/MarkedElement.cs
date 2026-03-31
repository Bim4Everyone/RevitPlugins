using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitMarkAllDocuments.Models;

internal class MarkedElement {
    private readonly ElementId _id;
    private readonly string _document;

    public MarkedElement(Element element, string document) {
        _id = element.Id;
        _document = document;
    }

    public ElementId Id => _id;
    public string Document => _document;
    public string MarkValue { get; set; }
}
