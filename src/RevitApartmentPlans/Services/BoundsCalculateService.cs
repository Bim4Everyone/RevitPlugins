using System;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using RevitApartmentPlans.Models;

namespace RevitApartmentPlans.Services {
    internal class BoundsCalculateService : IBoundsCalculateService {
        private readonly RevitRepository _revitRepository;
        private readonly ICurveLoopsMerger _curveLoopsMerger;


        public BoundsCalculateService(RevitRepository revitRepository, ICurveLoopsMerger curveLoopsMerger) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
            _curveLoopsMerger = curveLoopsMerger ?? throw new ArgumentNullException(nameof(curveLoopsMerger));
        }


        public CurveLoop CreateBounds(Apartment apartment, double feetOffset) {
            var curveLoops = apartment.GetRooms()
                .Select(r => GetOuterRoomBound(r))
                .ToArray();
            return CurveLoop.CreateViaOffset(_curveLoopsMerger.Merge(curveLoops), feetOffset, XYZ.BasisZ);
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
