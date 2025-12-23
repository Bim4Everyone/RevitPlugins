using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitUnmodelingMep.Models.Entities;

internal class CalculationElement {
    private string _formula;
    private string _systemTypeName;
    private string _systemSharedName;
    private bool _isRound;
    private bool _isInsulated;
    private double? _insulationThikness;
    private double? _length;
    private double? _perimeter;
    private double? _area;
    private double? _volume;
    private double? _outDiameter;
    private double? _inDiameter;
    private double? _width;
    private double? _height;
    private double? _insulationArea;
    private Element _element;

    public CalculationElement(Element element) { 
        Element = element;
    }

    public bool IsRound {
        get { return _isRound; }
        set { _isRound = value; }
    }

    public bool IsInsulated {
        get { return _isInsulated; }
        set { _isInsulated = value; }
    }

    public double? InsulationThikness {
        get { return _insulationThikness; }
        set { _insulationThikness = value.HasValue
            ? UnitUtils.ConvertFromInternalUnits(value.Value, UnitTypeId.Millimeters)
            : (double?) null; }
    }

    public double? Length {
        get { return _length; }
        set { _length = value.HasValue
            ? UnitUtils.ConvertFromInternalUnits(value.Value, UnitTypeId.Millimeters)
            : (double?) null; }
    }

    public double? Diameter {
        get { return _inDiameter; }
        set { _inDiameter = value.HasValue
            ? UnitUtils.ConvertFromInternalUnits(value.Value, UnitTypeId.Millimeters)
            : (double?) null; }
    }

    public double? Width {
        get { return _width; }
        set { _width = value.HasValue
            ? UnitUtils.ConvertFromInternalUnits(value.Value, UnitTypeId.Millimeters)
            : (double?) null; }
    }

    public double? Height {
        get { return _height; }
        set { _height = value.HasValue
            ? UnitUtils.ConvertFromInternalUnits(value.Value, UnitTypeId.Millimeters)
            : (double?) null; }
    }

    public Element Element {
        get { return _element; }
        set { _element = value; }
    }

    public string Formula { 
        get => _formula; 
        set => _formula = value; 
    }

    public double? Perimeter { 
        get => _perimeter; 
        set => _perimeter = value.HasValue
            ? UnitUtils.ConvertFromInternalUnits(value.Value, UnitTypeId.Millimeters)
            : (double?) null; 
    }

    public double? OutDiameter { 
        get => _outDiameter; 
        set => _outDiameter = value.HasValue
            ? UnitUtils.ConvertFromInternalUnits(value.Value, UnitTypeId.Millimeters)
            : (double?) null; 
    }
    public double? Area { 
        get => _area; 
        set => _area = value.HasValue
            ? UnitUtils.ConvertFromInternalUnits(value.Value, UnitTypeId.SquareMeters)
            : (double?) null; 
    }

    public double? InsulationArea {
        get => _insulationArea;
        set => _insulationArea = value.HasValue
            ? UnitUtils.ConvertFromInternalUnits(value.Value, UnitTypeId.SquareMeters)
            : (double?) null;
    }
    public double? Volume {
        get => _volume; 
        set => _volume = value.HasValue
            ? UnitUtils.ConvertFromInternalUnits(value.Value, UnitTypeId.CubicMeters)
            : (double?) null; 
    }
    public string SystemTypeName { 
        get => _systemTypeName; 
        set => _systemTypeName = value; 
    }
    public string SystemSharedName { 
        get => _systemSharedName; 
        set => _systemSharedName = value; 
    }
}
