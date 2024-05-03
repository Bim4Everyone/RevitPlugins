using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement {
    internal class MepCurveClashProvider<T> : IClashProvider<MEPCurve, T> where T : Element {
        public Clash<MEPCurve, T> GetClash(RevitRepository revitRepository, ClashModel model) {
            return new MepCurveClash<T>(revitRepository, model);
        }
    }
}
