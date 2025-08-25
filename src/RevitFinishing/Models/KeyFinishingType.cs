using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;

namespace RevitFinishing.Models;

internal class KeyFinishingType {
    private readonly string _name;
    private readonly Element _element;
    private readonly int _numberOfWalls;
    private readonly int _numberOfFloors;
    private readonly int _numberOfBaseboards;
    private readonly int _numberOCeilings;

    private static readonly  SharedParamsConfig _paramConfig = SharedParamsConfig.Instance;

    private readonly IList<SharedParam> _wallParams = [
        _paramConfig.WallFinishingType1,
        _paramConfig.WallFinishingType2,
        _paramConfig.WallFinishingType3,
        _paramConfig.WallFinishingType4,
        _paramConfig.WallFinishingType5,
        _paramConfig.WallFinishingType6,
        _paramConfig.WallFinishingType7,
        _paramConfig.WallFinishingType8,
        _paramConfig.WallFinishingType9,
        _paramConfig.WallFinishingType10
    ];

    private readonly IList<SharedParam> _floorParams = [
        _paramConfig.FloorFinishingType1,
        _paramConfig.FloorFinishingType2,
        _paramConfig.FloorFinishingType3,
        _paramConfig.FloorFinishingType4,
        _paramConfig.FloorFinishingType5
    ];

    private readonly IList<SharedParam> _baseboardParams = [
        _paramConfig.BaseboardFinishingType1,
        _paramConfig.BaseboardFinishingType2,
        _paramConfig.BaseboardFinishingType3,
        _paramConfig.BaseboardFinishingType4,
        _paramConfig.BaseboardFinishingType5
    ];

    private readonly IList<SharedParam> _ceilingParams = [
        _paramConfig.CeilingFinishingType1,
        _paramConfig.CeilingFinishingType2,
        _paramConfig.CeilingFinishingType3,
        _paramConfig.CeilingFinishingType4,
        _paramConfig.CeilingFinishingType5
    ];

    public KeyFinishingType(Element keyScheduleValue) {
        _name = keyScheduleValue.Name;
        _element = keyScheduleValue;
        _numberOfWalls = _wallParams.Count(x => keyScheduleValue.GetParam(x).HasValue);
        _numberOfFloors = _floorParams.Count(x => keyScheduleValue.GetParam(x).HasValue);
        _numberOfBaseboards = _baseboardParams.Count(x => keyScheduleValue.GetParam(x).HasValue);
        _numberOCeilings = _ceilingParams.Count(x => keyScheduleValue.GetParam(x).HasValue);
    }

    public string Name => _name;
    public Element Element => _element;
    public int NumberOfWalls => _numberOfWalls;
    public int NumberOfFloors => _numberOfFloors;
    public int NumberOfBaseboards => _numberOfBaseboards;
    public int NumberOfCeilings => _numberOCeilings;

}
