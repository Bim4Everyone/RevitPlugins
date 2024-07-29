using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitOpeningSlopes.ViewModels {
    internal class SlopeTypeViewModel : IEquatable<SlopeTypeViewModel> {
        private readonly FamilySymbol _slopeType;

        public SlopeTypeViewModel(FamilySymbol slopeType) {
            _slopeType = slopeType ?? throw new ArgumentNullException(nameof(slopeType));
        }

        public string Name => _slopeType.Name;
        public ElementId SlopeTypeId => _slopeType.Id;

        public override bool Equals(object obj) {
            return Equals(obj as SlopeTypeViewModel);
        }

        public bool Equals(SlopeTypeViewModel other) {
            if(ReferenceEquals(null, other)) { return false; };
            if(ReferenceEquals(this, other)) { return true; };
            return _slopeType.Id == other._slopeType.Id;
        }

        public override int GetHashCode() {
            return 1226191301 + EqualityComparer<ElementId>.Default.GetHashCode(SlopeTypeId);
        }

        public override string ToString() {
            return Name;
        }
    }
}
