using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitClashDetective.Models.Extensions;

namespace RevitOpeningPlacement.Services;

/// <summary>
/// Сервис поиска элементов, пересекающихся с заданным солидом.
/// <para>
/// Поиск двухэтапный: сначала грубый отбор по <see cref="BoundingBoxIntersectsFilter"/>,
/// затем точная проверка по <see cref="ElementIntersectsSolidFilter"/>.
/// </para>
/// </summary>
internal class IntersectingElementsFinder : IIntersectingElementsFinder {
    public IntersectingElementsFinder() {
    }

    public ICollection<ElementId> GetIntersectingElementIds(
        Document document,
        ICollection<ElementId> elementIds,
        Solid solid) {
        if(document is null) {
            throw new ArgumentNullException(nameof(document));
        }

        if(elementIds is null) {
            throw new ArgumentNullException(nameof(elementIds));
        }

        return SolidIsEmpty(solid) || !elementIds.Any()
            ? Array.Empty<ElementId>()
            : new FilteredElementCollector(document, elementIds)
                .WherePasses(new BoundingBoxIntersectsFilter(solid.GetOutline()))
                .WherePasses(new ElementIntersectsSolidFilter(solid))
                .ToElementIds();
    }

    public ICollection<ElementId> GetIntersectingElementIds(
        Document document,
        ICollection<BuiltInCategory> categories,
        Solid solid) {
        if(document is null) {
            throw new ArgumentNullException(nameof(document));
        }

        if(categories is null) {
            throw new ArgumentNullException(nameof(categories));
        }

        return SolidIsEmpty(solid) || !categories.Any()
            ? Array.Empty<ElementId>()
            : new FilteredElementCollector(document)
                .WherePasses(new ElementMulticategoryFilter(categories))
                .WherePasses(new BoundingBoxIntersectsFilter(solid.GetOutline()))
                .WherePasses(new ElementIntersectsSolidFilter(solid))
                .ToElementIds();
    }

    private bool SolidIsEmpty(Solid solid) {
        return (solid is null) || (solid.GetVolumeOrDefault() <= 0);
    }
}
