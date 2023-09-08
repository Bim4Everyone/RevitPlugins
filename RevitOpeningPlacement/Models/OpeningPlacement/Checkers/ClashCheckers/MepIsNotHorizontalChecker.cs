using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Checkers {
    internal class MepIsNotHorizontalChecker : ClashChecker {
        public MepIsNotHorizontalChecker(RevitRepository revitRepository, IClashChecker clashChecker) : base(revitRepository, clashChecker) { }
        public override bool CheckModel(ClashModel clashModel) {
            return !((MEPCurve) clashModel.MainElement.GetElement(_revitRepository.DocInfos)).IsHorizontal();
        }
        //public override string GetMessage() => "Задание на отверстие в перекрытии: Инженерная система расположена горизонтально.";
        public override string GetMessage() => RevitRepository.SystemCheck;
    }
}
