using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Checkers.ClashCheckers;
internal class WallIsStraight : ClashChecker {
    public WallIsStraight(RevitRepository revitRepository, IClashChecker clashChecker) : base(revitRepository, clashChecker) { }
    public override bool CheckModel(ClashModel clashModel) {
        try {
            // Если стена - FaceWall, будет System.InvalidCastException
            var wall = (Wall) clashModel.OtherElement.GetElement(_revitRepository.DocInfos);
            return wall.GetLine() is not null;
        } catch {
            return false;
        }
    }
    public override string GetMessage() {
        return "Стена не является прямой";
    }
}
