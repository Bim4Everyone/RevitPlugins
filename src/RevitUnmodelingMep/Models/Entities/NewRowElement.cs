using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

namespace RevitUnmodelingMep.Models.Entities;

internal class NewRowElement {
    private Element _element;
    private string _system;
    private string _function;
    private string _group;
    private string _name;
    private string _mark;
    private string _code;
    private string _maker;
    private string _unit;
    private string _description;
    private double _number;
    private string _note;
    private string _mass;
    private string _smrBlock;
    private string _smrSection;
    private string _smrFloor;
    private string _smrFloorDE;

    public string System {
        get => _system;
        set => _system = value;
    }

    public string Function {
        get => _function;
        set => _function = value;
    }

    public string Group {
        get => _group;
        set => _group = value;
    }

    public string Name {
        get => _name;
        set => _name = value;
    }

    public string Mark {
        get => _mark;
        set => _mark = value;
    }

    public string Code {
        get => _code;
        set => _code = value;
    }

    public string Maker {
        get => _maker;
        set => _maker = value;
    }

    public string Unit {
        get => _unit;
        set => _unit = value;
    }

    public string Description {
        get => _description;
        set => _description = value;
    }

    public double Number {
        get => _number;
        set => _number = value;
    }

    public string Note {
        get => _note;
        set => _note = value;
    }

    public string Mass {
        get => _mass;
        set => _mass = value;
    }

    public string SmrBlock {
        get => _smrBlock;
        set => _smrBlock = value;
    }

    public string SmrSection {
        get => _smrSection;
        set => _smrSection = value;
    }

    public string SmrFloor {
        get => _smrFloor;
        set => _smrFloor = value;
    }

    public string SmrFloorDE {
        get => _smrFloorDE;
        set => _smrFloorDE = value;
    }
    public Element Element { 
        get => _element; 
        set => _element = value; 
    }
}
