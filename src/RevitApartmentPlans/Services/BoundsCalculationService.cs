using System;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using RevitApartmentPlans.Models;

namespace RevitApartmentPlans.Services {
    internal class BoundsCalculationService : IBoundsCalculationService {
        private readonly RevitRepository _revitRepository;
        private readonly ICurveLoopsMerger _curveLoopsMerger;
        private readonly ICurveLoopsOffsetter _curveLoopsOffsetter;
        private readonly ICurveLoopsSimplifier _curveLoopsSimplifier;

        public BoundsCalculationService(
            RevitRepository revitRepository,
            ICurveLoopsMerger curveLoopsMerger,
            ICurveLoopsOffsetter curveLoopsOffsetter,
            ICurveLoopsSimplifier curveLoopsSimplifier) {

            _revitRepository = revitRepository
                ?? throw new ArgumentNullException(nameof(revitRepository));
            _curveLoopsMerger = curveLoopsMerger
                ?? throw new ArgumentNullException(nameof(curveLoopsMerger));
            _curveLoopsOffsetter = curveLoopsOffsetter
                ?? throw new ArgumentNullException(nameof(curveLoopsOffsetter));
            _curveLoopsSimplifier = curveLoopsSimplifier
                ?? throw new ArgumentNullException(nameof(curveLoopsSimplifier));
        }


        public CurveLoop CreateBounds(Apartment apartment, double feetOffset) {
            var curveLoops = apartment.GetRooms()
                .Select(r => GetOuterRoomBound(r))
                .ToArray();
            var mergedLoop = _curveLoopsMerger.Merge(curveLoops);
            var loop = _curveLoopsOffsetter.CreateOffsetLoop(mergedLoop, feetOffset);
            try {
                return _curveLoopsSimplifier.Simplify(loop);
            } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                return loop;
            }
        }

        /// <summary>
        /// Находит наружный контур помещения
        /// </summary>
        /// <param name="room">Помещение</param>
        /// <returns>Внешний контур помещения</returns>
        private CurveLoop GetOuterRoomBound(Room room) {
            return _revitRepository.GetBoundaryCurveLoops(room)
                .OrderByDescending(loop => loop.GetExactLength())
                .First();
        }
    }
}
