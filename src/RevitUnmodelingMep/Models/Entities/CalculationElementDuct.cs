using Autodesk.Revit.DB;

namespace RevitUnmodelingMep.Models.Entities;

internal sealed class CalculationElementDuct : CalculationElementBase {
    private double? _insulationThikness;
    private double? _length;
    private double? _perimeter;
    private double? _area;
    private double? _volume;
    private double? _diameter;
    private double? _width;
    private double? _height;
    private double? _insulationArea;

    public CalculationElementDuct(Element element) : base(element) {
    }

    public bool IsRound { get; set; }

    public bool IsInsulated { get; set; }

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

    public double? Width {
        get => _width;
        set => _width = ToMillimeters(value);
    }

    public double? Height {
        get => _height;
        set => _height = ToMillimeters(value);
    }

    public double? Perimeter {
        get => _perimeter;
        set => _perimeter = ToMillimeters(value);
    }

    public double? Area {
        get => _area;
        set => _area = ToSquareMeters(value);
    }

    public double? InsulationArea {
        get => _insulationArea;
        set => _insulationArea = ToSquareMeters(value);
    }

    public double? Volume {
        get => _volume;
        set => _volume = ToCubicMeters(value);
    }
}

