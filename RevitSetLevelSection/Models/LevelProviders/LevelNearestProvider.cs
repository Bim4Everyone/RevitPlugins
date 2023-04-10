using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitSetLevelSection.Models.ElementPositions;

namespace RevitSetLevelSection.Models.LevelProviders {
    internal class LevelNearestProvider : ILevelProvider {
        private readonly IElementPosition _elementPosition;
        private readonly ILevelElevationService _levelElevationService;

        public LevelNearestProvider(IElementPosition elementPosition, ILevelElevationService levelElevationService) {
            _elementPosition = elementPosition;
            _levelElevationService = levelElevationService;
        }

        public Level GetLevel(Element element, List<Level> levels) {
            double position = _elementPosition.GetPosition(element);
            return levels
                .OrderBy(item => Math.Abs(_levelElevationService.GetElevation(item) - position))
                .FirstOrDefault();
        }
    }
}