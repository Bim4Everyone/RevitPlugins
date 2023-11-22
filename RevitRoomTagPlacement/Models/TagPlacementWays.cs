using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitRoomTagPlacement.Models {
    public enum GroupPlacementWay {
        EveryRoom = 0,
        OneRoomPerGroupRandom = 1,
        OneRoomPerGroupByName = 2
    }

    public enum PositionPlacementWay {
        LeftTop = 0,
        CenterTop = 1,
        RightTop = 2,
        LeftCenter = 3,
        CenterCenter = 4,
        RightCenter = 5,
        LeftBottom = 6,
        CenterBottom = 7,
        RightBottom = 8
    }
}
