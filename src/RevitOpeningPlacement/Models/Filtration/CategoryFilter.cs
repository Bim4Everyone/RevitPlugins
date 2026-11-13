using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration;

namespace RevitOpeningPlacement.Models.Filtration;

/// <summary>
/// Поисковый набор: название, правила фильтрации и категории элементов.
/// </summary>
internal class CategoryFilter {
    /// <summary>
    /// Конструктор поискового набора
    /// </summary>
    /// <param name="name">Название набора</param>
    /// <param name="filter">Правила фильтрации</param>
    /// <param name="categories">Категории элементов, среди которых происходит поиск</param>
    /// <exception cref="ArgumentException">Исключение, если название пустое</exception>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Исключение, если категории не заданы</exception>
    public CategoryFilter(string name, ILogicalFilter filter, ICollection<BuiltInCategory> categories) {
        if(string.IsNullOrWhiteSpace(name)) {
            throw new ArgumentException(nameof(name));
        }

        if(categories is null) {
            throw new ArgumentNullException(nameof(categories));
        }

        if(categories.Count == 0) {
            throw new ArgumentOutOfRangeException(nameof(categories));
        }

        Name = name;
        Filter = filter ?? throw new ArgumentNullException(nameof(filter));
        Categories = categories;
    }

    /// <summary>
    /// Название поискового набора
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Правила фильтрации элементов
    /// </summary>
    public ILogicalFilter Filter { get; }

    /// <summary>
    /// Категории элементов, среди которых происходит поиск
    /// </summary>
    public ICollection<BuiltInCategory> Categories { get; }

    /// <summary>
    /// Возвращает фильтр элементов Revit по правилам фильтрации данного набора
    /// </summary>
    /// <param name="doc">Документ, в котором происходит фильтрация</param>
    /// <param name="options">Настройки построения фильтра</param>
    public ElementFilter Build(Document doc, Bim4Everyone.RevitFiltration.Options options) {
        return Filter.Build(doc, options);
    }
}
