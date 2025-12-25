using Autodesk.Revit.DB;

namespace RevitUnmodelingMep.Models.Entities;

internal sealed class CalculationElementPipeIns : CalculationElementBase {
    private double? _insulationThikness;
    private double? _length;
    private double? _perimeter;
    private double? _area;
    private double? _outDiameter;
    private double? _diameter;

    public CalculationElementPipeIns(Element element) : base(element) {
    }

    public bool IsRound { get; set; }

    public double? InsulationThikness {
        get => _insulationThikness;
        set => _insulationThikness = ToMillimeters(value);
    }

    public double? Length {
        get => _length;
        set => _length = ToMillimeters(value);
    }

    public double? Diameter {
        get => _diameter;
        set => _diameter = ToMillimeters(value);
    }

    public double? OutDiameter {
        get => _outDiameter;
        set => _outDiameter = ToMillimeters(value);
    }

    public double? Perimeter {
        get => _perimeter;
        set => _perimeter = ToMillimeters(value);
    }

    public double? Area {
        get => _area;
        set => _area = ToSquareMeters(value);
    }
}

