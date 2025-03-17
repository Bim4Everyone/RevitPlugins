using System;
using System.Collections.Generic;

using RevitClashDetective.Models.Clashes;

namespace RevitClashDetective.Models.RevitClashReport {
    internal class ReportModel : IEquatable<ReportModel> {
        public ReportModel(string name, IEnumerable<ClashModel> clashes) {
            if(string.IsNullOrWhiteSpace(name)) {
                throw new ArgumentException(nameof(name));
            }

            Name = name;
            Clashes = clashes ?? throw new ArgumentNullException(nameof(clashes));
        }


        public string Name { get; }
        public IEnumerable<ClashModel> Clashes { get; set; }


        public override bool Equals(object obj) {
            return Equals(obj as ReportModel);
        }

        public override int GetHashCode() {
            return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
        }

        public bool Equals(ReportModel other) {
            if(ReferenceEquals(null, other)) { return false; }
            if(ReferenceEquals(this, other)) { return true; }

            return Equals(Name, other.Name);
        }
    }
}
