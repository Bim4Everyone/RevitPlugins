using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSuperfilter.Models;

internal interface IElementIndex {
    void Add(Element element);
    void Remove(ElementId elementId);
}
