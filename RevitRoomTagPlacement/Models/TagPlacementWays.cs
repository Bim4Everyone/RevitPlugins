using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitRoomTagPlacement.Models {
    public enum GroupPlacementWay {
        EveryRoom = 0,
        OneRoomPerGroup = 1
    }

    public enum PositionPlacementWay {
        LeftUp = 0,
        CenterUp = 1,
        RightUp = 2,
        LeftCenter = 3,
        CenterCenter = 4,
        RightCenter = 5,
        LeftDown = 6,
        CenterDown = 7,
        RightDown = 8,
    }
}
