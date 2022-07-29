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
        private readonly List<DocInfo> _docInfos;
        private List<ClashModel> _unplacedClashes = new List<ClashModel>();

        public PlacementConfigurator(RevitRepository revitRepository, MepCategoryCollection categories) {
            _revitRepository = revitRepository;
            _categories = categories;
            _docInfos = _revitRepository.GetDocInfos();
        }

        public IEnumerable<OpeningPlacer> GetPlacers() {
            var pipeFilter = GetPipeFilter();
            var roundDuctFilter = GetRoundDuctFilter();
            var rectangleDuctFilter = GetRectangleDuctFilter();
            var trayFilter = GetTrayFilter();

            var wallFilter = FiltersInitializer.GetWallFilter(_revitRepository.GetClashRevitRepository());
            var floorFilter = FiltersInitializer.GetFloorFilter(_revitRepository.GetClashRevitRepository());

            List<OpeningPlacer> placers = new List<OpeningPlacer>();



            placers.AddRange(GetPipeWallPlacers(pipeFilter, wallFilter));

            placers.AddRange(GetRoundDuctWallPlacers(roundDuctFilter, wallFilter));
            return placers;
            //TODO: добавить все случаи
        }

        public List<ClashModel> GetUnplacedClashes() {
            return _unplacedClashes;
        }

        private IEnumerable<OpeningPlacer> GetRoundDuctWallPlacers(Filter roundDuctFilter, Filter wallFilter) {
            var pipeWallClashes = ClashInitializer.GetClashes(_revitRepository.GetClashRevitRepository(), roundDuctFilter, wallFilter).ToList();

            _unplacedClashes.AddRange(pipeWallClashes.Where(item => !ClashChecker.CheckWallClash(_docInfos, item)));

            return pipeWallClashes.Where(item => ClashChecker.CheckWallClash(_docInfos, item))
                                  .Select(item => RoundMepWallPlacerInitializer.GetPlacer(_revitRepository, _docInfos, item, _categories[CategoryEnum.RoundDuct]));
        }

        private IEnumerable<OpeningPlacer> GetPipeWallPlacers(Filter pipeFilter, Filter wallFilter) {
            var pipeWallClashes = ClashInitializer.GetClashes(_revitRepository.GetClashRevitRepository(), pipeFilter, wallFilter).ToList();

            _unplacedClashes.AddRange(pipeWallClashes.Where(item => !ClashChecker.CheckWallClash(_docInfos, item)));

            return pipeWallClashes.Where(item => ClashChecker.CheckWallClash(_docInfos, item))
                                  .Select(item => RoundMepWallPlacerInitializer.GetPlacer(_revitRepository, _docInfos, item, _categories[CategoryEnum.Pipe]));
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
