using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitFinishingWalls.ViewModels {
    internal class WallTypeViewModel : IEquatable<WallTypeViewModel> {
        private readonly WallType _wallType;

        public WallTypeViewModel(WallType wallType) {
            _wallType = wallType ?? throw new ArgumentNullException(nameof(wallType));
        }


        public string Name => _wallType.Name;

        public ElementId WallTypeId => _wallType.Id;


        public override bool Equals(object obj) {
            return Equals(obj as WallTypeViewModel);
        }

        public bool Equals(WallTypeViewModel other) {
            if(ReferenceEquals(null, other)) { return false; }
            if(ReferenceEquals(this, other)) { return true; }
            return _wallType.Id == other._wallType.Id;
        }

        public override int GetHashCode() {
            return 1320577646 + EqualityComparer<ElementId>.Default.GetHashCode(_wallType.Id);
        }

        public override string ToString() {
            return Name;
        }
    }
}
