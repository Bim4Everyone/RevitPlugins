using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Checkers {
    internal class WallIsNotCurtainChecker : ClashChecker {
        public WallIsNotCurtainChecker(RevitRepository revitRepository, IClashChecker clashChecker) : base(revitRepository, clashChecker) { }
        public override bool CheckModel(ClashModel clashModel) {
            // Если стена - FaceWall, будет System.InvalidCastException
            return (clashModel.OtherElement.GetElement(_revitRepository.DocInfos) is Wall wall) && (wall.WallType.Kind != WallKind.Curtain);
        }
        public override string GetMessage() => "Задание на отверстие в стене: Стена относится к витражной системе.";
    }
}
