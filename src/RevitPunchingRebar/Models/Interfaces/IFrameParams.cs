namespace RevitPunchingRebar.Models.Interfaces;
internal interface IFrameParams {
    string FrameFamilyName { get; }
    string FrameFamilyTypeName { get; }

    double SlabThickness { get; }
    Slab HostSlab { get; }
    double RebarCoverTop { get; }
    double RebarCoverBottom { get; }
    double PlateRebarDiameter { get; }

    RebarClass StirrupRebarClass { get; }
    double StirrupRebarDiameter { get; }
    double StirrupStep { get; }

    double FrameWidth { get; }

    /// <summary>
    /// Находит длину каркаса (без учета "хвостиков")
    /// </summary>
    /// <returns></returns>
    double GetFrameLength();

    /// <summary>
    /// Находит высоту каркаса (расстояние между ц.т.
    /// продольных стержней)
    /// </summary>
    /// <returns></returns>
    double GetFrameHeight(double longRebarDiameter);

    /// <summary>
    /// Находит расстояние от грани колонны до первого хомута каркаса
    /// </summary>
    /// <returns></returns>
    double GetAfterPylonDistance();

    /// <summary>
    /// Находит размер зоны продавливания (расстояние от грани пилона до 1,5h0)
    /// </summary>
    /// <returns></returns>
    double GetPunchingZone();
}
