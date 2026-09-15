using System;
using System.ComponentModel;

using Bim4Everyone.RevitFiltration.Controls;

using dosymep.WPF.ViewModels;

using RevitParamsChecker.Models;

namespace RevitParamsChecker.ViewModels.Filtration;

internal class FilterViewModel : BaseViewModel, IEquatable<FilterViewModel>, IName {
    private readonly Guid _guid;
    private readonly IFilterContextParser _filterContextParser;
    private string _savedFilterContext;
    private string _savedMaterialsFilterContext;
    private string _name;
    private bool _modified;

    public FilterViewModel(
        string name,
        ILogicalFilterProvider filterProvider,
        ILogicalFilterProvider materialsFilterProvider,
        IFilterContextParser filterContextParser) {
        if(string.IsNullOrWhiteSpace(name)) {
            throw new ArgumentException(nameof(name));
        }

        FilterProvider = filterProvider ?? throw new ArgumentNullException(nameof(filterProvider));
        MaterialsFilterProvider = materialsFilterProvider
                                  ?? throw new ArgumentNullException(nameof(materialsFilterProvider));
        _filterContextParser = filterContextParser ?? throw new ArgumentNullException(nameof(filterContextParser));
        _savedFilterContext = GetContext(FilterProvider);
        _savedMaterialsFilterContext = GetContext(MaterialsFilterProvider);
        FilterProvider.FilterContextChanged += OnFilterContextChanged;
        MaterialsFilterProvider.FilterContextChanged += OnFilterContextChanged;
        Name = name;
        Modified = true;
        _guid = Guid.NewGuid();
        PropertyChanged += OnModelPropertyChanged;
    }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public bool Modified {
        get => _modified;
        set => RaiseAndSetIfChanged(ref _modified, value);
    }

    /// <summary>
    /// Провайдер фильтра по параметрам элементов.
    /// </summary>
    public ILogicalFilterProvider FilterProvider { get; }

    /// <summary>
    /// Провайдер фильтра по параметрам материалов элементов.
    /// </summary>
    public ILogicalFilterProvider MaterialsFilterProvider { get; }

    /// <summary>
    /// Запоминает текущее состояние фильтров как сохраненное и снимает пометку об изменениях.
    /// </summary>
    public void AcceptChanges() {
        _savedFilterContext = GetContext(FilterProvider);
        _savedMaterialsFilterContext = GetContext(MaterialsFilterProvider);
        Modified = false;
    }

    public override bool Equals(object obj) {
        return Equals(obj as FilterViewModel);
    }

    public override int GetHashCode() {
        return _guid.GetHashCode();
    }

    public bool Equals(FilterViewModel other) {
        if(ReferenceEquals(other, null)) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return _guid == other._guid;
    }

    /// <summary>
    /// Помечает набор измененным, если фильтр действительно отличается от сохраненного.
    /// </summary>
    /// <remarks>
    /// Контрол пересобирает контекст фильтра при загрузке провайдера, то есть при каждом выборе набора в списке,
    /// поэтому одного факта события недостаточно: состояния сравниваются в том же виде, в котором сохраняются.
    /// </remarks>
    private void OnFilterContextChanged(object sender, FilterContextChangedEventArgs e) {
        if(IsFilterChanged()) {
            Modified = true;
        }
    }

    private bool IsFilterChanged() {
        return !string.Equals(
                   _savedFilterContext,
                   GetContext(FilterProvider),
                   StringComparison.CurrentCultureIgnoreCase)
               || !string.Equals(
                   _savedMaterialsFilterContext,
                   GetContext(MaterialsFilterProvider),
                   StringComparison.CurrentCultureIgnoreCase);
    }

    /// <summary>
    /// Возвращает сериализованный контекст фильтра либо null, если фильтр задан некорректно.
    /// </summary>
    private string GetContext(ILogicalFilterProvider provider) {
        return provider.CanGetFilter(out _)
            ? _filterContextParser.Serialize(provider.GetFilter())
            : null;
    }

    private void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(Name)) {
            Modified = true;
        }
    }
}
