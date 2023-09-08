using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Checkers {
    internal class ClashVolumeChecker<T1, T2> : ClashChecker where T1 : Element where T2 : Element {
        private readonly IClashProvider<T1, T2> _clashProvider;

        public ClashVolumeChecker(RevitRepository revitRepository, IClashChecker clashChecker, IClashProvider<T1, T2> clashProvider) : base(revitRepository, clashChecker) {
            _clashProvider = clashProvider;
        }

        public override bool CheckModel(ClashModel clashModel) {
            var clash = _clashProvider.GetClash(_revitRepository, clashModel);
            try {
                var solid = clash.GetIntersection();
                return solid.Volume > clash.GetConnectorArea() * 0.05;
            } catch {
                return true;
            }
        }
        public override string GetMessage() => "Объем пересечения элементов меньше заданного.";
    }
}
