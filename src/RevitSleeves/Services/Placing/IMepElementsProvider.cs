using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSleeves.Services.Placing;
internal interface IMepElementsProvider {
    ICollection<Element> GetMepElements();
}
