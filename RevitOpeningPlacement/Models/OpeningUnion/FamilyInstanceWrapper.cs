using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.OpeningUnion {
    internal class FamilyInstanceWrapper : IEquatable<FamilyInstanceWrapper> {
        public FamilyInstance Element { get; set; }

        public override bool Equals(object obj) {
            return Equals(obj as FamilyInstanceWrapper);
        }

        public bool Equals(FamilyInstanceWrapper other) {
            return other != null &&
                   EqualityComparer<ElementId>.Default.Equals(Element.Id, other.Element.Id);
        }

        public override int GetHashCode() {
            return -703426257 + EqualityComparer<ElementId>.Default.GetHashCode(Element.Id);
        }
    }
}
