using System;

using Bim4Everyone.RevitFiltration.Controls;

namespace RevitClashDetective.Models.Filtration;

/// <summary>
/// Поисковый набор: название и контекст фильтра
/// </summary>
internal class NamedFilterContext {
    public NamedFilterContext(string name, ILogicalFilterContext context) {
        if(string.IsNullOrWhiteSpace(name)) {
            throw new ArgumentException(nameof(name));
        }

        Name = name;
        Context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public string Name { get; }

    public ILogicalFilterContext Context { get; }
}
