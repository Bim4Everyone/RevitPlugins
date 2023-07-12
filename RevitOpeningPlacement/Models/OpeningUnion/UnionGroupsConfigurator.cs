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

        /// <summary>
        /// Возвращает список сущностей для дальнейшей расстановки исходящих заданий на отверстия от инженера из текущего файла Revit
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public List<OpeningPlacer> GetPlacersMepTasksOutcoming(IProgress<int> progress, CancellationToken ct) {
            var wallOpeningsGroup = GetOpeningsGroupsMepTasksOutcomingInWall(progress, ct);
            _elementsToDelete.AddRange(wallOpeningsGroup.SelectMany(item => item.Elements));
            var floorOpeningsGroup = GetOpeningsGroupsMepTasksOutcomingInFloor(progress, ct);
            _elementsToDelete.AddRange(floorOpeningsGroup.SelectMany(item => item.Elements));
            var wallOpeningGroupPlacerInitializer = new WallOpeningGroupPlacerInitializer();
            var floorOpeningGroupPlacerInitializer = new FloorOpeningGroupPlacerInitializer();
            return wallOpeningsGroup.Select(item => wallOpeningGroupPlacerInitializer.GetPlacer(_revitRepository, item))
                .Union(floorOpeningsGroup.Select(item => floorOpeningGroupPlacerInitializer.GetPlacer(_revitRepository, item)))
                .ToList();
        }

        /// <summary>
        /// Возвращает список групп исходящих заданий на отверстия от инженера из текущего документа Revit, в которых находятся пересекающие друг друга задания на отверстия
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        public List<OpeningsGroup> GetGroupsMepTasksOutcoming(IProgress<int> progress, CancellationToken ct) {
            return GetOpeningsGroupsMepTasksOutcomingInWall(progress, ct)
                .Union(GetOpeningsGroupsMepTasksOutcomingInFloor(progress, ct))
                .ToList();
        }

        /// <summary>
        /// Возвращает список групп исходящих заданий на отверстия в стенах от инженера из текущего документа Revit, в которых находятся пересекающие друг друга задания на отверстия
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private List<OpeningsGroup> GetOpeningsGroupsMepTasksOutcomingInWall(IProgress<int> progress, CancellationToken ct) {
            return new UnionGroupProvider(new WallIntersectionProvider()).GetOpeningGroups(_revitRepository.GetWallOpeningsMepTasksOutcoming(), progress, ct, 0);
        }

        /// <summary>
        /// Возвращает список групп исходящих заданий на отверстия в перекрытиях от инженера из текущего документа Revit, в которых находятся пересекающие друг друга задания на отверстия
        /// </summary>
        /// <param name="progress"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        private List<OpeningsGroup> GetOpeningsGroupsMepTasksOutcomingInFloor(IProgress<int> progress, CancellationToken ct) {
            return new UnionGroupProvider(new FloorIntersectionProvider()).GetOpeningGroups(_revitRepository.GetFloorOpeningsMepTasksOutcoming(), progress, ct, _revitRepository.GetWallOpeningsMepTasksOutcoming().Count);
        }

        public List<Element> GetElementsToDelete() {
            return _elementsToDelete;
        }
    }
}
