using System;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.LevelFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.OpeningPlacement.PointFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.SolidProviders;
using RevitOpeningPlacement.Models.OpeningUnion;

namespace RevitOpeningPlacement.Models.OpeningPlacement.PlacerInitializers {
    internal class WallOpeningGroupPlacerInitializer : IOpeningGroupPlacerInitializer {
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public OpeningPlacer GetPlacer(RevitRepository revitRepository, OpeningsGroup openingsGroup) {
            if(openingsGroup is null) {
                throw new ArgumentNullException(nameof(openingsGroup));
            }
            if(openingsGroup.Elements is null) {
                throw new ArgumentNullException(nameof(openingsGroup.Elements));
            }
            if(openingsGroup.Elements.Count < 2) {
                throw new ArgumentOutOfRangeException(nameof(openingsGroup.Elements.Count));
            }
            var pointFinder = new WallOpeningsGroupPointFinder(openingsGroup);
            Element element1 = openingsGroup.Elements[0];
            Element element2 = openingsGroup.Elements[1];

            return new OpeningPlacer(revitRepository) {
                Type = revitRepository.GetOpeningTaskType(OpeningType.WallRectangle),
                PointFinder = pointFinder,
                LevelFinder = new OpeningsGroupLevelFinder(revitRepository, openingsGroup),
                AngleFinder = new WallOpeningsGroupAngleFinder(openingsGroup),
                ParameterGetter = new WallSolidParameterGetter(new OpeningGroupSolidProvider(openingsGroup), pointFinder, element1, element2)
            };
        }
    }
}
