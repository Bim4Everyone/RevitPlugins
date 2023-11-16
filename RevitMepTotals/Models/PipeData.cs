using System;
using System.Collections.Generic;

using RevitMepTotals.Models.Interfaces;

namespace RevitMepTotals.Models {
    internal class PipeData : IPipeData, IEquatable<PipeData> {
        public PipeData(string systemName, string typeName, string size, string name) {
            if(string.IsNullOrWhiteSpace(systemName)) { throw new ArgumentException(nameof(systemName)); }
            if(string.IsNullOrWhiteSpace(typeName)) { throw new ArgumentException(nameof(typeName)); }
            if(string.IsNullOrWhiteSpace(size)) { throw new ArgumentException(nameof(size)); }
            if(string.IsNullOrWhiteSpace(name)) { throw new ArgumentException(nameof(name)); }

            SystemName = systemName;
            TypeName = typeName;
            Size = size;
            Name = name;
        }


        public string TypeName { get; }

        public string Size { get; }

        public string Name { get; }

        public double Length { get; set; }

        public string SystemName { get; }


        public override bool Equals(object obj) {
            return Equals(obj as PipeData);
        }

        public override int GetHashCode() {
            int hashCode = 608894917;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SystemName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TypeName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Size);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + Length.GetHashCode();
            return hashCode;
        }

        public bool Equals(PipeData other) {
            if(ReferenceEquals(null, other)) { return false; }
            if(ReferenceEquals(this, other)) { return true; }

            return SystemName == other.SystemName
                && TypeName == TypeName
                && Size == other.Size
                && Name == other.Name
                && Length == other.Length
                ;
        }
    }
}
