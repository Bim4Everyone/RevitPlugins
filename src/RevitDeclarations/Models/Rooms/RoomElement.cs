using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using pyRevitLabs.Json;

namespace RevitDeclarations.Models;
internal class RoomElement {
    public RoomElement(Room room, DeclarationSettings settings) {
        RevitRoom = room;

        Name = RevitRoom.get_Parameter(BuiltInParameter.ROOM_NAME).AsString();

        AreaRevit = ParamConverter.ConvertArea(RevitRoom.Area, settings.AccuracyForArea);
        var areaCalculator = new RoomAreaCalculator(settings, this);
        AreaCoefRevit = areaCalculator.CalculateAreaCoefRevit();
        AreaLivingRevit = areaCalculator.CalculateAreaLivingRevit();
        AreaNonSummerRevit = areaCalculator.CalculateAreaNonSummerRevit();

        Area = GetAreaParamValue(settings.RoomAreaParam, settings.AccuracyForArea);
        AreaCoef = GetAreaParamValue(settings.RoomAreaCoefParam, settings.AccuracyForArea);
    }

    [JsonIgnore]
    public Room RevitRoom { get; }

    [JsonIgnore]
    public ElementId RoomLevel => RevitRoom.LevelId;
    [JsonProperty("room_type")]
    public string Name { get; }
    [JsonIgnore]
    public string DeclarationName => $"{Name}_{Number}";

    [JsonIgnore]
    public double AreaRevit { get; }
    [JsonIgnore]
    public double AreaCoefRevit { get; }
    [JsonIgnore]
    public double AreaLivingRevit { get; }
    [JsonIgnore]
    public double AreaNonSummerRevit { get; }

    [JsonProperty("area")]
    public double Area { get; }
    [JsonProperty("area_k")]
    public double AreaCoef { get; }
    [JsonProperty("number")]
    public string Number => RevitRoom.Number;

    public string GetTextParamValue(Parameter parameter) {
        return RevitRoom.GetParamValueOrDefault<string>(parameter.Definition.Name);
    }

    public double GetAreaParamValue(Parameter parameter, int accuracy) {
        double value = RevitRoom.GetParamValueOrDefault<double>(parameter.Definition.Name);
        return ParamConverter.ConvertArea(value, accuracy);
    }

    public double GetLengthParamValue(Parameter parameter, int accuracy) {
        double value = RevitRoom.GetParamValueOrDefault<double>(parameter.Definition.Name);
        return ParamConverter.ConvertLength(value, accuracy);
    }

    public double GetIntAndCurrencyParamValue(Parameter parameter) {
        return parameter.StorageType == StorageType.Double
            ? RevitRoom.GetParamValueOrDefault<double>(parameter.Definition.Name)
            : RevitRoom.GetParamValueOrDefault<int>(parameter.Definition.Name);
    }

    public IReadOnlyList<ElementId> GetBoundaries() {
        return RevitRoom.GetBoundarySegments(SpatialElementExtensions.DefaultBoundaryOptions)
            .SelectMany(item => item)
            .Select(item => item.ElementId)
            .ToList();
    }
}
