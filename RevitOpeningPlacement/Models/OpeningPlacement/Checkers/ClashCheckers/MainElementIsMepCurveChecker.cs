using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Checkers {
    internal class MainElementIsMepCurveChecker : ClashChecker {
        public MainElementIsMepCurveChecker(RevitRepository revitRepository, IClashChecker clashChecker) : base(revitRepository, clashChecker) { }
        public override bool CheckModel(ClashModel clashModel) {
            return clashModel.MainElement.GetElement(_revitRepository.DocInfos) is MEPCurve;
        }

        public override string GetMessage() => "Нет элемента инженерной системы.";
    }
}
