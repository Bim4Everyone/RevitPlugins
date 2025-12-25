using Autodesk.Revit.DB;

namespace RevitUnmodelingMep.Models.Entities;

internal sealed class CalculationElementDuctIns : CalculationElementBase {
    private double? _length;
    private double? _perimeter;
    private double? _area;
    private double? _diameter;
    private double? _width;
    private double? _height;

    public CalculationElementDuctIns(Element element) : base(element) {
    }

    public bool IsRound { get; set; }

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
}

