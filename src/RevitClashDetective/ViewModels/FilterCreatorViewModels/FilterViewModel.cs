using System;
using System.Collections.Generic;

using Bim4Everyone.RevitFiltration.Controls;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.ViewModels.FilterCreatorViewModels;
internal class FilterViewModel : BaseViewModel, IEquatable<FilterViewModel>, INamedEntity {
    private readonly string _id;
    private string _name;

    public FilterViewModel(string name, ILogicalFilterProvider filterProvider) {
        FilterProvider = filterProvider ?? throw new ArgumentNullException(nameof(filterProvider));
        _id = Guid.NewGuid().ToString();
        Name = name;
    }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    /// <summary>
    /// Провайдер контекста фильтра, который пользователь настраивает в UI
    /// </summary>
    public ILogicalFilterProvider FilterProvider { get; }

    public override bool Equals(object obj) {
        return Equals(obj as FilterViewModel);
    }

    public override int GetHashCode() {
        return 539060726 + EqualityComparer<string>.Default.GetHashCode(_id);
    }

    public bool Equals(FilterViewModel other) {
        if(other is null) { return false; }
        return ReferenceEquals(this, other) || _id == other._id;
    }
}
