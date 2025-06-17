namespace RevitSleeves.Models.Config;
/// <summary>
/// Расстояния между элементами и гильзами
/// </summary>
internal enum OffsetType {
    /// <summary>
    /// Отступ от верхней грани перекрытия до верхнего торца гильзы
    /// </summary>
    FromSleeveEndToTopFloorFace,
    /// <summary>
    /// Расстояние между осью элемента ВИС и осью гильзы
    /// </summary>
    FromSleeveAxisToMepAxis
}
