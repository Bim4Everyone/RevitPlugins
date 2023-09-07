using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Checkers.ClashCheckers {
    internal class MainElementIsFittingChecker : ClashChecker {
        public MainElementIsFittingChecker(RevitRepository revitRepository, IClashChecker clashChecker) : base(revitRepository, clashChecker) { }
        public override bool CheckModel(ClashModel clashModel) {
            return clashModel.MainElement.GetElement(_revitRepository.DocInfos) is FamilyInstance;
        }

        public override string GetMessage() => "Элемент не является соединительной арматурой.";
    }
}
