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

        public Level GetLevel(Element element, ICollection<Level> levels) {
            double position = _elementPosition.GetPosition(element);
            return levels.OrderByDescending(item => item.ProjectElevation)
                .Where(item => item.ProjectElevation < position)
                .FirstOrDefault(item => Math.Abs(item.ProjectElevation - position) >= 4.92125984251969); // больше 1500мм
        }
    }
}