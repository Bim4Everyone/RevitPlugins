using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models.Extensions;

namespace RevitOpeningPlacement.Services;
/// <summary>
/// Сервис поиска основы (хоста) отверстия или задания на отверстие среди элементов конструкций
/// </summary>
internal class HostFinder : IHostFinder {
    private readonly IConstantsProvider _constantsProvider;

    public HostFinder(IConstantsProvider constantsProvider) {
        _constantsProvider = constantsProvider ?? throw new ArgumentNullException(nameof(constantsProvider));
    }


    public Element FindBestHost(Solid solid, ICollection<Element> hostCandidates) {
        if(hostCandidates is null) {
            throw new ArgumentNullException(nameof(hostCandidates));
        }
        if((solid is null) || (solid.Volume <= 0) || !hostCandidates.Any()) {
            return default;
        }

        // если пересечение с элементом больше половины объема отверстия,
        // то дальше искать нет смысла - это точно основа
        double halfOpeningVolume = solid.Volume / 2;
        double intersectingVolumePrevious = 0;
        var hostCandidate = hostCandidates.FirstOrDefault();
        foreach(var element in hostCandidates) {
            var structureSolid = element?.GetSolid();
            if((structureSolid is null) || (structureSolid.Volume <= _constantsProvider.ToleranceVolumeFeetCube)) {
                continue;
            }
            try {
                double intersectingVolumeCurrent = BooleanOperationsUtils.ExecuteBooleanOperation(
                    solid,
                    structureSolid,
                    BooleanOperationsType.Intersect)
                    ?.Volume
                    ?? 0;
                if(intersectingVolumeCurrent >= halfOpeningVolume) {
                    return element;
                }
                if(intersectingVolumeCurrent > intersectingVolumePrevious) {
                    intersectingVolumePrevious = intersectingVolumeCurrent;
                    hostCandidate = element;
                }
            } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                continue;
            }
        }
        return hostCandidate;
    }

    public bool InDifferentCategories(ICollection<Element> elements) {
        return elements is null
            ? throw new ArgumentNullException(nameof(elements))
            : elements
                .Where(element => element != null)
                .Select(element => element.Category.GetBuiltInCategory())
                .Distinct()
                .Count() > 1;
    }
}
