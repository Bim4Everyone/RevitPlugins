using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;

namespace RevitFinishing.Models;

internal class KeyFinishingType {
    private readonly string _name;
    private readonly int _numberOfWalls;
    private readonly int _numberOfFloors;
    private readonly int _numberOfBaseboards;
    private readonly int _numberOCeilings;

    private static SharedParamsConfig _paramConfig = SharedParamsConfig.Instance;

    private IList<SharedParam> _wallParams = [
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

    public KeyFinishingType(Element keyScheduleValue) {
        _name = keyScheduleValue.Name;
        _numberOfWalls = _wallParams.Count(x => keyScheduleValue.GetParam(x).HasValue);
        _numberOfFloors = _wallParams.Count(x => keyScheduleValue.GetParam(x).HasValue);
        _numberOfBaseboards = _wallParams.Count(x => keyScheduleValue.GetParam(x).HasValue);
        _numberOCeilings = _wallParams.Count(x => keyScheduleValue.GetParam(x).HasValue);
    }

    public string Name => _name;
    public int NumberOfWalls => _numberOfWalls;
    public int NumberOfFloors => _numberOfFloors;
    public int NumberOfBaseboards => _numberOfBaseboards;
    public int NumberOfCeilings => _numberOCeilings;

}
