using System;
using System.Collections.Generic;

using RevitMepTotals.Models.Interfaces;

namespace RevitMepTotals.Models {
    internal class PipeInsulationData : IPipeInsulationData, IEquatable<PipeInsulationData> {
        public PipeInsulationData(string systemName, string typeName, string pipeSize, string name) {
            if(string.IsNullOrWhiteSpace(systemName)) { throw new ArgumentException(nameof(systemName)); }
            if(string.IsNullOrWhiteSpace(typeName)) { throw new ArgumentException(nameof(typeName)); }
            if(string.IsNullOrWhiteSpace(pipeSize)) { throw new ArgumentException(nameof(pipeSize)); }
            if(string.IsNullOrWhiteSpace(name)) { throw new ArgumentException(nameof(name)); }

            SystemName = systemName;
            TypeName = typeName;
            PipeSize = pipeSize;
            Name = name;
        }


        public string TypeName { get; }

        public string PipeSize { get; }

        public string Name { get; }

        public double Thickness { get; set; }

        public double Length { get; set; }

        public string SystemName { get; }


        public override bool Equals(object obj) {
            return Equals(obj as PipeInsulationData);
        }

        public override int GetHashCode() {
            int hashCode = -151365086;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(SystemName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TypeName);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(PipeSize);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + Thickness.GetHashCode();
            hashCode = hashCode * -1521134295 + Length.GetHashCode();
            return hashCode;
        }

        public bool Equals(PipeInsulationData other) {
            if(ReferenceEquals(null, other)) { return false; }
            if(ReferenceEquals(this, other)) { return true; }

            return SystemName == other.SystemName
                && TypeName == other.TypeName
                && PipeSize == other.PipeSize
                && Name == other.Name
                && Thickness == other.Thickness
                && Length == other.Length
                ;
        }
    }
}
