using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitClashDetective.Models {
    internal interface IProvider {
        List<Element> GetElements(Document doc);
    }
}
