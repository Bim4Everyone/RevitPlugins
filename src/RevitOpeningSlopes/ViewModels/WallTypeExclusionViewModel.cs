using System;

using Autodesk.Revit.DB;

namespace RevitOpeningSlopes.ViewModels {
    internal class WallTypeExclusionViewModel {
        public WallTypeExclusionViewModel(WallType wallType) {
            WallType = wallType ?? throw new ArgumentNullException(nameof(wallType));
        }

        public WallType WallType { get; }
        public ElementId WallTypeId => WallType.Id;
        public string Name => WallType.Name;
        public string FamilyName => WallType.FamilyName;
        public string DisplayName => $"{FamilyName}: {Name}";
    }
}
