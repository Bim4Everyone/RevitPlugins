using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement {
    internal class FittingClashProvider<T> : IClashProvider<FamilyInstance, T> where T : Element {
        public Clash<FamilyInstance, T> GetClash(RevitRepository revitRepository, ClashModel model) {
            return new FittingClash<T>(revitRepository, model);
        }
    }
}
