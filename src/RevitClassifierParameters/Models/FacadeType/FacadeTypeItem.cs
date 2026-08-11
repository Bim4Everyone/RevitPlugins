namespace RevitClassifierParameters.Models.FacadeType;

/// <summary>
/// Строка правил заполнения типа фасада, прочитанная из Excel-файла.
/// </summary>
internal class FacadeTypeItem {
    /// <summary>
    /// Характеристика функции (столбец 1) хранится со скобками, например "(ФМ)".
    /// Приводится к нормализованному виду (Trim + верхний регистр) при чтении из Excel.
    /// </summary>
    public string FunctionCharacteristic { get; set; }

    /// <summary>
    /// Сокращение основного материала (столбец 2), например "ОК".
    /// Приводится к нормализованному виду (Trim + верхний регистр) при чтении из Excel.
    /// </summary>
    public string MaterialAbbreviation { get; set; }

    /// <summary>
    /// Значение (столбец 3), которое записывается в параметр стены.
    /// </summary>
    public string Value { get; set; }
}
