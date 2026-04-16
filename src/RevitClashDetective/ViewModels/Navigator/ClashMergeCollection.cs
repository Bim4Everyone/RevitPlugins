using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RevitClashDetective.ViewModels.Navigator;

internal class ClashMergeCollection {
    public ClashMergeCollection(string name, IEnumerable<ClashMergePairViewModel> items) {
        if(items == null) {
            throw new ArgumentNullException(nameof(items));
        }

        if(string.IsNullOrWhiteSpace(name)) {
            throw new ArgumentException(nameof(name));
        }

        Name = name;
        Items = new ReadOnlyCollection<ClashMergePairViewModel>(items.ToArray());
    }

    public string Name { get; }

    /// <summary>
    /// Коллизии, которые будут видны пользователю
    /// </summary>
    public ICollection<ClashMergePairViewModel> Items { get; }
}
