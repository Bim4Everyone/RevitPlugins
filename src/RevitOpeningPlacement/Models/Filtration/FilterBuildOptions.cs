using Bim4Everyone.RevitFiltration;

namespace RevitOpeningPlacement.Models.Filtration;
/// <summary>
/// Настройки построения фильтра элементов по поисковому набору
/// </summary>
internal static class FilterBuildOptions {
    /// <summary>
    /// Создает настройки построения фильтра
    /// </summary>
    public static Options Create() {
        return new Options() { Tolerance = 0.001, FilterByType = false };
    }

    /// <summary>
    /// Создает настройки построения инвертированного фильтра
    /// </summary>
    public static Options CreateInverted() {
        return new Options() { Tolerance = 0.001, FilterByType = false, Inverted = true };
    }
}
