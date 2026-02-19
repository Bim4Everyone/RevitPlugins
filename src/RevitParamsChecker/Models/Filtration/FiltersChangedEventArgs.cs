using System;
using System.Collections.Generic;

namespace RevitParamsChecker.Models.Filtration;

internal class FiltersChangedEventArgs : EventArgs {
    public FiltersChangedEventArgs(ICollection<Filter> oldFilters, ICollection<Filter> newFilters) {
        OldFilters = oldFilters ?? throw new ArgumentNullException(nameof(oldFilters));
        NewFilters = newFilters ?? throw new ArgumentNullException(nameof(newFilters));
    }

    public ICollection<Filter> OldFilters { get; }

    public ICollection<Filter> NewFilters { get; }
}
