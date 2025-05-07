using System;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitFinishingWalls.Models.Enums;

namespace RevitFinishingWalls.ViewModels {
    internal class WallHeightStyleViewModel : BaseViewModel, IEquatable<WallHeightStyleViewModel> {
        private readonly ILocalizationService _localizationService;

        public WallHeightStyleViewModel(ILocalizationService localizationService, WallHeightStyle wallHeightStyle) {
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));

            WallHeightStyle = wallHeightStyle;
            Name = _localizationService.GetLocalizedString($"{nameof(WallHeightStyle)}.{wallHeightStyle}");
        }

        public WallHeightStyle WallHeightStyle { get; }
        public string Name { get; }

        public bool Equals(WallHeightStyleViewModel other) {
            return WallHeightStyle == other?.WallHeightStyle;
        }

        public override int GetHashCode() {
            return -1014265442 + WallHeightStyle.GetHashCode();
        }

        public override bool Equals(object obj) {
            return Equals(obj as WallHeightStyleViewModel);
        }

        public override string ToString() {
            return Name;
        }
    }
}
