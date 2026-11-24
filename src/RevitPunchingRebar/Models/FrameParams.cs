using System;

using RevitPunchingRebar.Models.Interfaces;

namespace RevitPunchingRebar.Models;

internal class FrameParams : IFrameParams {
    // Шаг округления расстояний зоны продавливания, мм
    private const double _roundingStepMm = 10;

    // Ширина зоны продавливания в долях рабочей высоты сечения
    private const double _punchingZoneFactor = 1.5;

    // Расстояние от грани пилона до первого хомута: рабочая высота сечения, деленная на это число
    private const double _afterPylonDistanceDivider = 3;

    // Диаметр оправки в долях диаметра хомута
    private const double _stirrupBendFactorA240 = 2.5;
    private const double _stirrupBendFactorA500C = 5;

    private readonly double _slabThickness;
    private readonly double _rebarCoverBottom;

    /// <summary>
    /// Создает параметры каркасов по настройкам пользователя.
    /// </summary>
    /// <param name="settings">Настройки плагина для текущего проекта (размеры в мм).</param>
    /// <param name="hostSlab">Плита, в которой расставляются каркасы.</param>
    public FrameParams(RevitSettings settings, Slab hostSlab) {
        ParameterSettings = FrameParameterSettings.CopyWithDefaults(settings.ParameterSettings);
        FrameFamilyName = settings.FamilyName;
        FrameFamilyTypeName = settings.FamilyType;
        SwapFrameElevations = settings.SwapFrameElevations;

        HostSlab = hostSlab;
        _slabThickness = hostSlab.Thickness;

        // В режиме "из плиты" берется защитный слой плиты, иначе - введенный вручную
        RebarCoverTop = settings.IsRebarCoverFromUser
            ? UnitUtilsHelper.FromMm(settings.RebarCoverTop)
            : hostSlab.RebarCoverTop.Value;
        _rebarCoverBottom = settings.IsRebarCoverFromUser
            ? UnitUtilsHelper.FromMm(settings.RebarCoverBottom)
            : hostSlab.RebarCoverBottom.Value;
        PlateRebarDiameter = UnitUtilsHelper.FromMm(settings.SlabRebarDiameter);

        StirrupRebarClass = settings.StirrupRebarClass ?? DefaultSettings.StirrupRebarClass;
        StirrupRebarDiameter = UnitUtilsHelper.FromMm(settings.StirrupDiameter);
        StirrupStep = UnitUtilsHelper.FromMm(settings.StirrupStep);

        WidthStep = UnitUtilsHelper.FromMm(settings.GetWidthStepMm());
        LongRebarDiameter = UnitUtilsHelper.FromMm(settings.LongRebarDiameter ?? DefaultSettings.LongRebarDiameterMm);
        double stirrupBendFactor = StirrupRebarClass == RebarClass.A240 ? _stirrupBendFactorA240 : _stirrupBendFactorA500C;
        double stirrupBendDiameter = stirrupBendFactor * StirrupRebarDiameter;
        FrameWidth = WidthStep + LongRebarDiameter + 2 * StirrupRebarDiameter + stirrupBendDiameter;
        LongRebarAnchorageLength = UnitUtilsHelper.FromMm(
            settings.LongRebarAnchorageLength ?? DefaultSettings.LongRebarAnchorageLengthMm);
        StirrupBendLength = UnitUtilsHelper.FromMm(
            settings.StirrupBendLength ?? DefaultSettings.StirrupBendLengthMm);
    }

    public FrameParameterSettings ParameterSettings { get; }

    public string FrameFamilyName { get; }
    public string FrameFamilyTypeName { get; }
    public bool SwapFrameElevations { get; }

    public Slab HostSlab { get; }

    public double RebarCoverTop { get; }

    public double PlateRebarDiameter { get; }

    public RebarClass StirrupRebarClass { get; }
    public double StirrupRebarDiameter { get; }
    public double StirrupStep { get; }
    public double WidthStep { get; }
    public double FrameWidth { get; }

    public double LongRebarDiameter { get; }
    public double LongRebarAnchorageLength { get; }
    public double StirrupBendLength { get; }

    public double GetFrameHeight() {
        return _slabThickness - (RebarCoverTop + PlateRebarDiameter)
            - (_rebarCoverBottom + 2 * PlateRebarDiameter) - LongRebarDiameter;
    }

    public double GetFrameLength() {
        double afterColumnDistance = GetAfterPylonDistance();
        double punchingZoneLength = GetPunchingZone();
        double punchingZoneLengthRounded = UnitUtilsHelper.CeilingToMm(punchingZoneLength, _roundingStepMm);
        return Math.Ceiling((punchingZoneLengthRounded - afterColumnDistance) / StirrupStep) * StirrupStep;
    }

    /// <summary>
    /// Находит расстояние от грани колонны до первого хомута каркаса
    /// </summary>
    public double GetAfterPylonDistance() {
        double workingHeight = GetWorkingHeight();
        double afterColumnDistance = workingHeight / _afterPylonDistanceDivider;
        return UnitUtilsHelper.CeilingToMm(afterColumnDistance, _roundingStepMm);
    }

    /// <summary>
    /// Находит размер зоны продавливания (расстояние от грани пилона до 1,5h0)
    /// </summary>
    public double GetPunchingZone() {
        double workingHeight = GetWorkingHeight();
        return UnitUtilsHelper.CeilingToMm(_punchingZoneFactor * workingHeight, _roundingStepMm);
    }

    /// <summary>
    /// Находит рабочую высоту сечения фундамента (h0)
    /// </summary>
    private double GetWorkingHeight() {
        return _slabThickness - (_rebarCoverBottom + PlateRebarDiameter);
    }
}
