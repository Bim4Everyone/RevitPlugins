using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.LevelFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.OpeningPlacement.PointFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.SolidProviders;
using RevitOpeningPlacement.Models.OpeningUnion;

namespace RevitOpeningPlacement.Models.OpeningPlacement.PlacerInitializers {
    internal class WallOpeningGroupPlacerInitializer : IOpeningGroupPlacerInitializer {
        public OpeningPlacer GetPlacer(RevitRepository revitRepository, OpeningsGroup openingsGroup) {
            return new OpeningPlacer(revitRepository) {
                Type = revitRepository.GetOpeningType(OpeningType.WallRectangle),
                PointFinder = new WallOpeningsGroupPointFinder(openingsGroup),
                LevelFinder = new OpeningsGroupLevelFinder(revitRepository, openingsGroup),
                AngleFinder = new WallOpeningsGroupAngleFinder(openingsGroup),
                ParameterGetter = new WallSolidParameterGetter(new OpeningGroupSolidProvider(openingsGroup))
            };
        }
    }
}
