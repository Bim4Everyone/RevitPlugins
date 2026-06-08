using System;

using Autodesk.Revit.DB;

namespace RevitPylonLoadAreas.Exceptions;

internal class LoadAreasProcessingException : Exception {
    public LoadAreasProcessingException(string message) : base(message) { }
}

internal sealed class PylonsTooCloseException : LoadAreasProcessingException {
    public PylonsTooCloseException(ElementId first, ElementId second, double distance)
        : base($"Пилоны [{first}] и [{second}] расположены слишком близко (расстояние {distance:0.###} фт).") {
        FirstPylonId = first;
        SecondPylonId = second;
        Distance = distance;
    }

    public ElementId FirstPylonId { get; }
    public ElementId SecondPylonId { get; }
    public double Distance { get; }
}

internal sealed class PylonAndWallTooCloseException : LoadAreasProcessingException {
    public PylonAndWallTooCloseException(ElementId pylonId, double distance)
        : base($"Пилон [{pylonId}] расположен слишком близко к точке стены (расстояние {distance:0.###} фт).") {
        PylonId = pylonId;
        Distance = distance;
    }

    public ElementId PylonId { get; }
    public double Distance { get; }
}

internal sealed class FloorTopFaceNotFoundException : LoadAreasProcessingException {
    public FloorTopFaceNotFoundException(ElementId floorId)
        : base($"У плиты [{floorId}] не найдена верхняя плоская грань.") { }
}

internal sealed class FilledRegionTypeNotFoundException : LoadAreasProcessingException {
    public FilledRegionTypeNotFoundException()
        : base("В проекте не найден ни один FilledRegionType. Добавьте хотя бы один тип закраски.") { }
}
