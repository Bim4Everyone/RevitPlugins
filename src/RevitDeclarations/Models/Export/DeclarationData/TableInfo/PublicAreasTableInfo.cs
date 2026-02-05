using System.Collections.Generic;
using System.Linq;

namespace RevitDeclarations.Models;
internal class PublicAreasTableInfo : ITableInfo {
    private readonly IReadOnlyCollection<PublicArea> _publicAreas;

    public PublicAreasTableInfo(IReadOnlyCollection<PublicArea> publicAreas,
                                DeclarationSettings settings) {
        _publicAreas = publicAreas;
        Settings = settings;

        GroupsInfoColumnsNumber = 6;
        SummerRoomsStart = 0;
        OtherRoomsStart = 0;
        UtpStart = 0;
        RowsTotalNumber = RoomGroups.Count;

        AreaTypeColumnsIndexes = new int[] { 4 };
        LengthTypeColumnsIndexes = new int[] { };
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
    public IReadOnlyCollection<RoomGroup> RoomGroups => _publicAreas;
}
