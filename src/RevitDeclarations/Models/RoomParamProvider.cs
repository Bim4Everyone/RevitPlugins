using System.Collections.Generic;
using System.Linq;

namespace RevitDeclarations.Models;
internal class RoomParamProvider {
    private readonly DeclarationSettings _settings;
    public RoomParamProvider(DeclarationSettings settings) {
        _settings = settings;
    }
    public string GetTwoParamsWithHyphen(RoomElement room, bool addPrefix) {
        string number = room.GetTextParamValue(_settings.RoomNumberParam);
        if(addPrefix) {
            string group = room.GetTextParamValue(_settings.ApartmentNumberParam);
            if(!string.IsNullOrEmpty(group)) {
                return $"{group}-{number}";
            }
        }
        return number;
    }

    public string GetDepartment(RoomElement room, string name) {
        return string.IsNullOrEmpty(room.GetTextParamValue(_settings.MultiStoreyParam))
            ? room.GetTextParamValue(_settings.DepartmentParam)
            : $"{name} на двух и более этажах";
    }

    public string GetRoomsPosition(RoomGroup roomGroup) {
        List<string> resultString = [];

        if(!string.IsNullOrEmpty(roomGroup.Building)) {
            resultString.Add($"Корпус {roomGroup.Building}");
        }

        if(!string.IsNullOrEmpty(roomGroup.Section)) {
            resultString.Add($"Секция {roomGroup.Section}");
        }

        if(!string.IsNullOrEmpty(roomGroup.Level)) {
            if(roomGroup.Level.ToLower().Contains("кровля")) {
                resultString.Add($"{roomGroup.Level}");
            } else {
                resultString.Add($"{roomGroup.Level} этаж");
            }
        }

        return string.Join(", ", resultString);
    }

    public string GetAllLevels(IEnumerable<RoomElement> rooms) {
        var levelNames = rooms
            .Select(x => x.GetTextParamValue(_settings.LevelParam))
            .Distinct();
        return string.Join(",", levelNames);
    }
}
