using Autodesk.Revit.DB;

namespace RevitUnmodelingMep.Models.Entities;

internal sealed class CalculationElementDuct : CalculationElementBase {
    private double? _insulationThikness_mm;
    private double? _length_mm;
    private double? _perimeter_mm;
    private double? _area_m2;
    private double? _volume_m3;
    private double? _diameter_mm;
    private double? _width_mm;
    private double? _height_mm;
    private double? _insulationArea_m2;

    public CalculationElementDuct(Element element) : base(element) {
    }

    public bool IsRound { get; set; }

    public bool IsInsulated { get; set; }

    public double ProjectStock { get; set; }

    public double? InsulationThikness_mm {
        get => _insulationThikness_mm;
        set => _insulationThikness_mm = ToMillimeters(value);
    }

    public double? InsulationThikness_m => MillimetersToMeters(InsulationThikness_mm);

    public double? Length_mm {
        get => _length_mm;
        set => _length_mm = ToMillimeters(value);
    }

    public double? Length_m => MillimetersToMeters(Length_mm);

    public double? Diameter_mm {
        get => _diameter_mm;
        set => _diameter_mm = ToMillimeters(value);
    }

    public double? Diameter_m => MillimetersToMeters(Diameter_mm);

    public double? Width_mm {
        get => _width_mm;
        set => _width_mm = ToMillimeters(value);
    }

    public double? Width_m => MillimetersToMeters(Width_mm);

    public double? Height_mm {
        get => _height_mm;
        set => _height_mm = ToMillimeters(value);
    }

    public double? Height_m => MillimetersToMeters(Height_mm);

    public double? Perimeter_mm {
        get => _perimeter_mm;
        set => _perimeter_mm = ToMillimeters(value);
    }

    public double? Perimeter_m => MillimetersToMeters(Perimeter_mm);

    public double? Area_m2 {
        get => _area_m2;
        set => _area_m2 = ToSquareMeters(value);
    }

    public double? InsulationArea_m2 {
        get => _insulationArea_m2;
        set => _insulationArea_m2 = ToSquareMeters(value);
    }

    public double? Volume_m3 {
        get => _volume_m3;
        set => _volume_m3 = ToCubicMeters(value);
    }
}

