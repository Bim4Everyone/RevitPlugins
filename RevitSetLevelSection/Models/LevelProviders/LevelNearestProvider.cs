using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitSetLevelSection.Models.ElementPositions;

namespace RevitSetLevelSection.Models.LevelProviders {
    internal class LevelNearestProvider : ILevelProvider {
        private readonly IElementPosition _elementPosition;

        public LevelNearestProvider(IElementPosition elementPosition) {
            _elementPosition = elementPosition;
        }

        public Level GetLevel(Element element, ICollection<Level> levels) {
            double position = _elementPosition.GetPosition(element);
            return levels
                .OrderBy(item => Math.Abs(item.ProjectElevation - position))
                .FirstOrDefault();
        }
    }
}