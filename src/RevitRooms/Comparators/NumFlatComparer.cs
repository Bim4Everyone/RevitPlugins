using System.Collections.Generic;
using System.Linq;

using RevitRooms.ViewModels;

namespace RevitRooms.Comparators
{
    internal class NumFlatComparer : IComparer<SpatialElementViewModel> {
        private readonly ILookup<string, SpatialElementViewModel> _elements;

        public NumFlatComparer(SpatialElementViewModel[] elements) {
            _elements = elements
                .Where(item => item.IsRoomMainLevel)
                .ToLookup(item => item.RoomMultilevelGroup);
        }

        public int Compare(SpatialElementViewModel x, SpatialElementViewModel y) {
            return Comparer<double?>.Default.Compare(GetElevation(x), GetElevation(y));
        }

        private double? GetElevation(SpatialElementViewModel spatialElement) {
            if(string.IsNullOrEmpty(spatialElement?.RoomMultilevelGroup)) {
                return spatialElement?.LevelElevation;
            }

            var element = _elements[spatialElement.RoomMultilevelGroup].FirstOrDefault();
            if(element == null) {
                return spatialElement?.LevelElevation;
            }

            return element?.LevelElevation;
        }
    }
}