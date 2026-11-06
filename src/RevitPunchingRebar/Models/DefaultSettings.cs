namespace RevitPunchingRebar.Models;

/// <summary>
/// Значения настроек по умолчанию.
/// </summary>
/// <remarks>
/// Используются при первом запуске в проекте, для полей, которых нет в старом конфиге,
/// и при сбросе поля к значению по умолчанию.
/// </remarks>
internal static class DefaultSettings {
    /// <summary>
    /// Диаметр фоновой арматуры плиты, мм.
    /// </summary>
    public const int SlabRebarDiameterMm = 10;

    /// <summary>
    /// Защитный слой сверху, мм.
    /// </summary>
    public const int RebarCoverTopMm = 20;

    /// <summary>
    /// Защитный слой снизу, мм.
    /// </summary>
    public const int RebarCoverBottomMm = 40;

    /// <summary>
    /// Семейство каркаса.
    /// </summary>
    public const string FrameFamilyName = "IFC_Каркас_!КГор_№1";

    /// <summary>
    /// Тип каркаса.
    /// </summary>
    public const string FrameFamilyTypeName = "Х_1501";

    /// <summary>
    /// Диаметр хомутов, мм.
    /// </summary>
    public const int StirrupDiameterMm = 10;

    /// <summary>
    /// Класс арматуры хомутов.
    /// </summary>
    public const RebarClass StirrupRebarClass = RebarClass.A500C;

    /// <summary>
    /// Шаг хомутов, мм.
    /// </summary>
    public const int StirrupStepMm = 200;

    /// <summary>
    /// Ширина каркаса, мм.
    /// </summary>
    public const int FrameWidthMm = 200;

    /// <summary>
    /// Длина отгибов хомутов, мм.
    /// </summary>
    public const int StirrupBendLengthMm = 90;

    /// <summary>
    /// Диаметр продольных стержней каркаса, мм.
    /// </summary>
    public const int LongRebarDiameterMm = 10;

    /// <summary>
    /// Длина выпуска продольных стержней за крайние хомуты, мм.
    /// </summary>
    public const int LongRebarAnchorageLengthMm = 50;
}
