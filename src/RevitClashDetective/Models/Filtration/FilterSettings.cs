namespace RevitClashDetective.Models.Filtration;
/// <summary>
/// Настройки поискового набора на основе Bim4Everyone.RevitFiltration
/// </summary>
internal class FilterSettings {
    /// <summary>
    /// Название поискового набора
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Сериализованный контекст фильтра
    /// (<see cref="Bim4Everyone.RevitFiltration.Controls.ILogicalFilterContext"/>)
    /// </summary>
    public string FilterContext { get; set; }
}
