using Autodesk.Revit.DB;

namespace RevitStairRailing.Models;

/// <summary>
/// Параметры ограждения из окна плагина. Размеры — в миллиметрах.
/// </summary>
internal class RailingParameters {
    /// <summary>
    /// Тип стоек.
    /// </summary>
    public ElementId PostTypeId { get; set; }

    /// <summary>
    /// Тип поручня.
    /// </summary>
    public ElementId HandrailTypeId { get; set; }

    /// <summary>
    /// Тип продольных прутков.
    /// </summary>
    public ElementId RodTypeId { get; set; }

    /// <summary>
    /// Тип опорных пластин; null — пластины не ставятся.
    /// </summary>
    public ElementId PlateTypeId { get; set; }

    /// <summary>
    /// Тип анкеров; null — анкеры не ставятся.
    /// </summary>
    public ElementId AnchorTypeId { get; set; }

    /// <summary>
    /// Высота стоек от траектории до оси поручня.
    /// </summary>
    public double PostHeight { get; set; }

    /// <summary>
    /// Наибольший шаг стоек в плане: стойки расставляются равномерно с шагом не больше этого.
    /// </summary>
    public double MaxPostStep { get; set; }

    /// <summary>
    /// Угол поворота поперечного сечения стоек, градусы.
    /// </summary>
    public double PostRotationDegrees { get; set; }

    /// <summary>
    /// Расстояния от оси поручня вниз до осей продольных прутков.
    /// </summary>
    public List<double> RodOffsets { get; set; } = new();

    /// <summary>
    /// Длина опорной пластины вдоль ограждения.
    /// </summary>
    public double PlateLength { get; set; }

    /// <summary>
    /// Длина анкера вниз от низа стойки.
    /// </summary>
    public double AnchorLength { get; set; }

    /// <summary>
    /// Сторона ограждения относительно линий траектории по ходу подъёма.
    /// </summary>
    public RailingSide Side { get; set; }

    /// <summary>
    /// Отступ оси ограждения от линий траектории (края марша) в плане.
    /// </summary>
    public double EdgeOffset { get; set; }

    /// <summary>
    /// Собирать построенные элементы в сборку.
    /// </summary>
    public bool CreateAssembly { get; set; }

    /// <summary>
    /// Начало имени сборки; к нему добавляется следующий свободный номер.
    /// </summary>
    public string AssemblyNamePrefix { get; set; }
}

/// <summary>
/// Сторона ограждения относительно траектории по ходу подъёма.
/// </summary>
internal enum RailingSide {
    Left,
    Right
}
