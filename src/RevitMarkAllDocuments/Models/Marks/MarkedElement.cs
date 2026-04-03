using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitMarkAllDocuments.Models;

internal class MarkedElement {
    private readonly Element _element;
    private readonly ElementId _id;

    public MarkedElement(Element element) {
        _element = element;
        _id = element.Id;
    }

    public Element Element => _element;
    public ElementId Id => _id;
    public string MarkValue { get; set; }
}
