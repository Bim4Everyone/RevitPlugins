using System;
using System.Collections.Generic;

using RevitMepTotals.Models.Interfaces;

namespace RevitMepTotals.Models {
    internal class DuctInsulationData : IDuctInsulationData, IEquatable<DuctInsulationData> {
        public DuctInsulationData(string typeName, string ductSize, string name) {
            if(string.IsNullOrWhiteSpace(typeName)) { throw new ArgumentException(nameof(typeName)); }
            if(string.IsNullOrWhiteSpace(ductSize)) { throw new ArgumentException(nameof(ductSize)); }
            if(string.IsNullOrWhiteSpace(name)) { throw new ArgumentException(nameof(name)); }

            TypeName = typeName;
            DuctSize = ductSize;
            Name = name;
        }


        public string TypeName { get; }

        public string DuctSize { get; }

        public string Name { get; }

        public double Thickness { get; set; }

        public double Length { get; set; }

        public double Area { get; set; }


        public override bool Equals(object obj) {
            return Equals(obj as DuctInsulationData);
        }

        public override int GetHashCode() {
            int hashCode = -841646042;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TypeName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(DuctSize);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + Thickness.GetHashCode();
            hashCode = hashCode * -1521134295 + Length.GetHashCode();
            hashCode = hashCode * -1521134295 + Area.GetHashCode();
            return hashCode;
        }

        public bool Equals(DuctInsulationData other) {
            if(ReferenceEquals(null, other)) { return false; }
            if(ReferenceEquals(this, other)) { return true; }

            return TypeName == other.TypeName
                && DuctSize == other.DuctSize
                && Name == other.Name
                && Thickness == other.Thickness
                && Length == other.Length
                && Area == other.Area
                ;
        }
    }
}
