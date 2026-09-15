namespace RevitParamsChecker.Models.Filtration;

internal class Filter {
    public Filter() {
    }

    public string Name { get; set; }

    /// <summary>
    /// Сериализованный контекст фильтра по параметрам элементов.
    /// Пустое значение означает, что фильтрация не задана.
    /// </summary>
    public string FilterContext { get; set; }

    /// <summary>
    /// Сериализованный контекст фильтра по параметрам материалов элементов.
    /// Пустое значение означает, что фильтрация по материалам не задана.
    /// </summary>
    public string MaterialsFilterContext { get; set; }
}
