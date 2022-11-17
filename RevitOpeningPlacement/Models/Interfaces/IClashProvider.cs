
using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.OpeningPlacement;

namespace RevitOpeningPlacement.Models.Interfaces {
    internal interface IClashProvider<T1, T2> where T1 : Element where T2 : Element {
        Clash<T1, T2> GetClash(RevitRepository revitRepository, ClashModel model);
    }
}
