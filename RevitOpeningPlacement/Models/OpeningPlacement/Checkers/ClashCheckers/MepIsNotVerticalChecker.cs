using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Checkers {
    internal class MepIsNotVerticalChecker : ClashChecker {
        public MepIsNotVerticalChecker(RevitRepository revitRepository, IClashChecker clashChecker) : base(revitRepository, clashChecker) { }
        public override bool CheckModel(ClashModel clashModel) {
            return !((MEPCurve) clashModel.MainElement.GetElement(_revitRepository.DocInfos)).IsVertical();
        }

        public override string GetMessage() => "Задание на отверстие в стене: Инженерная система расположена вертикально.";
    }
}
