using System;
using System.Collections.Generic;
using System.Text;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

using dosymep.Revit;

using RevitOpeningSlopes.Services;

namespace RevitOpeningSlopes.Models.Services {
    internal class CreationOpeningSlopes : ICreationOpeningSlopes {
        private readonly RevitRepository _revitRepository;

        private readonly SlopeParams _slopeParams;
        private readonly SlopesDataGetter _slopesDataGetter;

        public CreationOpeningSlopes(
            RevitRepository revitRepository,
            SlopesDataGetter slopesDataGetter) {

            _revitRepository = revitRepository
                ?? throw new ArgumentNullException(nameof(revitRepository));
            _slopesDataGetter = slopesDataGetter ?? throw new ArgumentNullException(nameof(slopesDataGetter));
            _slopeParams = new SlopeParams(revitRepository);
        }
        public void CreateSlope(SlopeCreationData slopeCreationData) {
            FamilySymbol slopeType = _revitRepository.GetSlopeType(slopeCreationData.SlopeTypeId);
            FamilyInstance slope = _revitRepository
                        .Document
                        .Create
                        .NewFamilyInstance(slopeCreationData.Center, slopeType, StructuralType.NonStructural);
            _slopeParams.SetSlopeParams(slope, slopeCreationData);
        }
        public void CreateSlopes(PluginConfig config, out string error) {
            if(config is null) { throw new ArgumentNullException(nameof(config)); }
            StringBuilder sb = new StringBuilder();

            using(var transaction = _revitRepository.Document.StartTransaction("Размещение откосов")) {
                IList<SlopeCreationData> slopeCreationData = _slopesDataGetter.GetSlopeCreationData(config);
                foreach(SlopeCreationData slope in slopeCreationData) {
                    CreateSlope(slope);

                }
                transaction.Commit();
            }
            error = sb.ToString();
        }

    }
}
