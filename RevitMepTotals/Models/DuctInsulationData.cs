using System;

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
            return (obj != null) && (obj is DuctInsulationData other) && Equals(other);
        }

        public override int GetHashCode() {
            return (TypeName + DuctSize + Name).GetHashCode();
        }

        public bool Equals(DuctInsulationData other) {
            return (other != null)
                && string.Equals(TypeName, other.TypeName, StringComparison.CurrentCultureIgnoreCase)
                && string.Equals(DuctSize, other.DuctSize, StringComparison.CurrentCultureIgnoreCase)
                && string.Equals(Name, other.Name, StringComparison.CurrentCultureIgnoreCase)
                ;
        }
    }
}
