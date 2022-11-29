
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.OpeningUnion;

namespace RevitOpeningPlacement.Models.Interfaces {
    internal interface IIntersectionProvider {
        List<FamilyInstanceWrapper> GetIntersectedElements(OpeningsGroup group, IList<FamilyInstanceWrapper> elements);
    }
}
