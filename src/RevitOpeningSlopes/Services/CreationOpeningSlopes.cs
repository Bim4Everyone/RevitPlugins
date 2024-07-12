using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

using dosymep.Revit;

using RevitOpeningSlopes.Models;
using RevitOpeningSlopes.Models.Exceptions;

namespace RevitOpeningSlopes.Services {
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
            if(!slopeType.IsActive) {
                slopeType.Activate();
            }
            FamilyInstance slope = _revitRepository
                        .Document
                        .Create
                        .NewFamilyInstance(slopeCreationData.Center, slopeType, StructuralType.NonStructural);
            _slopeParams.SetSlopeParams(slope, slopeCreationData);
        }

        public void CreateSlopes(PluginConfig config,
            ICollection<FamilyInstance> openings,
            out string error,
            IProgress<int> progress = null,
            CancellationToken ct = default) {
            if(config is null) { throw new ArgumentNullException(nameof(config)); }
            StringBuilder sb = new StringBuilder();

            using(var transaction = _revitRepository.Document.StartTransaction("Размещение откосов")) {
                int i = 0;
                foreach(FamilyInstance opening in openings) {
                    ct.ThrowIfCancellationRequested();
                    progress.Report(i++);
                    try {
                        SlopeCreationData slopeCreationData = _slopesDataGetter
                            .GetOpeningSlopeCreationData(config, opening);
                        CreateSlope(slopeCreationData);
                    } catch(OpeningNullSolidException e) {
                        sb.AppendLine($"{e.Message}, Id = {opening.Id}");
                    } catch(ArgumentException e) {
                        sb.AppendLine($"{e.Message}, Id = {opening.Id}");
                    }
                }
                transaction.Commit();
            }
            error = sb.ToString();
        }
    }
}
