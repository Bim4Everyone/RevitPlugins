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

        public void PlaceSlopes(PluginConfig config) {
            using(var transaction = _revitRepository.Document.StartTransaction("Размещение откосов")) {
                ICollection<FamilyInstance> openings = _revitRepository.GetOpenings(config.WindowsGetterMode);
                foreach(FamilyInstance opening in openings) {
                    XYZ origin = _openingParams.GetOpeningCenter(opening);
                    FamilySymbol slopeType = _revitRepository.GetSlopeType(config.SlopeTypeId);
                    FamilyInstance slope = _revitRepository
                        .Document
                        .Create
                        .NewFamilyInstance(origin, slopeType, StructuralType.NonStructural);
                    _slopesParams.SetSlopeParams(slope, opening);
                }
                transaction.Commit();
            }

        }
    }
}

