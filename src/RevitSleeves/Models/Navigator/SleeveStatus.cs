using System.ComponentModel;

namespace RevitSleeves.Models.Navigator;
internal enum SleeveStatus {
    /// <summary>Ошибка обработки геометрии</summary>
    [Description("Ошибка обработки геометрии")]
    Invalid,
    /// <summary>Пустая гильза</summary>
    [Description("Пустая гильза")]
    Empty,
    /// <summary>Вне конструкции</summary>
    [Description("Вне конструкции")]
    OutsideOfStructure,
    /// <summary>В недопустимых конструкциях</summary>
    [Description("В недопустимых конструкциях")]
    UnacceptableConstructions,
    /// <summary>В разных конструкциях</summary>
    [Description("В разных конструкциях")]
    DifferentConstructions,
    /// <summary>Не соответствует отверстию</summary>
    [Description("Не соответствует отверстию")]
    BeyondOpening,
    /// <summary>Пересечение с двумя и более элементами ВИС</summary>
    [Description("Пересечение с двумя и более элементами ВИС")]
    MultipleMepElements,
    /// <summary>Неактуальная гильза</summary>
    [Description("Неактуальная гильза")]
    Irrelevant,
    /// <summary>Большой диаметр</summary>
    [Description("Большой диаметр")]
    TooBigDiameter,
    /// <summary>Маленький диаметр</summary>
    [Description("Маленький диаметр")]
    TooSmallDiameter,
    /// <summary>Диаметр не задан в настройках</summary>
    [Description("Диаметр не задан в настройках")]
    DiameterNotFound,
    /// <summary>Торец гильзы далеко от конструкции</summary>
    [Description("Торец гильзы далеко от конструкции")]
    EndFaceFarAwayFromStructure,
    /// <summary>Торец гильзы внутри конструкции</summary>
    [Description("Торец гильзы внутри конструкции")]
    EndFaceInsideStructure,
    /// <summary>Ось гильзы не параллельна оси трубы</summary>
    [Description("Ось гильзы не параллельна оси трубы")]
    AxisNotParallelToMepElement,
    /// <summary>Ось гильзы смещена от оси трубы</summary>
    [Description("Ось гильзы смещена от оси трубы")]
    AxisDistanceTooBig,
    /// <summary>Пересекающиеся гильзы</summary>
    [Description("Пересекающиеся гильзы")]
    IntersectSleeve,
    /// <summary>Актуальная гильза</summary>
    [Description("Актуальная гильза")]
    Correct
}
