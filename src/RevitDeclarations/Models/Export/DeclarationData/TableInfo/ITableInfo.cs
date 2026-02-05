using System.Collections.Generic;

namespace RevitDeclarations.Models;
internal interface ITableInfo {
    int ColumnsTotalNumber { get; }
    int RowsTotalNumber { get; }

    int GroupsInfoColumnsNumber { get; }
    int SummerRoomsStart { get; }
    int OtherRoomsStart { get; }
    int UtpStart { get; }
    int[] NumericColumnsIndexes { get; }
    int[] AreaTypeColumnsIndexes { get; }
    int[] LengthTypeColumnsIndexes { get; }

    DeclarationSettings Settings { get; }
    IReadOnlyCollection<RoomGroup> RoomGroups { get; }
}
