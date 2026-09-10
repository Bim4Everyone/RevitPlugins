namespace RevitClassifierParameters.Models.ConcreteParams;

/// <summary>
/// Результат разбора имени типоразмера железобетонного элемента:
/// марки бетона B, F, W и строковое значение типа материала.
/// </summary>
internal readonly struct ConcreteMarks {
    public ConcreteMarks(double markB, double markF, double markW, string materialType) {
        MarkB = markB;
        MarkF = markF;
        MarkW = markW;
        MaterialType = materialType;
    }

    /// <summary>
    /// Значение марки бетона B (параметр "обр_ФОП_Марка бетона B").
    /// </summary>
    public double MarkB { get; }

    /// <summary>
    /// Значение марки бетона F (параметр "обр_ФОП_Марка бетона F").
    /// </summary>
    public double MarkF { get; }

    /// <summary>
    /// Значение марки бетона W (параметр "обр_ФОП_Марка бетона W").
    /// </summary>
    public double MarkW { get; }

    /// <summary>
    /// Значение типа материала (параметр "ФОП_ТИП_Тип материала"),
    /// например "B30" или "B7.5".
    /// </summary>
    public string MaterialType { get; }
}
