using System.Collections.Generic;
using System.Linq;

using dosymep.Bim4Everyone.SharedParams;

namespace RevitDeclarations.Models;

internal class ExtraApartPropsCalculator {
    private readonly PrioritiesConfig _priorities;

    public ExtraApartPropsCalculator(DeclarationSettings settings) {
        _priorities = settings.PrioritiesConfig;
    }

    public string CalculateSummerRoomsList(Apartment apartment) {
        List<RoomElement> summerRooms =
        [       
            .. apartment.GetRoomsByPrior(_priorities.Balcony),
            .. apartment.GetRoomsByPrior(_priorities.Loggia),
            .. apartment.GetRoomsByPrior(_priorities.Terrace)
        ];
        if(summerRooms.Any()) {
            return string.Join(", ", summerRooms.Select(x => x.GetNameForSummerRoomWithGlazing()).Distinct());
        }

        return "Нет";
    }

    public string CalculateSummerRoomsFloorDiff(Apartment apartment) {
        List<RoomElement> summerRooms =
        [       
            .. apartment.GetRoomsByPrior(_priorities.Balcony),
            .. apartment.GetRoomsByPrior(_priorities.Loggia),
            .. apartment.GetRoomsByPrior(_priorities.Terrace)
        ];

        var param = SharedParamsConfig.Instance.RoomFloorDifference;
        var diffValues = summerRooms
            .Select(x => x.GetLengthParamValueMm(param, 3))
            .Where(x => x != 0)
            .Distinct()
            .ToList();

        if(diffValues.Count == 1) {
            return diffValues.First().ToString();
        } else if(diffValues.Count > 1) {
            double minValue = diffValues.Min();
            double maxValue = diffValues.Max();

            return $"{minValue}-{maxValue}";
        }

        return string.Empty;
    }
}
