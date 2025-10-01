using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Checkers.ClashCheckers;
internal class FittingAndWallAreNotParallelChecker : ClashChecker {
    public FittingAndWallAreNotParallelChecker(RevitRepository revitRepository, IClashChecker clashChecker) : base(revitRepository, clashChecker) { }

    public override bool CheckModel(ClashModel clashModel) {
        var clash = new FittingClash<Wall>(_revitRepository, clashModel);
        var connectorLines = clash.Element1.GetConnectionLines();
        return connectorLines == null || connectorLines.Select(item => clash.Element2Transform.Inverse.Multiply(Transform.Identity).OfVector(item))
                             .Any(item => !item.RunAlongWall(clash.Element2));
    }
    public override string GetMessage() {
        return "Задание на отверстие в стене: Соединительная арматура расположена параллельно стене.";
    }
}
