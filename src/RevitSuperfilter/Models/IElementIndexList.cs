using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSuperfilter.Models;

internal interface IElementIndexList {
    void Add(Element element);
    void Remove(ElementId elementId);
}
