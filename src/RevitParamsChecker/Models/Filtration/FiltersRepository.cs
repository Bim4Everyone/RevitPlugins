using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace RevitParamsChecker.Models.Filtration;

internal class FiltersRepository {
    private readonly FiltersConfig _filtersConfig;

    public FiltersRepository(FiltersConfig filtersConfig) {
        _filtersConfig = filtersConfig ?? throw new ArgumentNullException(nameof(filtersConfig));
    }

    public event EventHandler<FiltersChangedEventArgs> FiltersChanged;

    public void SetFilters(ICollection<Filter> newFilters) {
        var oldFilters = _filtersConfig.Filters;
        _filtersConfig.Filters = newFilters?.ToArray() ?? throw new ArgumentNullException(nameof(newFilters));
        _filtersConfig.SaveProjectConfig();
        FiltersChanged?.Invoke(this, new FiltersChangedEventArgs(oldFilters, newFilters));
    }

    public ICollection<Filter> GetFilters() {
        return new ReadOnlyCollection<Filter>(_filtersConfig.Filters);
    }

    public Filter GetFilter(string filterName) {
        return _filtersConfig.Filters.FirstOrDefault(f => f.Name.Equals(
            filterName,
            StringComparison.CurrentCultureIgnoreCase));
    }
}
