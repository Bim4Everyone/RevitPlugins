using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Checkers {
    internal class ElementsIsNotParallelChecker : ClashChecker {
        public ElementsIsNotParallelChecker(RevitRepository revitRepository, IClashChecker clashChecker) : base(revitRepository, clashChecker) { }
        public override bool CheckModel(ClashModel clashModel) {
            var clash = new MepCurveClash<Wall>(_revitRepository, clashModel);
            var curve = clash.GetTransformedMepLine();
            return !curve.IsParallel(clash.Element2.GetLine()) && !curve.RunAlongWall(clash.Element2);
        }
        public override string GetMessage() => "Задание на отверстие в стене: Инженерная система расположена параллельно стене.";
    }
}
