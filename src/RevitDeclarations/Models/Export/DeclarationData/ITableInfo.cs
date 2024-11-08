using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitDeclarations.Models {
    internal interface ITableInfo {
        int FullTableWidth { get; }
        int RoomGroupsInfoWidth { get; }
        int SummerRoomsStart { get; }
        int OtherRoomsStart { get; }
        int UtpStart { get; }

        IReadOnlyCollection<RoomGroup> RoomGroups { get; }
    }
}
