using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSleeves.Services.Placing;
internal interface IMepElementsProvider {
    ICollection<Element> GetMepElements(BuiltInCategory category);

    ICollection<ElementId> GetMepElementIds(BuiltInCategory category);
}
