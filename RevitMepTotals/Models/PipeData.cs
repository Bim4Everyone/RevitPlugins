using System;

using RevitMepTotals.Models.Interfaces;

namespace RevitMepTotals.Models {
    internal class PipeData : IPipeData, IEquatable<PipeData> {
        public PipeData(string typeName, string size, string name) {
            if(string.IsNullOrWhiteSpace(typeName)) { throw new System.ArgumentException(nameof(typeName)); }
            if(string.IsNullOrWhiteSpace(size)) { throw new System.ArgumentException(nameof(size)); }
            if(string.IsNullOrWhiteSpace(name)) { throw new System.ArgumentException(nameof(name)); }

            TypeName = typeName;
            Size = size;
            Name = name;
        }


        public string TypeName { get; }

        public string Size { get; }

        public string Name { get; }

        public double Length { get; set; }


        public override bool Equals(object obj) {
            return (obj != null) && (obj is PipeData other) && Equals(other);
        }

        public override int GetHashCode() {
            return (TypeName + Size + Name).GetHashCode();
        }

        public bool Equals(PipeData other) {
            return (other != null)
                && string.Equals(TypeName, other.TypeName, StringComparison.CurrentCultureIgnoreCase)
                && string.Equals(Size, other.Size, StringComparison.CurrentCultureIgnoreCase)
                && string.Equals(Name, other.Name, StringComparison.CurrentCultureIgnoreCase)
                ;
        }
    }
}
