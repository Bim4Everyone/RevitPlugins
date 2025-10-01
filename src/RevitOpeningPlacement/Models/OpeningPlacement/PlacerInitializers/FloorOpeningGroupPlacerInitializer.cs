using System;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.AngleFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.LevelFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters.ParametersGetters;
using RevitOpeningPlacement.Models.OpeningPlacement.PointFinders;
using RevitOpeningPlacement.Models.OpeningPlacement.SolidProviders;
using RevitOpeningPlacement.Models.OpeningUnion;

namespace RevitOpeningPlacement.Models.OpeningPlacement.PlacerInitializers;
/// <summary>
/// Класс для размещения объединенного задания на отверстие в перекрытии по группе заданий на отверстия
/// </summary>
internal class FloorOpeningGroupPlacerInitializer : IOpeningGroupPlacerInitializer {
    public FloorOpeningGroupPlacerInitializer() { }


    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Исключение, если количество элементов в группе отверстий меньше 2</exception>
    public OpeningPlacer GetPlacer(RevitRepository revitRepository, Configs.OpeningConfig config, OpeningsGroup openingsGroup) {
        if(openingsGroup is null) {
            throw new ArgumentNullException(nameof(openingsGroup));
        }
        if(openingsGroup.Elements is null) {
            throw new ArgumentNullException(nameof(openingsGroup.Elements));
        }
        if(openingsGroup.Elements.Count < 2) {
            throw new ArgumentOutOfRangeException(nameof(openingsGroup.Elements.Count));
        }
        var pointFinder = new FloorOpeningsGroupPointFinder(openingsGroup);
        var levelFinder = new OpeningsGroupLevelFinder(openingsGroup);
        return new OpeningPlacer(revitRepository) {
            Type = openingsGroup.IsCylinder
            ? revitRepository.GetOpeningTaskType(OpeningType.FloorRound)
            : revitRepository.GetOpeningTaskType(OpeningType.FloorRectangle),

            PointFinder = pointFinder,

            LevelFinder = levelFinder,
            AngleFinder = new FloorOpeningsGroupAngleFinder(openingsGroup),

            ParameterGetter = new FloorSolidParameterGetter(new OpeningGroupSolidProvider(openingsGroup), pointFinder, levelFinder, openingsGroup, config)
        };
    }
}
