using Autodesk.Revit.DB;

namespace RevitUnmodelingMep.Models.Entities;

internal sealed class CalculationElementPipeIns : CalculationElementBase {
    private double? _insulationThikness_mm;
    private double? _length_mm;
    private double? _perimeter_mm;
    private double? _area_m2;
    private double? _outDiameter_mm;
    private double? _diameter_mm;

    public CalculationElementPipeIns(Element element) : base(element) {
    }

    public bool IsRound { get; set; }

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

    public double? OutDiameter_mm {
        get => _outDiameter_mm;
        set => _outDiameter_mm = ToMillimeters(value);
    }

    public double? OutDiameter_m => MillimetersToMeters(OutDiameter_mm);

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

