
using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.OpeningPlacement;

namespace RevitOpeningPlacement.Models.Interfaces {
    internal interface IPlacerInitializer {
        OpeningPlacer GetPlacer(RevitRepository revitRepository, ClashModel clashModel, MepCategory categoryOption);
    }
}
