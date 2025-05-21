using RevitFinishingWalls.Models;

namespace RevitFinishingWalls.Services.Creation {
    internal interface IWallCreatorFactory {
        IWallCreator Create(RevitSettings settings);
    }
}
