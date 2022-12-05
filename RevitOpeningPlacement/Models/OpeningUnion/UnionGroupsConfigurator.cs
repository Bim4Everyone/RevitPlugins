using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningPlacement.PlacerInitializers;
using RevitOpeningPlacement.Models.OpeningUnion.IntersectionProviders;

namespace RevitOpeningPlacement.Models.OpeningUnion {
    internal class UnionGroupsConfigurator {
        private readonly RevitRepository _revitRepository;
        private List<Element> _elementsToDelete = new List<Element>();

        public UnionGroupsConfigurator(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }

        public List<OpeningPlacer> GetPlacers(IProgress<int> progress, CancellationToken ct) {
            var wallOpeningsGroup = GetOpeningsGroupsInWall(progress, ct);
            _elementsToDelete.AddRange(wallOpeningsGroup.SelectMany(item => item.Elements));
            var floorOpeningsGroup = GetOpeningsGroupsInFloor(progress, ct);
            _elementsToDelete.AddRange(floorOpeningsGroup.SelectMany(item => item.Elements));
            return wallOpeningsGroup.Select(item => new WallOpeningGroupPlacerInitializer().GetPlacer(_revitRepository, item))
                .Union(floorOpeningsGroup.Select(item => new FloorOpeningGroupPlacerInitializer().GetPlacer(_revitRepository, item)))
                .ToList();
        }

        public List<OpeningsGroup> GetGroups(IProgress<int> progress, CancellationToken ct) {
            return GetOpeningsGroupsInWall(progress, ct)
                .Union(GetOpeningsGroupsInFloor(progress, ct))
                .ToList();
        }

        private List<OpeningsGroup> GetOpeningsGroupsInWall(IProgress<int> progress, CancellationToken ct) {
            return new UnionGroupProvider(new WallIntersectionProvider()).GetOpeningGroups(_revitRepository.GetWallOpenings(), progress, ct, 0);
        }

        private List<OpeningsGroup> GetOpeningsGroupsInFloor(IProgress<int> progress, CancellationToken ct) {
            return new UnionGroupProvider(new FloorIntersectionProvider()).GetOpeningGroups(_revitRepository.GetFloorOpenings(), progress, ct, _revitRepository.GetWallOpenings().Count);
        }

        public List<Element> GetElementsToDelete() {
            return _elementsToDelete;
        }
    }
}
