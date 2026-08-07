namespace RevitClassifierParameters.Models.FacadeType;

/// <summary>
/// Строка правил заполнения типа фасада, прочитанная из Excel-файла.
/// </summary>
public class FacadeTypeItem {
    /// <summary>
    /// Характеристика функции (столбец 1) хранится со скобками, например "(ФМ)".
    /// </summary>
    public string FunctionCharacteristic { get; set; }

    /// <summary>
    /// Сокращение основного материала (столбец 2), например "ОК".
    /// </summary>
    public string MaterialAbbreviation { get; set; }

    /// <summary>
    /// Значение (столбец 3), которое записывается в параметр стены.
    /// </summary>
    public string Value { get; set; }
}
