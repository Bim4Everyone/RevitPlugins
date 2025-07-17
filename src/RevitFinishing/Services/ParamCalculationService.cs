using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitFinishing.Models;

namespace RevitFinishing.Services;
internal class ParamCalculationService {
    private string GenerateStrWithUniqueValues(IEnumerable<string> values) {
        return string.Join("; ", values.Distinct());
    }

    public string GetRoomsParameters(IEnumerable<RoomElement> rooms, BuiltInParameter bltnParam) {
        IEnumerable<string> values = rooms
            .Select(x => x.RevitRoom.GetParamValue<string>(bltnParam));

        return GenerateStrWithUniqueValues(values);
    }

    public string GetRoomsParameters(IEnumerable<RoomElement> rooms, RevitParam parameter) {
        IEnumerable<string> values = rooms
            .Select(x => x.RevitRoom.GetParamValue<string>(parameter));

        return GenerateStrWithUniqueValues(values);
    }

    public string GetRoomsKeyParameters(IEnumerable<RoomElement> rooms, RevitParam parameter) {
        IEnumerable<string> values = rooms
            .Select(x => x.RevitRoom.GetParamValueString(parameter));

        return GenerateStrWithUniqueValues(values);
    }
}
