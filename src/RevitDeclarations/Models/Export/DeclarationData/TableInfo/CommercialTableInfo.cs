using System.Collections.Generic;
using System.Linq;

namespace RevitDeclarations.Models;
internal class CommercialTableInfo : ITableInfo {
    private readonly IReadOnlyCollection<CommercialRooms> _commercialRooms;

    public CommercialTableInfo(IReadOnlyCollection<CommercialRooms> commercialRooms,
                               DeclarationSettings settings) {
        _commercialRooms = commercialRooms;
        Settings = settings;

        GroupsInfoColumnsNumber = 14;
        SummerRoomsStart = 0;
        OtherRoomsStart = 0;
        UtpStart = 0;
        NumericColumnsIndexes = new int[] { 7, 8 };
        RowsTotalNumber = RoomGroups.Count;

        AreaTypeColumnsIndexes = new int[] { 7 };
        LengthTypeColumnsIndexes = new int[] { 8 };
        NumericColumnsIndexes = AreaTypeColumnsIndexes
            .Concat(LengthTypeColumnsIndexes)
            .ToArray();
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
    public IReadOnlyCollection<RoomGroup> RoomGroups => _commercialRooms;
}
