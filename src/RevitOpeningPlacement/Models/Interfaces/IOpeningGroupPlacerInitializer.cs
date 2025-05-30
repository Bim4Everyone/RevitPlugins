using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningUnion;

namespace RevitOpeningPlacement.Models.Interfaces {
    internal interface IOpeningGroupPlacerInitializer {
        OpeningPlacer GetPlacer(RevitRepository revitRepository, OpeningConfig config, OpeningsGroup openingsGroup);
    }
}
