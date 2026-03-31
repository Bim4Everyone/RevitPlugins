using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitMarkAllDocuments.Models;

internal class MarkedElement {
    public MarkedElement(Element element) {
        Id = element.Id;
        Document = "doc";
        MarkValue = "value";
    }

    public ElementId Id { get; set; }
    public string Document { get; set; }
    public string MarkValue { get; set; }
}
