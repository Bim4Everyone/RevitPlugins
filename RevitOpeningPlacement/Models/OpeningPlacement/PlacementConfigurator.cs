using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.FilterModel;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.OpeningPlacement.Checkers;
using RevitOpeningPlacement.Models.OpeningPlacement.PlacerInitializers;

namespace RevitOpeningPlacement.Models.OpeningPlacement {
    internal class PlacementConfigurator {
        private readonly RevitRepository _revitRepository;
        private readonly MepCategoryCollection _categories;
        private List<ClashModel> _unplacedClashes = new List<ClashModel>();

        public PlacementConfigurator(RevitRepository revitRepository, MepCategoryCollection categories) {
            _revitRepository = revitRepository;
            _categories = categories;
        }

        public IEnumerable<OpeningPlacer> GetPlacers() {
            var pipeFilter = GetPipeFilter();
            var roundDuctFilter = GetRoundDuctFilter();
            var rectangleDuctFilter = GetRectangleDuctFilter();
            var trayFilter = GetTrayFilter();

            var wallFilter = FiltersInitializer.GetWallFilter(_revitRepository.GetClashRevitRepository());
            var floorFilter = FiltersInitializer.GetFloorFilter(_revitRepository.GetClashRevitRepository());

            List<OpeningPlacer> placers = new List<OpeningPlacer>();
            placers.AddRange(GetRoundMepWallPlacers(pipeFilter, wallFilter, _categories[CategoryEnum.Pipe]));
            placers.AddRange(GetRoundMepWallPlacers(roundDuctFilter, wallFilter, _categories[CategoryEnum.RoundDuct]));
            placers.AddRange(GetRectangleMepWallPlacers(rectangleDuctFilter, wallFilter, _categories[CategoryEnum.RectangleDuct]));
            placers.AddRange(GetRectangleMepWallPlacers(trayFilter, wallFilter, _categories[CategoryEnum.CableTray]));
            return placers;
        }

        public List<ClashModel> GetUnplacedClashes() {
            return _unplacedClashes;
        }

        private IEnumerable<OpeningPlacer> GetRectangleMepWallPlacers(Filter rectangleDuctFilter, Filter wallFilter, MepCategory mepCategory) {
            return GetWallClashes(rectangleDuctFilter, wallFilter).Where(item => ClashChecker.CheckWallClash(_revitRepository, item))
                                  .Select(item => RectangleMepWallPlacerInitializer.GetPlacer(_revitRepository, item, mepCategory));
        }

        private IEnumerable<OpeningPlacer> GetRoundMepWallPlacers(Filter roundDuctFilter, Filter wallFilter, MepCategory mepCategory) {
            return GetWallClashes(roundDuctFilter, wallFilter).Where(item => ClashChecker.CheckWallClash(_revitRepository, item))
                                  .Select(item => RoundMepWallPlacerInitializer.GetPlacer(_revitRepository, item, mepCategory));
        }

        private IEnumerable<ClashModel> GetWallClashes(Filter mepFilter, Filter wallFilter) {
            var wallClashes = ClashInitializer.GetClashes(_revitRepository.GetClashRevitRepository(), mepFilter, wallFilter).ToList();

            _unplacedClashes.AddRange(wallClashes.Where(item => !ClashChecker.CheckWallClash(_revitRepository, item)));
            return wallClashes;
        }

        private Filter GetPipeFilter() {
            var minSizePipe = _categories[CategoryEnum.Pipe]?.MinSizes[Parameters.Diameter];
            if(minSizePipe != null) {
                return FiltersInitializer.GetPipeFilter(_revitRepository.GetClashRevitRepository(), minSizePipe.Value);
            }
            return null;
        }

        private Filter GetRoundDuctFilter() {
            var minSizeRoundDuct = _categories[CategoryEnum.RoundDuct]?.MinSizes[Parameters.Diameter];
            if(minSizeRoundDuct != null) {
                return FiltersInitializer.GetRoundDuctFilter(_revitRepository.GetClashRevitRepository(), minSizeRoundDuct.Value);
            }
            return null;
        }

        private Filter GetRectangleDuctFilter() {
            var minSizesRectangleDuct = _categories[CategoryEnum.RectangleDuct]?.MinSizes;
            if(minSizesRectangleDuct != null) {
                var height = minSizesRectangleDuct[Parameters.Height];
                var width = minSizesRectangleDuct[Parameters.Width];
                if(height != null && width != null) {
                    return FiltersInitializer.GetRectangleDuctFilter(_revitRepository.GetClashRevitRepository(), height.Value, width.Value);
                }
            }
            return null;
        }

        private Filter GetTrayFilter() {
            var minSizesTray = _categories[CategoryEnum.CableTray]?.MinSizes;
            if(minSizesTray != null) {
                var height = minSizesTray[Parameters.Height];
                var width = minSizesTray[Parameters.Width];
                if(height != null && width != null) {
                    return FiltersInitializer.GetTrayFilter(_revitRepository.GetClashRevitRepository(), height.Value, width.Value);
                }
            }
            return null;
        }
    }
}
