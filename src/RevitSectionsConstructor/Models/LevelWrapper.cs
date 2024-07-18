using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSectionsConstructor.Models {
    internal class LevelWrapper : IEquatable<LevelWrapper> {
        public LevelWrapper(Level level) {
            if(level is null) { throw new ArgumentNullException(nameof(level)); }

            LevelId = level.Id;
            Elevation = level.Elevation;
            Name = level.Name;
        }


        public ElementId LevelId { get; }
        public double Elevation { get; }
        public string Name { get; }


        public bool Equals(LevelWrapper other) {
            if(other is null) { return false; }
            if(ReferenceEquals(this, other)) { return true; }
            return LevelId == other.LevelId;
        }

        public override bool Equals(object obj) {
            return Equals(obj as LevelWrapper);
        }

        public override int GetHashCode() {
            int hashCode = -659757646;
            hashCode = hashCode * -1521134295 + EqualityComparer<ElementId>.Default.GetHashCode(LevelId);
            return hashCode;
        }

        public override string ToString() {
            return Name;
        }
    }
}
