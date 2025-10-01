using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using RevitClashDetective.Models.Clashes;

namespace RevitClashDetective.Models.RevitClashReport;
internal class ReportModel : IEquatable<ReportModel> {
    public ReportModel(string name, IEnumerable<ClashModel> clashes) {
        if(string.IsNullOrWhiteSpace(name)) {
            throw new ArgumentException(nameof(name));
        }
        if(clashes is null) {
            throw new ArgumentNullException(nameof(clashes));
        }

        Name = name;
        Clashes = new ReadOnlyCollection<ClashModel>(clashes.ToArray());
    }


    public string Name { get; }
    public IReadOnlyCollection<ClashModel> Clashes { get; set; }


    public override bool Equals(object obj) {
        return Equals(obj as ReportModel);
    }

    public override int GetHashCode() {
        return 539060726 + EqualityComparer<string>.Default.GetHashCode(Name);
    }

    public bool Equals(ReportModel other) {
        return other is not null && (ReferenceEquals(this, other) || Equals(Name, other.Name));
    }
}
