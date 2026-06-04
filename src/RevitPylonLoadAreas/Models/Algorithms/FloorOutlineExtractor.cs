using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonLoadAreas.Models.Geometry;

namespace RevitPylonLoadAreas.Models.Algorithms;

/// <summary>
/// Извлекает контуры плит как набор <see cref="Polygon2D"/> по их верхним граням.
/// </summary>
internal sealed class FloorOutlineExtractor {
    private readonly List<string> _warnings = new();

    /// <summary>
    /// Сообщения о проблемах извлечения (например, найдены криволинейные кромки).
    /// </summary>
    public IReadOnlyList<string> Warnings => _warnings;

    /// <summary>
    /// Z-координата верхней грани одной из обработанных плит (в футах).
    /// Используется для построения detail-линий.
    /// </summary>
    public double? SlabElevation { get; private set; }

    /// <summary>
    /// Возвращает список полигонов 2D — по одному на каждую верхнюю грань каждой плиты.
    /// Множественные плиты не объединяются здесь; объединение делает <see cref="PolygonBoolean"/>.
    /// </summary>
    public List<Polygon2D> Extract(IEnumerable<Floor> floors) {
        var result = new List<Polygon2D>();
        var options = new Options {
            ComputeReferences = false,
            IncludeNonVisibleObjects = false,
            DetailLevel = ViewDetailLevel.Fine
        };

        foreach(var floor in floors) {
            ExtractOne(floor, options, result);
        }
        return result;
    }

    private void ExtractOne(Floor floor, Options options, List<Polygon2D> sink) {
        var geometry = floor.get_Geometry(options);
        if(geometry == null) {
            _warnings.Add($"У плиты [{floor.Id}] нет геометрии — пропущено.");
            return;
        }

        var topFaces = CollectTopFaces(geometry);
        if(topFaces.Count == 0) {
            _warnings.Add($"У плиты [{floor.Id}] не найдена верхняя плоская грань — пропущено.");
            return;
        }

        foreach(var face in topFaces) {
            IList<CurveLoop> loops;
            try {
                loops = face.GetEdgesAsCurveLoops();
            } catch(Exception ex) {
                _warnings.Add($"Не удалось получить контуры верхней грани плиты [{floor.Id}]: {ex.Message}");
                continue;
            }
            if(loops == null || loops.Count == 0) {
                continue;
            }

            var outer = LoopToRing(loops[0], floor.Id);
            if(outer == null) {
                continue;
            }
            var holes = new List<IReadOnlyList<Point2D>>();
            for(int i = 1; i < loops.Count; i++) {
                var holeRing = LoopToRing(loops[i], floor.Id);
                if(holeRing != null) {
                    holes.Add(holeRing);
                }
            }

            var poly = new Polygon2D(
                Polygon2D.EnsureCcw(outer),
                holes.Select(Polygon2D.EnsureCw).ToList());
            sink.Add(poly);

            if(SlabElevation == null) {
                SlabElevation = face.Origin.Z;
            }
        }
    }

    private static List<PlanarFace> CollectTopFaces(GeometryElement geometry) {
        var faces = new List<PlanarFace>();
        foreach(var go in geometry) {
            switch(go) {
                case Solid solid when solid.Volume > 0:
                    foreach(Face f in solid.Faces) {
                        if(f is PlanarFace pf && pf.FaceNormal.Z > 0.95) {
                            faces.Add(pf);
                        }
                    }
                    break;
                case GeometryInstance gi:
                    var inner = gi.GetInstanceGeometry();
                    if(inner != null) {
                        faces.AddRange(CollectTopFaces(inner));
                    }
                    break;
            }
        }
        return faces;
    }

    private IReadOnlyList<Point2D> LoopToRing(CurveLoop loop, ElementId floorId) {
        var ring = new List<Point2D>();
        foreach(var curve in loop) {
            if(curve is not Line line) {
                _warnings.Add(
                    $"У плиты [{floorId}] обнаружена криволинейная кромка ({curve.GetType().Name}). " +
                    $"В v1 поддерживаются только прямые кромки. Контур пропущен.");
                return null;
            }
            var p = line.GetEndPoint(0);
            ring.Add(new Point2D(p.X, p.Y));
        }
        if(ring.Count < 3) {
            return null;
        }
        return ring;
    }
}
