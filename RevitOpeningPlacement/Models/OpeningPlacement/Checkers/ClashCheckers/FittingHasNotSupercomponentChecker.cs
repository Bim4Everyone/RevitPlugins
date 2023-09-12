using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Checkers {
    internal class FittingHasNotSupercomponentChecker : ClashChecker {
        public FittingHasNotSupercomponentChecker(RevitRepository revitRepository, IClashChecker clashChecker) : base(revitRepository, clashChecker) { }
        public override bool CheckModel(ClashModel clashModel) {
            var fitting = (FamilyInstance) clashModel.MainElement.GetElement(_revitRepository.DocInfos);
            return fitting.SuperComponent == null;
        }
        public override string GetMessage() => RevitRepository.SystemCheck;
        //public override string GetMessage() => nameof(FittingHasNotSupercomponentChecker);
    }
}
