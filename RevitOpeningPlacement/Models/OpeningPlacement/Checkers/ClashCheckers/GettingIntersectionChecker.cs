using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Checkers {
    internal class GettingIntersectionChecker<T1, T2> : ClashChecker where T1 : Element where T2 : Element {
        private readonly IClashProvider<T1, T2> _clashProvider;

        public GettingIntersectionChecker(RevitRepository revitRepository, IClashChecker clashChecker, IClashProvider<T1, T2> clashProvider) : base(revitRepository, clashChecker) {
            _clashProvider = clashProvider;
        }

        public override bool CheckModel(ClashModel clashModel) {
            var clash = _clashProvider.GetClash(_revitRepository, clashModel);
            try {
                var solid = clash.GetIntersection();
            } catch {
                return false;
            }
            return true;
        }
        public override string GetMessage() => "Невозможно получить пересечение элементов.";
    }
}
