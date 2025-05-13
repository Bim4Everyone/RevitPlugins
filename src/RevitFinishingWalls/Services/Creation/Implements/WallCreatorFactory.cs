using System;

using Ninject;
using Ninject.Syntax;

using RevitFinishingWalls.Models;

namespace RevitFinishingWalls.Services.Creation.Implements {
    internal class WallCreatorFactory : IWallCreatorFactory {
        private readonly IResolutionRoot _resolutionRoot;

        public WallCreatorFactory(IResolutionRoot resolutionRoot) {
            _resolutionRoot = resolutionRoot ?? throw new ArgumentNullException(nameof(resolutionRoot));
        }


        public IWallCreator Create(RevitSettings settings) {
            if(settings.WallHeightStyle == Models.Enums.WallHeightStyle.UpToLevel
                && settings.WallTopElevationMode == Models.Enums.WallElevationMode.HeightByRoom) {
                return _resolutionRoot.Get<TopConnectedToRoomTopWallCreator>();

            } else if(settings.WallHeightStyle == Models.Enums.WallHeightStyle.UpToLevel
                && settings.WallTopElevationMode == Models.Enums.WallElevationMode.ManualHeight) {
                return _resolutionRoot.Get<TopConnectedToLevelWallCreator>();

            } else {
                return _resolutionRoot.Get<UnconnectedTopWallCreator>();
            }
        }
    }
}
