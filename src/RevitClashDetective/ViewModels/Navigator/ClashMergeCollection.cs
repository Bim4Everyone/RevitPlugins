using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace RevitClashDetective.ViewModels.Navigator;

internal class ClashMergeCollection {
    public ClashMergeCollection(string name, IList<ClashMergeViewModel> items) {
        if(items == null) {
            throw new ArgumentNullException(nameof(items));
        }

        if(string.IsNullOrWhiteSpace(name)) {
            throw new ArgumentException(nameof(name));
        }

        Name = name;
        Items = new ReadOnlyCollection<ClashMergeViewModel>(items);
    }

    public string Name { get; }

    public IReadOnlyCollection<ClashMergeViewModel> Items { get; }
}
