using System;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using RevitApartmentPlans.Models;

namespace RevitApartmentPlans.Services {
    internal class BoundsCalculateService : IBoundsCalculateService {
        private readonly RevitRepository _revitRepository;
        private readonly ICurveLoopsMerger _curveLoopsMerger;
        private readonly ICurveLoopsOffsetter _curveLoopsOffsetter;

        public BoundsCalculateService(
            RevitRepository revitRepository,
            ICurveLoopsMerger curveLoopsMerger,
            ICurveLoopsOffsetter curveLoopsOffsetter) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _curveLoopsMerger = curveLoopsMerger ?? throw new ArgumentNullException(nameof(curveLoopsMerger));
            _curveLoopsOffsetter = curveLoopsOffsetter ?? throw new ArgumentNullException(nameof(curveLoopsOffsetter));
        }


        public CurveLoop CreateBounds(Apartment apartment, double feetOffset) {
            var curveLoops = apartment.GetRooms()
                .Select(r => GetOuterRoomBound(r))
                .ToArray();
            return _curveLoopsOffsetter.CreateOffsetLoop(_curveLoopsMerger.Merge(curveLoops), feetOffset);
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
