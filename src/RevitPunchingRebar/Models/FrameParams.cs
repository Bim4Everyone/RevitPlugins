using System;

using RevitPunchingRebar.Models.Interfaces;

namespace RevitPunchingRebar.Models;
internal class FrameParams : IFrameParams {
    private readonly double _slabThickness;
    private readonly double _rebarCoverBottom;

    // Шаг округления расстояний зоны продавливания, мм
    private const double _roundingStepMm = 10;

    // Ширина зоны продавливания в долях рабочей высоты сечения
    private const double _punchingZoneFactor = 1.5;

    // Расстояние от грани пилона до первого хомута: рабочая высота сечения, деленная на это число
    private const double _afterPylonDistanceDivider = 3;

    /// <summary>
    /// Создает параметры каркасов по настройкам пользователя.
    /// </summary>
    /// <param name="settings">Настройки плагина для текущего проекта (размеры в мм).</param>
    /// <param name="hostSlab">Плита, в которой расставляются каркасы.</param>
    public FrameParams(RevitSettings settings, Slab hostSlab) {
        ParameterSettings = FrameParameterSettings.CopyWithDefaults(settings.ParameterSettings);
        FrameFamilyName = settings.FamilyName;
        FrameFamilyTypeName = settings.FamilyType;

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
        FrameWidth = UnitUtilsHelper.FromMm(settings.FrameWidth);

        LongRebarDiameter = UnitUtilsHelper.FromMm(
            settings.LongRebarDiameter ?? DefaultSettings.LongRebarDiameterMm);
        LongRebarAnchorageLength = UnitUtilsHelper.FromMm(
            settings.LongRebarAnchorageLength ?? DefaultSettings.LongRebarAnchorageLengthMm);
        StirrupBendLength = UnitUtilsHelper.FromMm(
            settings.StirrupBendLength ?? DefaultSettings.StirrupBendLengthMm);
    }

    public FrameParameterSettings ParameterSettings { get; }

    public string FrameFamilyName { get; }
    public string FrameFamilyTypeName { get; }

    public Slab HostSlab { get; }

    public double RebarCoverTop { get; }

    public double PlateRebarDiameter { get; }

    public RebarClass StirrupRebarClass { get; }
    public double StirrupRebarDiameter { get; }
    public double StirrupStep { get; }
    public double FrameWidth { get; }

    public double LongRebarDiameter { get; }
    public double LongRebarAnchorageLength { get; }
    public double StirrupBendLength { get; }

    public double GetFrameHeight() {
        double height = _slabThickness - (RebarCoverTop + PlateRebarDiameter) -
                                        (_rebarCoverBottom + 2 * PlateRebarDiameter) -
                                        LongRebarDiameter;

        return height;
    }

    public double GetFrameLength() {
        double afterColumnDistance = GetAfterPylonDistance();
        double punchingZoneLength = GetPunchingZone();
        double punchingZoneLengthRounded = UnitUtilsHelper.CeilingToMm(punchingZoneLength, _roundingStepMm);
        double frameLength = Math.Ceiling((punchingZoneLengthRounded - afterColumnDistance) / StirrupStep) * StirrupStep;

        return frameLength;
    }

    /// <summary>
    /// Находит расстояние от грани колонны до первого хомута каркаса
    /// </summary>
    /// <returns></returns>
    public double GetAfterPylonDistance() {
        double workingHeight = GetWorkingHeight();
        double afterColumnDistance = workingHeight / _afterPylonDistanceDivider;
        double afterColumnDistanceRounded = UnitUtilsHelper.CeilingToMm(afterColumnDistance, _roundingStepMm);

        return afterColumnDistanceRounded;
    }

    /// <summary>
    /// Находит размер зоны продавливания (расстояние от грани пилона до 1,5h0)
    /// </summary>
    /// <returns></returns>
    public double GetPunchingZone() {
        double workingHeight = GetWorkingHeight();
        double punchingZone = UnitUtilsHelper.CeilingToMm(_punchingZoneFactor * workingHeight, _roundingStepMm);

        return punchingZone;
    }

    /// <summary>
    /// Находит рабочую высоту сечения фундамента (h0)
    /// </summary>
    /// <returns></returns>
    private double GetWorkingHeight() {
        return _slabThickness - (_rebarCoverBottom + PlateRebarDiameter);
    }
}
