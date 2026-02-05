using System.Collections.Generic;
using System.Linq;

namespace RevitDeclarations.Models;
internal class ApartmentsTableInfo : ITableInfo {
    public const int MainRoomCells = 3;
    public const int SummerRoomCells = 4;
    private readonly int _utpWidth = 9;
    private readonly IReadOnlyCollection<Apartment> _apartments;

    public ApartmentsTableInfo(IReadOnlyCollection<Apartment> apartments, DeclarationSettings settings) {
        _apartments = apartments;
        Settings = settings;

        GroupsInfoColumnsNumber = 15;
        if(!Settings.LoadUtp) {
            _utpWidth = 0;
        }

        RowsTotalNumber = RoomGroups.Count;

        var otherNames = GetOtherPriorities();
        settings.UpdatePriorities(otherNames);

        CountRoomsForPriorities();
        CalculateTableSizes();

        int[] mainColumnsNumIndexes = new int[] { 7, 8, 9, 13, 14 };
        AreaTypeColumnsIndexes = mainColumnsNumIndexes
            .Concat(FindNumericColumns())
            .ToArray();
        LengthTypeColumnsIndexes = new int[] { 10 };

        NumericColumnsIndexes = AreaTypeColumnsIndexes
            .Concat(LengthTypeColumnsIndexes)
            .ToArray();
    }

    public int ColumnsTotalNumber { get; private set; }
    public int RowsTotalNumber { get; }

    public int GroupsInfoColumnsNumber { get; }
    public int SummerRoomsStart { get; private set; }
    public int OtherRoomsStart { get; private set; }
    public int UtpStart { get; private set; }
    public int[] NumericColumnsIndexes { get; }
    public int[] AreaTypeColumnsIndexes { get; }
    public int[] LengthTypeColumnsIndexes { get; }

    public DeclarationSettings Settings { get; }
    public IReadOnlyCollection<RoomGroup> RoomGroups => _apartments;

    private List<string> GetOtherPriorities() {
        return _apartments
           .SelectMany(x => x.GetOtherPriorityNames())
           .Distinct()
           .ToList();
    }

    private void CountRoomsForPriorities() {
        foreach(var priority in Settings.Priorities) {
            int numberOfRooms = _apartments
                .Select(x => x.GetRoomsByPrior(priority).Count)
                .Max();

            priority.MaxRoomAmount = numberOfRooms;
        }
    }

    private void CalculateTableSizes() {
        int mainRoomsWidth = Settings
            .UsedPriorities
            .Where(x => !x.IsSummer)
            .Where(x => !x.IsNonConfig)
            .Select(x => x.MaxRoomAmount * MainRoomCells)
            .Sum();

        int summerRoomsWidth = Settings
            .UsedPriorities
            .Where(x => x.IsSummer)
            .Where(x => !x.IsNonConfig)
            .Select(x => x.MaxRoomAmount * SummerRoomCells)
            .Sum();

        int otherRoomsWidth = Settings
            .UsedPriorities
            .Where(x => !x.IsSummer)
            .Where(x => x.IsNonConfig)
            .Select(x => x.MaxRoomAmount * MainRoomCells)
            .Sum();

        ColumnsTotalNumber = GroupsInfoColumnsNumber
            + mainRoomsWidth
            + summerRoomsWidth
            + otherRoomsWidth
            + _utpWidth;

        SummerRoomsStart = GroupsInfoColumnsNumber + mainRoomsWidth;
        OtherRoomsStart = GroupsInfoColumnsNumber + mainRoomsWidth + summerRoomsWidth;
        UtpStart = GroupsInfoColumnsNumber + mainRoomsWidth + summerRoomsWidth + otherRoomsWidth;
    }

    private int[] FindNumericColumns() {
        List<int> columnsIndexes = [];
        int columnNumber = GroupsInfoColumnsNumber;

        foreach(var priority in Settings.UsedPriorities) {
            if(priority.IsSummer) {
                for(int k = 0; k < priority.MaxRoomAmount; k++) {
                    int columnIndex = columnNumber + k * SummerRoomCells;
                    columnsIndexes.Add(columnIndex + 2);
                    columnsIndexes.Add(columnIndex + 3);
                }
                columnNumber += priority.MaxRoomAmount * SummerRoomCells;
            } else {
                for(int k = 0; k < priority.MaxRoomAmount; k++) {
                    int columnIndex = columnNumber + k * MainRoomCells;
                    columnsIndexes.Add(columnIndex + 2);
                }
                columnNumber += priority.MaxRoomAmount * MainRoomCells;
            }
        }
        return columnsIndexes.ToArray();
    }
}
