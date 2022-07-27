using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models;
using RevitClashDetective.Models.FilterModel;

using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.Models.OpeningPlacement {
    internal class PlacementConfigurator {
        private readonly RevitRepository _revitRepository;
        private readonly RevitClashDetective.Models.RevitRepository _clashRevitRepository;
        private readonly MepCategoryCollection _categories;
        private readonly List<DocInfo> _docInfos;

        public PlacementConfigurator(RevitRepository revitRepository, RevitClashDetective.Models.RevitRepository clashRevitRepository, MepCategoryCollection categories) {
            _revitRepository = revitRepository;
            _clashRevitRepository = clashRevitRepository;
            _categories = categories;
            _docInfos = _clashRevitRepository.GetDocInfos();
        }

        public IEnumerable<OpeningPlacer> GetPlacers() {
            var pipeFilter = GetPipeFilter();
            var roundDuctFilter = GetRoundDuctFilter();
            var rectangleDuctFilter = GetRectangleDuctFilter();
            var trayFilter = GetTrayFilter();

            var wallFilter = FiltersInitializer.GetWallFilter(_clashRevitRepository);
            var floorFilter = FiltersInitializer.GetFloorFilter(_clashRevitRepository);

            return ClashInitializer.GetClashes(_clashRevitRepository, pipeFilter, wallFilter)
                                    .Where(item =>ClashChecker.CheckPipeWallClash(item))
                                    .Select(item => new OpeningPlacer(_revitRepository) {
                                        Clash = item,
                                        PointFinder = new HorizontalPointFinder((MEPCurve) item.MainElement.GetElement(),
                                                                                (Wall) item.OtherElement.GetElement(),
                                                                                GetTransform(item.OtherElement.GetElement())),
                                        AngleFinder = new WallAngleFinder(item.OtherElement.GetElement() as Wall, GetTransform(item.OtherElement.GetElement())),
                                        ParameterGetter = new RoundCurveWithWallParamterGetter((MEPCurve) item.MainElement.GetElement(),
                                                                                               (Wall) item.OtherElement.GetElement(),
                                                                                               GetTransform(item.OtherElement.GetElement())),
                                        Type = _revitRepository.GetOpeningType(OpeningType.WallRound)
                                    });
            //TODO: добавить все случаи
        }

        private Transform GetTransform(Element element) {
            return _docInfos.FirstOrDefault(item => item.Name.Equals(_clashRevitRepository.GetDocumentName(element.Document), StringComparison.CurrentCultureIgnoreCase))?.Transform
                ?? Transform.Identity;
        }

        private Filter GetPipeFilter() {
            var minSizePipe = _categories[CategoryEnum.Pipe]?.MinSizes[Parameters.Diameter];
            if(minSizePipe != null) {
                return FiltersInitializer.GetPipeFilter(_clashRevitRepository, minSizePipe.Value);
            }
            return null;
        }

        private Filter GetRoundDuctFilter() {
            var minSizeRoundDuct = _categories[CategoryEnum.RoundDuct]?.MinSizes[Parameters.Diameter];
            if(minSizeRoundDuct != null) {
                return FiltersInitializer.GetRoundDuctFilter(_clashRevitRepository, minSizeRoundDuct.Value);
            }
            return null;
        }

        private Filter GetRectangleDuctFilter() {
            var minSizesRectangleDuct = _categories[CategoryEnum.RectangleDuct]?.MinSizes;
            if(minSizesRectangleDuct != null) {
                var height = minSizesRectangleDuct[Parameters.Height];
                var width = minSizesRectangleDuct[Parameters.Width];
                if(height != null && width != null) {
                    return FiltersInitializer.GetRectangleDuctFilter(_clashRevitRepository, height.Value, width.Value);
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
                    return FiltersInitializer.GetTrayFilter(_clashRevitRepository, height.Value, width.Value);
                }
            }
            return null;
        }
    }
}
