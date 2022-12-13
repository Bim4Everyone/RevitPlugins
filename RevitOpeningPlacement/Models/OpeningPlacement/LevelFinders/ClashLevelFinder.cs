using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.LevelFinders {
    internal class ClashLevelFinder : ILevelFinder {
        private readonly RevitRepository _revitRepository;
        private readonly ClashModel _clash;

        public ClashLevelFinder(RevitRepository revitRepository, ClashModel clash) {
            _revitRepository = revitRepository;
            _clash = clash;
        }

        public Level GetLevel() {
            return _revitRepository.GetLevel(_clash.MainElement.Level);
        }
    }
}
