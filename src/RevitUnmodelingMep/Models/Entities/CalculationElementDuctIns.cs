using Autodesk.Revit.DB;

namespace RevitUnmodelingMep.Models.Entities;

internal sealed class CalculationElementDuctIns : CalculationElementBase {
    private double? _length_mm;
    private double? _perimeter_mm;
    private double? _area_m2;
    private double? _diameter_mm;
    private double? _width_mm;
    private double? _height_mm;

    public CalculationElementDuctIns(Element element) : base(element) {
    }

    public bool IsRound { get; set; }

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
}

