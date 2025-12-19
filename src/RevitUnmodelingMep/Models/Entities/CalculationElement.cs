using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitUnmodelingMep.Models.Entities;

internal class CalculationElement {
    private string _formula;
    private bool _isRound;
    private bool _isInsulated;
    private double _insulationThikness;
    private double _length;
    private double _outDiameter;
    private double _perimeter;
    private double _inDiameter;
    private double _width;
    private double _height;
    private Element _element;

    

    public bool IsRound {
        get { return _isRound; }
        set { _isRound = value; }
    }

    public bool IsInsulated {
        get { return _isInsulated; }
        set { _isInsulated = value; }
    }

    public double InsulationThikness {
        get { return _insulationThikness; }
        set { _insulationThikness = value; }
    }

    public double Length {
        get { return _length; }
        set { _length = value; }
    }

    public double Diameter {
        get { return _outDiameter; }
        set { _outDiameter = value; }
    }

    public double Width {
        get { return _width; }
        set { _width = value; }
    }

    public double Height {
        get { return _height; }
        set { _height = value; }
    }

    public Element Element {
        get { return _element; }
        set { _element = value; }
    }

    public string Formula { 
        get => _formula; 
        set => _formula = value; 
    }

    public double Perimeter { 
        get => _perimeter; 
        set => _perimeter = value; 
    }

    public double InDiameter { 
        get => _inDiameter; 
        set => _inDiameter = value; 
    }
}
