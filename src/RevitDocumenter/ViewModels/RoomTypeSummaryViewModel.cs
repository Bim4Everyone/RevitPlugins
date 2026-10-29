namespace RevitDocumenter.ViewModels;

/// <summary>
/// Строка сводки в окне: тип помещения и сколько помещений этого типа найдено на виде.
/// </summary>
internal class RoomTypeSummaryViewModel {
    public RoomTypeSummaryViewModel(
        string roomTypeName,
        string purposeValue,
        int roomCount,
        string dimensionTypeName,
        bool dimensionTypeFound) {
        RoomTypeName = roomTypeName;
        PurposeValue = purposeValue;
        RoomCount = roomCount;
        DimensionTypeName = dimensionTypeName;
        DimensionTypeFound = dimensionTypeFound;
    }

    /// <summary>
    /// Имя типа помещения из настроек.
    /// </summary>
    public string RoomTypeName { get; }

    /// <summary>
    /// Значение параметра назначения для этого типа.
    /// </summary>
    public string PurposeValue { get; }

    /// <summary>
    /// Количество помещений этого типа на активном виде.
    /// </summary>
    public int RoomCount { get; }

    /// <summary>
    /// Имя типоразмера размера из настроек.
    /// </summary>
    public string DimensionTypeName { get; }

    /// <summary>
    /// Найден ли типоразмер размера в проекте.
    /// </summary>
    public bool DimensionTypeFound { get; }
}
