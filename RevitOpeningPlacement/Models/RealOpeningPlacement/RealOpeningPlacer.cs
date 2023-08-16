using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningPlacement {
    internal class RealOpeningPlacer {
        private readonly RevitRepository _revitRepository;


        public RealOpeningPlacer(RevitRepository revitRepository) {
            if(revitRepository == null) { throw new ArgumentNullException(nameof(revitRepository)); }
            _revitRepository = revitRepository;
        }


        /// <summary>
        /// Размещение чистового отверстия
        /// </summary>
        public void Place() {
            Element host = _revitRepository.PickHostForRealOpening();
            List<OpeningMepTaskIncoming> openingTasks = _revitRepository.PickOpeningTasksIncoming().Where(opening => opening.IntersectsSolid(host.GetSolid(), host.GetBoundingBox())).ToList();

            if(openingTasks.Count == 0) {
                return;
            } else if(openingTasks.Count == 1) {
                PlaceBySimpleAlgorithm(host, openingTasks.First());
            } else {
                PlaceByComplexAlgorithm(host, openingTasks);
            }
        }


        private void PlaceBySimpleAlgorithm(Element host, OpeningMepTaskIncoming openingTask) {
            using(var transaction = _revitRepository.GetTransaction("Размещение чистового отверстия")) {
                var openingType = openingTask.OpeningType;
                var symbol = GetFamilySymbol(host, openingType);

                var instance = _revitRepository.CreateInstance(openingTask.Location, symbol, host);

                transaction.Commit();
            }
        }

        private void PlaceByComplexAlgorithm(Element host, ICollection<OpeningMepTaskIncoming> openingTasks) {

        }

        private FamilySymbol GetFamilySymbol(Element host, OpeningType openingTaskType) {
            if(host is null) { throw new ArgumentNullException(nameof(host)); }

            if(host is Wall) {
                switch(openingTaskType) {
                    case OpeningType.WallRectangle:
                    case OpeningType.FloorRound:
                    case OpeningType.FloorRectangle:
                    return _revitRepository.GetOpeningRealType(OpeningType.WallRectangle);
                    case OpeningType.WallRound:
                    return _revitRepository.GetOpeningRealType(OpeningType.WallRound);
                    default:
                    throw new ArgumentException(nameof(openingTaskType));
                }
            } else if(host is Floor) {
                switch(openingTaskType) {
                    case OpeningType.FloorRectangle:
                    case OpeningType.WallRound:
                    case OpeningType.WallRectangle:
                    return _revitRepository.GetOpeningRealType(OpeningType.FloorRectangle);
                    case OpeningType.FloorRound:
                    return _revitRepository.GetOpeningRealType(OpeningType.FloorRound);
                    default:
                    throw new ArgumentException(nameof(openingTaskType));
                }
            } else {
                throw new ArgumentException(nameof(host));
            }
        }
    }
}
