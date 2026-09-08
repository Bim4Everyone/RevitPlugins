namespace RevitClashDetective.Models.Filtration;

/// <summary>
/// Настройки построения фильтра элементов по поисковому набору.
/// </summary>
internal static class FilterBuildOptions {
    /// <summary>
    /// Создает настройки построения фильтра.
    /// </summary>
    public static Bim4Everyone.RevitFiltration.Options Create() {
        return new Bim4Everyone.RevitFiltration.Options() {
            Tolerance = 0.001,
            FilterByType = false
        };
    }

    /// <summary>
    /// Создает настройки построения инвертированного фильтра.
    /// </summary>
    public static Bim4Everyone.RevitFiltration.Options CreateInverted() {
        return new Bim4Everyone.RevitFiltration.Options() {
            Tolerance = 0.001,
            FilterByType = false,
            Inverted = true
        };
    }
}
