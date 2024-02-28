using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

using dosymep.Revit;

namespace RevitOpeningSlopes.Models {
    internal class OpeningSlopesPlacement {
        private readonly RevitRepository _revitRepository;
        private readonly LinesFromOpening _linesFromOpening;
        private readonly OpeningParams _openingParams;
        private readonly SlopesParams _slopesParams;

        public OpeningSlopesPlacement(RevitRepository revitRepository, LinesFromOpening linesFromOpening) {
            _revitRepository = revitRepository;
            _linesFromOpening = linesFromOpening;
            _openingParams = new OpeningParams(revitRepository);
            _slopesParams = new SlopesParams(revitRepository);
        }

        public void OpeningPlacements(IList<FamilyInstance> openings) {
            using(var transaction = _revitRepository.Document.StartTransaction("Размещение откосов")) {
                foreach(FamilyInstance opening in openings) {
                    XYZ origin = _openingParams.GetOpeningCenter(opening);
                    FamilySymbol fm = _revitRepository.GetFamilySymbols()[0];
                    FamilyInstance slope = _revitRepository
                        .Document
                        .Create
                        .NewFamilyInstance(origin, fm, StructuralType.NonStructural);
                    _slopesParams.SetSlopeParams(slope, opening);
                }
                transaction.Commit();
            }

        }
    }
}

