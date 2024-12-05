using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDeclarations.Models {
    internal interface ITableInfo {
        int ColumnsTotalNumber { get; }
        int RowsTotalNumber { get; }

        int GroupsInfoColumnsNumber { get; }
        int SummerRoomsStart { get; }
        int OtherRoomsStart { get; }
        int UtpStart { get; }
        int[] NumericColumnsIndexes { get; }

        DeclarationSettings Settings { get; }
        IReadOnlyCollection<RoomGroup> RoomGroups { get; }
    }
}
