using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitVolumeOfWork.Models; 
internal class RoomElement {
    private readonly Room _room;
    private readonly Document _document;
    private readonly Level _level;
    private readonly string _name;
    private readonly string _number;
    private readonly string _group;
    private readonly string _id;

    public RoomElement(ILocalizationService localizationService, Room room, Document document) {
        _room = room;
        _document = document;

        string defaultParamValue = localizationService.GetLocalizedString("DefaultParamValue");
        
        _level =  _room.Level;
        _id = _room.Id.ToString();
        _name =  _room.GetParamValueOrDefault(BuiltInParameter.ROOM_NAME, defaultParamValue);
        _number = _room.GetParamValueOrDefault(BuiltInParameter.ROOM_NUMBER, defaultParamValue);
        var keyParamValueId = _room.GetParamValueOrDefault<ElementId>(ProjectParamsConfig.Instance.RoomGroupName);
        _group =  keyParamValueId.IsNull()
            ? defaultParamValue
            : _document.GetElement(keyParamValueId).Name;
    }

    public Level Level => _level;
    public string Name => _name;
    public string Number => _number;
    public string Group => _group;
    public string Id => _id;

    public List<Element> GetBoundaryWalls() {
        var wallCategoryId = new ElementId(BuiltInCategory.OST_Walls);

        return _room.GetBoundarySegments(SpatialElementExtensions.DefaultBoundaryOptions)
            .SelectMany(x => x)
            .Select(x => x.ElementId)
            .Where(x => x.IsNotNull())
            .Select(x => _document.GetElement(x))
            .Where(x => x.Category?.Id == wallCategoryId)
            .ToList();
    }
}
