using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitPunchingRebar.Models.Interfaces;

namespace RevitPunchingRebar.Models;
internal class FrameParams : IFrameParams {
    public string FrameFamilyName { get; set; }
    public string FrameFamilyTypeName { get; set; }

    public double SlabThickness { get; set; }
    public Slab HostSlab { get; set; }

    public double RebarCoverTop { get; set; }
    public double RebarCoverBottom { get; set; }

    public double PlateRebarDiameter { get; set; }

    public int StirrupRebarClass { get; set; }
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
        double punchingZoneLengthRounded = Math.Ceiling(punchingZoneLength * 304.8 / 10) * 10 / 304.8;
        double frameLength = Math.Ceiling((punchingZoneLengthRounded - afterColumnDistance) / StirrupStep) * StirrupStep;

        return frameLength;
    }

    /// <summary>
    /// Находит расстояние от грани колонны до первого хомута каркаса
    /// </summary>
    /// <returns></returns>
    public double GetAfterPylonDistance() {
        double workingHeight = GetWorkingHeight();
        double afterColumnDistance = workingHeight / 3;
        double afterColumnDistanceRounded = Math.Ceiling((afterColumnDistance * 304.8) / 10) * 10 / 304.8;

        return afterColumnDistanceRounded;
    }

    /// <summary>
    /// Находит размер зоны продавливания (расстояние от грани пилона до 1,5h0)
    /// </summary>
    /// <returns></returns>
    public double GetPunchingZone() {
        double workingHeight = GetWorkingHeight();
        double punchingZone = Math.Ceiling((1.5 * workingHeight * 304.8) / 10) * 10 / 304.8;

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
