using System.Collections.Generic;
using System.Linq;

namespace RevitOpeningSlopes.Models {
    internal class OpeningSlopesPlacement {
        private readonly RevitRepository _revitRepository;
        private readonly LinesFromOpening _linesFromOpening;
        private readonly SlopeParams _slopesParams;

        public OpeningSlopesPlacement(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
            _linesFromOpening = new LinesFromOpening(revitRepository);
            _slopesParams = new SlopeParams(revitRepository);
        }
        public IList<SlopeCreationData> GetSlopeCreationData(PluginConfig config) {
            ICollection<Opening> openings = _revitRepository
                    .GetWindows(config.WindowsGetterMode)
                    .Select(o => new Opening(_revitRepository, o))
                    .Where(o => o.Height != 0 && o.Width != 0)
                    .ToList();

            List<SlopeCreationData> slopeCreationData = new List<SlopeCreationData>();
            SlopeCreationData slopeData = null;
            foreach(Opening opening in openings) {
                slopeData = new SlopeCreationData(_revitRepository.Document) {
                    Height = opening.Height,
                    Width = opening.Width,
                    Center = opening.Center,
                    SlopeTypeId = config.SlopeTypeId
                };
                slopeCreationData.Add(slopeData);
            }
            return slopeCreationData;
        }
        //public void PlaceSlopes(PluginConfig config) {
        //    using(var transaction = _revitRepository.Document.StartTransaction("Размещение откосов")) {
        //        //ICollection<FamilyInstance> openings = _revitRepository.GetOpenings(config.WindowsGetterMode);
        //        //ICollection<Opening> openings = _revitRepository
        //        //    .GetWindows(config.WindowsGetterMode)
        //        //    .Select(o => new Opening(_revitRepository, o))
        //        //    .ToList();
        //        foreach(Opening opening in openings) {
        //            XYZ origin = opening.Center;
        //            FamilySymbol slopeType = _revitRepository.GetSlopeType(config.SlopeTypeId);
        //            FamilyInstance slope = _revitRepository
        //                .Document
        //                .Create
        //                .NewFamilyInstance(origin, slopeType, StructuralType.NonStructural);
        //            _slopesParams.SetSlopeParams(slope, opening);
        //        }
        //        transaction.Commit();
        //    }

        //}
    }
}

