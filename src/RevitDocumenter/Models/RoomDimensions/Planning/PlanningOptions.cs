using System.Collections.Generic;

using RevitDocumenter.Models.RoomDimensions.Geometry;

namespace RevitDocumenter.Models.RoomDimensions.Planning;

/// <summary>
/// Параметры планирования размеров для одного помещения. Все значения в футах модели.
/// </summary>
internal class PlanningOptions {
    /// <summary>
    /// Открытые дверные полотна поблизости от помещения (D77). Дуги открывания сюда не входят.
    /// </summary>
    public IReadOnlyList<DoorLeafObstacle> DoorLeaves { get; set; } = [];

    /// <summary>
    /// Кривые дуг открывания поблизости от помещения (D79), без заполнения сектора.
    /// </summary>
    public IReadOnlyList<DoorOpeningArcObstacle> DoorOpeningArcs { get; set; } = [];

    /// <summary>
    /// Защитный запас вокруг модели текста только для дверей (D79), не калибровка шрифта Revit.
    /// </summary>
    public double DoorTextClearance => TextSize * 0.5;

    /// <summary>
    /// Отступ размерной линии от контура.
    /// </summary>
    public double Offset { get; set; }

    /// <summary>
    /// Добавка к отступу, если текст размера обращен к контуру.
    /// </summary>
    public double TextAllowance { get; set; }

    /// <summary>
    /// Высота текста размера (модель текста, D62).
    /// </summary>
    public double TextSize { get; set; }

    /// <summary>
    /// Зазор текста от размерной линии.
    /// </summary>
    public double TextGap { get; set; }

    /// <summary>
    /// Коэффициент ширины текста типоразмера.
    /// </summary>
    public double TextWidthScale { get; set; } = 1;

    /// <summary>
    /// Допуск совпадения плоскостей привязки и равенства значений.
    /// </summary>
    public double CoordinateTolerance { get; set; }

    /// <summary>
    /// Объединять размеры с общими плоскостями в цепочки.
    /// </summary>
    public bool MergeChains { get; set; } = true;

    /// <summary>
    /// Максимальная длина вспомогательной линии при объединении размеров в цепочку.
    /// </summary>
    public double MaxWitnessLine { get; set; }

    /// <summary>
    /// Шаг между ярусами размеров.
    /// </summary>
    public double TierStep { get; set; }

    /// <summary>
    /// Общий размер ставится, если в самой длинной цепочке вдоль оси больше этого числа сегментов.
    /// </summary>
    public int OverallMinSegments { get; set; }

    /// <summary>
    /// Дубль удаляется, только если размерные линии ближе этого расстояния. 0 - на любом расстоянии.
    /// </summary>
    public double DuplicateMaxDistance { get; set; }

    /// <summary>
    /// Одинаковые собственные размеры разных пилонов считать дублями (остается один на ось на все пилоны).
    /// false - у каждого пилона свой размер, повторы между пилонами решает удаление дублей после цепочек.
    /// </summary>
    public bool PylonDuplicatesAcrossPylons { get; set; }
}
