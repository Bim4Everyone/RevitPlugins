using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSetLevelSection.Models.Repositories {
    internal interface IElementRepository<T>
        where T : Element {
        List<T> GetElements();
    }
}