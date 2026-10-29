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
    /// Тип продольных прутков; null — прутков нет.
    /// </summary>
    public ElementId RodTypeId { get; set; }

    /// <summary>
    /// Высота от траектории до оси поручня.
    /// </summary>
    public double PostHeight { get; set; }

    /// <summary>
    /// Наибольший шаг стоек в плане: стойки расставляются равномерно с шагом не больше этого.
    /// </summary>
    public double MaxPostStep { get; set; }

    /// <summary>
    /// Угол поворота поперечного сечения стоек относительно направления ограждения, градусы.
    /// </summary>
    public double PostRotationDegrees { get; set; }

    /// <summary>
    /// Расстояния от оси поручня вниз до осей продольных прутков.
    /// </summary>
    public List<double> RodOffsets { get; set; } = new();

    /// <summary>
    /// Крепление стоек на маршах (наклонных участках).
    /// </summary>
    public PostMount FlightMount { get; set; }

    /// <summary>
    /// Крепление стоек на площадках (горизонтальных участках).
    /// </summary>
    public PostMount LandingMount { get; set; }

    /// <summary>
    /// Семейство пластин (по линии, с параметрами типа «t» — толщина и «h» — ширина);
    /// типоразмеры нужных размеров находятся или создаются.
    /// </summary>
    public ElementId PlateFamilyId { get; set; }

    /// <summary>
    /// Опорная пластина под стойкой при креплении сверху.
    /// </summary>
    public PlateSize BasePlate { get; set; }

    /// <summary>
    /// Длина опорной пластины вдоль ограждения.
    /// </summary>
    public double BasePlateLength { get; set; }

    /// <summary>
    /// Тип анкеров опорной пластины; null — анкеры не ставятся.
    /// </summary>
    public ElementId AnchorTypeId { get; set; }

    public AnchorLayout AnchorLayout { get; set; }

    /// <summary>
    /// Расстояние от оси анкера до края опорной пластины.
    /// </summary>
    public double AnchorEdgeDistance { get; set; }

    /// <summary>
    /// Пластины-щёки по бокам стойки при боковом креплении (к закладной детали марша).
    /// </summary>
    public PlateSize Cheek { get; set; }

    public double CheekHeight { get; set; }

    /// <summary>
    /// Зазор между щёками — размер стойки между ними.
    /// </summary>
    public double CheekGap { get; set; }

    public CheekOrientation CheekOrientation { get; set; }

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
/// Толщина и ширина листовой пластины, мм.
/// </summary>
internal class PlateSize {
    public PlateSize(double thickness, double width) {
        Thickness = thickness;
        Width = width;
    }

    public double Thickness { get; }

    public double Width { get; }
}

/// <summary>
/// Сторона ограждения относительно траектории по ходу подъёма.
/// </summary>
internal enum RailingSide {
    Left,
    Right
}

/// <summary>
/// Крепление стойки.
/// </summary>
internal enum PostMount {
    /// <summary>
    /// Сверху: стойка на опорной пластине с анкерами.
    /// </summary>
    Top,

    /// <summary>
    /// Сбоку: стойка между двумя вертикальными пластинами-щёками, приваренными к закладной детали марша.
    /// </summary>
    Side
}

/// <summary>
/// Расстановка анкеров на опорной пластине.
/// </summary>
internal enum AnchorLayout {
    /// <summary>
    /// Два по диагонали.
    /// </summary>
    Diagonal,

    /// <summary>
    /// Три: два у края со стороны лестницы, один у противоположного края посередине.
    /// </summary>
    Triangle,

    /// <summary>
    /// Четыре по углам.
    /// </summary>
    Corners
}

/// <summary>
/// Положение щёк относительно ограждения.
/// </summary>
internal enum CheekOrientation {
    /// <summary>
    /// Щёки в плоскости ограждения: стойка между ними со стороны лестницы и снаружи.
    /// </summary>
    AlongRailing,

    /// <summary>
    /// Щёки поперёк ограждения: стойка между ними по ходу ограждения.
    /// </summary>
    AcrossRailing
}
