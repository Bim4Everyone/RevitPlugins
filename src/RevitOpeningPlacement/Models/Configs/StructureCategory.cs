namespace RevitOpeningPlacement.Models.Configs;
internal class StructureCategory {
    public bool IsSelected { get; set; }
    public string Name { get; set; }
    /// <summary>
    /// Сериализованный контекст фильтра элементов данной категории
    /// (<see cref="Bim4Everyone.RevitFiltration.Controls.ILogicalFilterContext"/>)
    /// </summary>
    public string FilterContext { get; set; }
}
