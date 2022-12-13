using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.LevelFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.OpeningPlacement.PointFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.SolidProviders;
using RevitOpeningPlacement.Models.OpeningUnion;

namespace RevitOpeningPlacement.Models.OpeningPlacement.PlacerInitializers {
    internal class FloorOpeningGroupPlacerInitializer : IOpeningGroupPlacerInitializer {
        public OpeningPlacer GetPlacer(RevitRepository revitRepository, OpeningsGroup openingsGroup) {
            return new OpeningPlacer(revitRepository) {
                Type = revitRepository.GetOpeningType(OpeningType.FloorRectangle),

                PointFinder = new FloorOpeningsGroupPointFinder(openingsGroup),

                LevelFinder = new OpeningsGroupLevelFinder(revitRepository, openingsGroup),
                AngleFinder = new ZeroAngleFinder(),

                ParameterGetter = new FloorSolidParameterGetter(new OpeningGroupSolidProvider(openingsGroup))
            };
        }
    }
}
