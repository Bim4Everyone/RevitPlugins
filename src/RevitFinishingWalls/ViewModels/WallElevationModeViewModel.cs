using System;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitFinishingWalls.Models.Enums;

namespace RevitFinishingWalls.ViewModels {
    internal class WallElevationModeViewModel : BaseViewModel, IEquatable<WallElevationModeViewModel> {
        private readonly ILocalizationService _localizationService;

        public WallElevationModeViewModel(ILocalizationService localizationService, WallElevationMode elevationMode) {
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));

            ElevationMode = elevationMode;
            Name = _localizationService.GetLocalizedString($"{nameof(WallElevationMode)}.{elevationMode}");
        }


        public WallElevationMode ElevationMode { get; }
        public string Name { get; }

        public bool Equals(WallElevationModeViewModel other) {
            return ElevationMode == other?.ElevationMode;
        }

        public override int GetHashCode() {
            return -801844897 + ElevationMode.GetHashCode();
        }

        public override bool Equals(object obj) {
            return Equals(obj as WallElevationModeViewModel);
        }

        public override string ToString() {
            return Name;
        }
    }
}
