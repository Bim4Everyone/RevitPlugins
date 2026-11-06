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

    public string FrameFamilyName { get; set; }
    public string FrameFamilyTypeName { get; set; }

    public double SlabThickness { get; set; }
    public Slab HostSlab { get; set; }

    public double RebarCoverTop { get; set; }
    public double RebarCoverBottom { get; set; }

    public double PlateRebarDiameter { get; set; }

    public RebarClass StirrupRebarClass { get; set; }
    public double StirrupRebarDiameter { get; set; }
    public double StirrupStep { get; set; }
    public double FrameWidth { get; set; }

    public double GetFrameHeight(double longRebarDiameter) {
        double height = SlabThickness - (RebarCoverTop + PlateRebarDiameter) -
                                        (RebarCoverBottom + 2 * PlateRebarDiameter) -
                                        longRebarDiameter;

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
        double slabHeight = SlabThickness;
        double workingHeight = slabHeight - (RebarCoverBottom + PlateRebarDiameter);

        return workingHeight;
    }
}
