using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitSetLevelSection.Models.ElementPositions;

namespace RevitSetLevelSection.Models.LevelProviders {
    internal class LevelMagicBottomProvider : ILevelProvider {
        private readonly IElementPosition _elementPosition;
        private readonly ILevelElevationService _levelElevationService;

        public LevelMagicBottomProvider(IElementPosition elementPosition, ILevelElevationService levelElevationService) {
            _elementPosition = elementPosition;
            _levelElevationService = levelElevationService;
        }
        
        public Level GetLevel(Element element, List<Level> levels) {
            double position = _elementPosition.GetPosition(element);
            return levels.OrderByDescending(item => item.Elevation)
                .Where(item => _levelElevationService.GetElevation(item) < position)
                .FirstOrDefault(item=> Math.Abs(item.Elevation - position) > 4.92125984251969); // больше 1500мм
        }
    }
}