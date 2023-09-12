using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Checkers {
    internal class OtherElementIsWallChecker : ClashChecker {
        public OtherElementIsWallChecker(RevitRepository revitRepository, IClashChecker clashChecker) : base(revitRepository, clashChecker) { }
        public override bool CheckModel(ClashModel clashModel) {
            return clashModel.OtherElement.GetElement(_revitRepository.DocInfos) is Wall;
        }

        public override string GetMessage() => "Задание на отверстие в стене: Нет элемента стены.";
    }
}
