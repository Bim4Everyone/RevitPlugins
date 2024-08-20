
using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.OpeningPlacement;

namespace RevitOpeningPlacement.Models.Interfaces {
    internal interface IFittingPlacerInitializer {
        OpeningPlacer GetPlacer(RevitRepository revitRepository, ClashModel clashModel, params MepCategory[] categoryOptions);

    }
}
