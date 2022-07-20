using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.FilterModel;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement {
    internal class PlacementConfigurator {

        public static IEnumerable<OpeningPlacer> GetPlacers(RevitRepository revitRepository, RevitClashDetective.Models.RevitRepository clashRevitRepository, MepCategoryCollection categories) {
            var pipeFilter = GetPipeFilter(clashRevitRepository, categories);
            var roundDuctFilter = GetRoundDuctFilter(clashRevitRepository, categories);
            var rectangleDuctFilter = GetRectangleDuctFilter(clashRevitRepository, categories);
            var trayFilter = GetTrayFilter(clashRevitRepository, categories);

            var wallFilter = FiltersInitializer.GetWallFilter(clashRevitRepository);
            var floorFilter = FiltersInitializer.GetFloorFilter(clashRevitRepository);



            return ClashInitializer.GetClashes(clashRevitRepository, pipeFilter, wallFilter)
                                    .Select(item => new OpeningPlacer(revitRepository) {
                                        Clash = item,
                                        PointFinder = GetWallPointFinder(item.MainElement.GetElement() as MEPCurve, item.OtherElement.GetElement() as Wall),
                                        AngleFinder = new WallAngleFinder(item.OtherElement.GetElement() as Wall),
                                        ParameterSetter = new RoundParameterSetter((item.MainElement.GetElement() as MEPCurve).Diameter, 
                                                                                   (item.OtherElement.GetElement() as Wall).Width,
                                                                                   categories[CategoryEnum.Pipe].Offsets),
                                        Type = revitRepository.GetOpeningType(OpeningType.WallRound)
                                    });
            //TODO: добавить все случаи
        }

        private static IPointFinder GetWallPointFinder(MEPCurve curve, Wall wall) {
            if(curve.IsPerpendicular(wall)) {
                return new HorizontalPointFinder(curve, wall);
            } else {
                return new InclinedPointFinder();
            }
        }

        private static Filter GetPipeFilter(RevitClashDetective.Models.RevitRepository revitRepository, MepCategoryCollection categories) {
            var minSizePipe = categories[CategoryEnum.Pipe]?.MinSizes[Parameters.Diameter];
            if(minSizePipe != null) {
                return FiltersInitializer.GetPipeFilter(revitRepository, minSizePipe.Value);
            }
            return null;
        }

        private static Filter GetRoundDuctFilter(RevitClashDetective.Models.RevitRepository revitRepository, MepCategoryCollection categories) {
            var minSizeRoundDuct = categories[CategoryEnum.RoundDuct]?.MinSizes[Parameters.Diameter];
            if(minSizeRoundDuct != null) {
                return FiltersInitializer.GetRoundDuctFilter(revitRepository, minSizeRoundDuct.Value);
            }
            return null;
        }

        private static Filter GetRectangleDuctFilter(RevitClashDetective.Models.RevitRepository revitRepository, MepCategoryCollection categories) {
            var minSizesRectangleDuct = categories[CategoryEnum.RectangleDuct]?.MinSizes;
            if(minSizesRectangleDuct != null) {
                var height = minSizesRectangleDuct[Parameters.Height];
                var width = minSizesRectangleDuct[Parameters.Width];
                if(height != null && width != null) {
                    return FiltersInitializer.GetRectangleDuctFilter(revitRepository, height.Value, width.Value);
                }
            }
            return null;
        }

        private static Filter GetTrayFilter(RevitClashDetective.Models.RevitRepository revitRepository, MepCategoryCollection categories) {
            var minSizesTray = categories[CategoryEnum.CableTray]?.MinSizes;
            if(minSizesTray != null) {
                var height = minSizesTray[Parameters.Height];
                var width = minSizesTray[Parameters.Width];
                if(height != null && width != null) {
                    return FiltersInitializer.GetTrayFilter(revitRepository, height.Value, width.Value);
                }
            }
            return null;
        }
    }
}
