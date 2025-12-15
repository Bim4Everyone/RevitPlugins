using System.Collections.Generic;
using System.Linq;

namespace RevitDeclarations.Models;
internal class CommercialGroupTableInfo : ITableInfo {
    private readonly IReadOnlyCollection<CommercialRooms> _roomGroups;

    public CommercialGroupTableInfo(IReadOnlyCollection<CommercialRooms> roomGroups,
                                    DeclarationSettings settings) {
        _roomGroups = roomGroups;
        Settings = settings;

        GroupsInfoColumnsNumber = 3;
        SummerRoomsStart = 0;
        OtherRoomsStart = 0;
        UtpStart = 0;

        AreaTypeColumnsIndexes = new int[] { 1 };
        LengthTypeColumnsIndexes = new int[] { };
        NumericColumnsIndexes = AreaTypeColumnsIndexes
            .Concat(LengthTypeColumnsIndexes)
            .ToArray();

        RowsTotalNumber = RoomGroups.Select(x => x.Rooms.Count()).Sum();
    }

    public int ColumnsTotalNumber => GroupsInfoColumnsNumber;
    public int RowsTotalNumber { get; }

    public int GroupsInfoColumnsNumber { get; }
    public int SummerRoomsStart { get; }
    public int OtherRoomsStart { get; }
    public int UtpStart { get; }
    public int[] NumericColumnsIndexes { get; }
    public int[] AreaTypeColumnsIndexes { get; }
    public int[] LengthTypeColumnsIndexes { get; }

    public DeclarationSettings Settings { get; }
    public IReadOnlyCollection<RoomGroup> RoomGroups => _roomGroups;
}
