using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Checkers {
    internal class OtherElementIsFloorChecker : ClashChecker {
        public OtherElementIsFloorChecker(RevitRepository revitRepository, IClashChecker clashChecker) : base(revitRepository, clashChecker) { }
        public override bool CheckModel(ClashModel clashModel) {
            return clashModel.OtherElement.GetElement(_revitRepository.DocInfos) is CeilingAndFloor;
        }
        public override string GetMessage() => "Задание на отверстие в перекрытии: Нет элемента перекрытия.";
    }
}
