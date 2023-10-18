using System;

using RevitMepTotals.Models.Interfaces;

namespace RevitMepTotals.Models {
    internal class DuctData : IDuctData, IEquatable<DuctData> {
        public DuctData(string typeName, string size, string name) {
            if(string.IsNullOrWhiteSpace(typeName)) { throw new ArgumentException(nameof(typeName)); }
            if(string.IsNullOrWhiteSpace(size)) { throw new ArgumentException(nameof(size)); }
            if(string.IsNullOrWhiteSpace(name)) { throw new ArgumentException(nameof(name)); }

            TypeName = typeName;
            Size = size;
            Name = name;
        }


        public string TypeName { get; }

        public string Size { get; }

        public string Name { get; }

        public double Length { get; set; }

        public override bool Equals(object obj) {
            return (obj != null) && (obj is DuctData other) && Equals(other);
        }

        public override int GetHashCode() {
            return (TypeName + Size + Name).GetHashCode();
        }

        public bool Equals(DuctData other) {
            return (other != null)
                && string.Equals(TypeName, other.TypeName, StringComparison.CurrentCultureIgnoreCase)
                && string.Equals(Size, other.Size, StringComparison.CurrentCultureIgnoreCase)
                && string.Equals(Name, other.Name, StringComparison.CurrentCultureIgnoreCase)
                ;
        }
    }
}
