using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitSetLevelSection.Models.ElementPositions;

namespace RevitSetLevelSection.Models.LevelProviders {
    internal class TopLevelProvider : ILevelProvider {
        private readonly IElementPosition _elementPosition;

        public TopLevelProvider(IElementPosition elementPosition) {
            _elementPosition = elementPosition;
        }
        
        public Level GetLevel(Element element, List<Level> levels) {
            double position = _elementPosition.GetPosition(element);
            return levels.OrderBy(item => item.Elevation)
                .Where(item => item.Elevation > position)
                .FirstOrDefault();
        }
    }
}