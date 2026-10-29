using System.Collections.Generic;

namespace RevitDocumenter.Models.RoomDimensions.Settings;

/// <summary>
/// Настройки образмеривания помещений, читаемые из JSON-файла.
/// </summary>
internal class RoomDimensioningSettings {
    /// <summary>
    /// Имя параметра помещения, по точному значению которого определяется тип помещения.
    /// </summary>
    public string PurposeParameterName { get; set; } = "Назначение";

    /// <summary>
    /// Общие параметры раскладки размеров на виде.
    /// </summary>
    public LayoutSettings Layout { get; set; } = new();

    /// <summary>
    /// Типы помещений с собственными порогами и типоразмером размера.
    /// </summary>
    public List<RoomTypeSettings> RoomTypes { get; set; } = [];

    /// <summary>
    /// Настройки отладочного вывода.
    /// </summary>
    public DebugSettings Debug { get; set; } = new();
}

/// <summary>
/// Параметры раскладки размеров. Значения в мм на печати, при построении умножаются на масштаб вида.
/// </summary>
internal class LayoutSettings {
    /// <summary>
    /// Отступ первого яруса размеров от контура помещения.
    /// </summary>
    public double FirstTierOffsetPrintMm { get; set; } = 8;

    /// <summary>
    /// Шаг между ярусами размеров.
    /// </summary>
    public double TierStepPrintMm { get; set; } = 7;

    /// <summary>
    /// Максимальный сдвиг размера при объединении размеров в одну линию.
    /// </summary>
    public double MaxShiftPrintMm { get; set; } = 10;

    /// <summary>
    /// Максимальная длина выноски размера.
    /// </summary>
    public double MaxLeaderPrintMm { get; set; } = 15;
}

/// <summary>
/// Настройки типа помещения. Длины в мм модели.
/// </summary>
internal class RoomTypeSettings {
    /// <summary>
    /// Отображаемое имя типа помещения.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Точное значение параметра назначения, по которому помещение относится к этому типу.
    /// </summary>
    public string PurposeValue { get; set; }

    /// <summary>
    /// Имя типоразмера линейного размера, которым образмериваются помещения этого типа.
    /// </summary>
    public string DimensionTypeName { get; set; }

    /// <summary>
    /// Граница помещения, по которой строится контур: Finish, Center, CoreBoundary, CoreCenter.
    /// </summary>
    public string BoundaryLocation { get; set; } = "Finish";

    /// <summary>
    /// Отступ между параллельными линиями контура, меньше которого линии сращиваются.
    /// </summary>
    public double MergeOffsetMm { get; set; } = 10;

    /// <summary>
    /// Минимальная длина линии контура, которая образмеривается.
    /// </summary>
    public double MinLineLengthMm { get; set; } = 10;

    /// <summary>
    /// Допуск равенства значений размеров.
    /// </summary>
    public double EqualityToleranceMm { get; set; } = 0.5;

    /// <summary>
    /// Общий размер ставится, если в цепочке вдоль оси больше этого числа сегментов.
    /// </summary>
    public int OverallDimMinSegments { get; set; } = 4;
}

/// <summary>
/// Настройки отладочного вывода.
/// </summary>
internal class DebugSettings {
    /// <summary>
    /// Режим отладки: отладочный контур в Revit (отдельной транзакцией), журнал и изображение вида.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Папка для журнала и изображения. Если не задана - папка файла настроек.
    /// </summary>
    public string OutputFolder { get; set; }
}
