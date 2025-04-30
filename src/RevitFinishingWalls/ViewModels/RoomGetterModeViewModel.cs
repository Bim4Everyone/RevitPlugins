using System;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitFinishingWalls.Models.Enums;

namespace RevitFinishingWalls.ViewModels {
    internal class RoomGetterModeViewModel : BaseViewModel, IEquatable<RoomGetterModeViewModel> {
        private readonly ILocalizationService _localizationService;

        public RoomGetterModeViewModel(ILocalizationService localizationService, RoomGetterMode roomGetterMode) {
            _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));

            RoomGetterMode = roomGetterMode;
            Name = _localizationService.GetLocalizedString($"{nameof(RoomGetterMode)}.{roomGetterMode}");
        }

        public RoomGetterMode RoomGetterMode { get; }
        public string Name { get; }

        public bool Equals(RoomGetterModeViewModel other) {
            return RoomGetterMode == other?.RoomGetterMode;
        }

        public override int GetHashCode() {
            return -1014265442 + RoomGetterMode.GetHashCode();
        }

        public override bool Equals(object obj) {
            return Equals(obj as RoomGetterModeViewModel);
        }

        public override string ToString() {
            return Name;
        }
    }
}
