using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitDocumenter.Models.RoomDimensions.Geometry;

namespace RevitDocumenter.Models.RoomDimensions.Diagnostics;

/// <summary>
/// Геометрия дверей для раскладки, независимо от режима отладки. Повторно используется в диагностике.
/// </summary>
internal class DoorObstacleCollector {
    private readonly Document _document;
    private readonly View _view;

    public DoorObstacleCollector(Document document, View view) {
        _document = document.ThrowIfNull();
        _view = view.ThrowIfNull();
    }

    public DoorObstacleResult Collect() {
        var result = new DoorObstacleResult();
        try {
            using var collector = new FilteredElementCollector(_document, _view.Id);
            var doors = collector.OfCategory(BuiltInCategory.OST_Doors).WhereElementIsNotElementType()
                .Where(e => !e.IsHidden(_view)).OrderBy(e => IdValue(e.Id)).ToList();
            foreach(var door in doors) {
                long id = IdValue(door.Id);
                result.DoorCount++;
                try {
                    var geometry = new DoorGeometryCollector(_document, _view).Collect(door);
                    result.Geometry.Add(id, geometry);
                    var leaves = DoorLeafBuilder.Build(id, geometry);
                    result.Leaves.AddRange(leaves);
                    result.OpeningArcs.AddRange(DoorLeafBuilder.BuildOpeningArcs(id, geometry));
                    if(leaves.Count == 0) {
                        result.WithoutRecognizedLeaf.Add(id);
                    }
                    result.Errors.AddRange(geometry.Errors.Select(e => $"Дверь {id}: {e}"));
                    if(geometry.Truncated) {
                        result.Errors.Add($"Дверь {id}: геометрия усечена, полотно не используется");
                    }
                } catch(Exception ex) {
                    result.Errors.Add($"Дверь {id}: {ex.GetType().Name}: {ex.Message}");
                }
            }
        } catch(Exception ex) {
            result.Errors.Add($"Сбор дверей: {ex.GetType().Name}: {ex.Message}");
        }
        return result;
    }

    private static long IdValue(ElementId id) {
#if REVIT_2024_OR_GREATER
        return id.Value;
#else
        return id.IntegerValue;
#endif
    }
}

/// <summary>
/// Распознанные препятствия и причины пропуска дверей; нет ссылок на объекты Revit API.
/// </summary>
internal class DoorObstacleResult {
    public int DoorCount { get; set; }
    public List<DoorLeafObstacle> Leaves { get; } = [];
    public List<DoorOpeningArcObstacle> OpeningArcs { get; } = [];
    public Dictionary<long, DoorGeometryData> Geometry { get; } = [];
    public List<long> WithoutRecognizedLeaf { get; } = [];
    public List<string> Errors { get; } = [];
}
