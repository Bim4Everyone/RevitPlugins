using System.ComponentModel;

namespace RevitSleeves.Models.Navigator;
internal enum SleeveStatus {
    [Description("Ошибка обработки геометрии")]
    Invalid,
    [Description("Пустая гильза")]
    Empty,
    [Description("Вне конструкции")]
    OutsideOfStructure,
    [Description("В недопустимых конструкциях")]
    UnacceptableConstructions,
    [Description("В разных конструкциях")]
    DifferentConstructions,
    [Description("Не соответствует отверстию")]
    BeyondOpening,
    [Description("Пересечение с двумя и более элементами ВИС")]
    MultipleMepElements,
    [Description("Неактуальная гильза")]
    IrrelevantSleeve,
    [Description("Большой диаметр")]
    TooBigDiameter,
    [Description("Маленький диаметр")]
    TooSmallDiameter,
    [Description("Торец гильзы далеко от конструкции")]
    EndFaceFarAwayFromStructure,
    [Description("Торец гильзы внутри конструкции")]
    EndFaceInsideStructure,
    [Description("Ось гильзы не параллельна оси трубы")]
    AxisNotParallelToMepElement,
    [Description("Ось гильзы смещена от оси трубы")]
    AxisDistanceTooBig,
    [Description("Пересекающиеся гильзы")]
    IntersectSleeve,
    [Description("Актуальная гильза")]
    Correct
}
