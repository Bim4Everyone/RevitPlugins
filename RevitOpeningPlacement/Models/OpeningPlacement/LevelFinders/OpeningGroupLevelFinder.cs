using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningUnion;

namespace RevitOpeningPlacement.Models.OpeningPlacement.LevelFinders {
    internal class OpeningsGroupLevelFinder : ILevelFinder {
        private readonly RevitRepository _revitRepository;
        private readonly OpeningsGroup _openingsGroup;

        public OpeningsGroupLevelFinder(RevitRepository revitRepository, OpeningsGroup openingsGroup) {
            _revitRepository = revitRepository;
            _openingsGroup = openingsGroup;
        }

        public Level GetLevel() {
            return _revitRepository.GetLevel(_openingsGroup.Elements.First());
        }
    }
}
