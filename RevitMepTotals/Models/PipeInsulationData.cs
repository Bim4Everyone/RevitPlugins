using System;

using RevitMepTotals.Models.Interfaces;

namespace RevitMepTotals.Models {
    internal class PipeInsulationData : IPipeInsulationData, IEquatable<PipeInsulationData> {
        public PipeInsulationData(string typeName, string pipeSize, string name) {
            if(string.IsNullOrWhiteSpace(typeName)) { throw new ArgumentException(nameof(typeName)); }
            if(string.IsNullOrWhiteSpace(pipeSize)) { throw new ArgumentException(nameof(pipeSize)); }
            if(string.IsNullOrWhiteSpace(name)) { throw new ArgumentException(nameof(name)); }

            TypeName = typeName;
            PipeSize = pipeSize;
            Name = name;
        }


        public string TypeName { get; }

        public string PipeSize { get; }

        public string Name { get; }

        public double Thickness { get; set; }

        public double Length { get; set; }

        public double Area { get; set; }


        public override bool Equals(object obj) {
            return (obj != null) && (obj is PipeInsulationData other) && Equals(other);
        }

        public override int GetHashCode() {
            return (TypeName + PipeSize + Name).GetHashCode();
        }

        public bool Equals(PipeInsulationData other) {
            return (other != null)
                && string.Equals(TypeName, other.TypeName, StringComparison.CurrentCultureIgnoreCase)
                && string.Equals(PipeSize, other.PipeSize, StringComparison.CurrentCultureIgnoreCase)
                && string.Equals(Name, other.Name, StringComparison.CurrentCultureIgnoreCase)
                ;
        }
    }
}
