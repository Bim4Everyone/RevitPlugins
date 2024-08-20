using System.Linq;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningUnion;

namespace RevitOpeningPlacement.Models.OpeningPlacement.LevelFinders {
    internal class OpeningsGroupLevelFinder : ILevelFinder {
        private readonly OpeningsGroup _openingsGroup;

        public OpeningsGroupLevelFinder(OpeningsGroup openingsGroup) {
            _openingsGroup = openingsGroup;
        }

        public Level GetLevel() {
            return RevitRepository.GetLevel(_openingsGroup.Elements.First().GetFamilyInstance());
        }
    }
}
