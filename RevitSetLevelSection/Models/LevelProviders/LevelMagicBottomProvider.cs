using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitSetLevelSection.Models.ElementPositions;

namespace RevitSetLevelSection.Models.LevelProviders {
    internal class LevelMagicBottomProvider : ILevelProvider {
        private readonly IElementPosition _elementPosition;

        public LevelMagicBottomProvider(IElementPosition elementPosition) {
            _elementPosition = elementPosition;
        }
        
        public Level GetLevel(Element element, List<Level> levels) {
            double position = _elementPosition.GetPosition(element);
            return levels.OrderBy(item => item.Elevation)
                .Where(item => item.Elevation < position)
                .FirstOrDefault(item=> Math.Abs(item.Elevation - position) > 4.92125984251969);
        }
    }
}