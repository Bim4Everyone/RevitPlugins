using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RevitClashDetective.ViewModels.Navigator;

internal class ClashMergeCollection {
    public ClashMergeCollection(string name, IEnumerable<ClashMergeViewModel> items) {
        if(items == null) {
            throw new ArgumentNullException(nameof(items));
        }

        if(string.IsNullOrWhiteSpace(name)) {
            throw new ArgumentException(nameof(name));
        }

        Name = name;
        Items = new ReadOnlyCollection<ClashMergeViewModel>(items.ToArray());
    }

    public string Name { get; }

    public ICollection<ClashMergeViewModel> Items { get; }
}
