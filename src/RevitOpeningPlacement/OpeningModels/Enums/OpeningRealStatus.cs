using System.ComponentModel;

namespace RevitOpeningPlacement.OpeningModels.Enums;
/// <summary>
/// Статусы чистовых экземпляров отверстий, которые идут на чертежи, относительно выданных заданий на отверстия 
/// </summary>
internal enum OpeningRealStatus {
    /// <summary>
    /// Нет ни одного элемента задания на отверстие или элемента ВИС, которое пересекается с этим чистовым отверстием
    /// </summary>
    [Description("Пустое отверстие")]
    Empty,
    /// <summary>
    /// Размещенное чистовое отверстие слишком большое
    /// </summary>
    [Description("Слишком большое отверстие")]
    TooBig,
    /// <summary>
    /// Размещенное чистовое отверстие не полностью закрывает задание или элементы ВИС
    /// </summary>
    [Description("Неактуальное отверстие")]
    NotActual,
    /// <summary>
    /// Размещенное чистовое отверстие корректно
    /// </summary>
    [Description("Корректно")]
    Correct,
    /// <summary>
    /// Произошла ошибка обработки геометрии в процессе определения статуса
    /// </summary>
    [Description("Ошибка обработки геометрии")]
    Invalid
}
