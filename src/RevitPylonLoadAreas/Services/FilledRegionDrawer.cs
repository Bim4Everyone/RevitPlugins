using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitPylonLoadAreas.Models.Geometry;

namespace RevitPylonLoadAreas.Services;

internal sealed class FilledRegionDrawer {
    private readonly Document _document;

    public FilledRegionDrawer(Document document) {
        _document = document ?? throw new ArgumentNullException(nameof(document));
    }

    public void Draw(View view, IEnumerable<LoadArea> areas, FilledRegionType type) {
        foreach(var area in areas) {
            if(area.Circuits.Count == 0) {
                continue;
            }
            var loops = new List<CurveLoop>(area.Circuits.Count);
            for(int i = 0; i < area.Circuits.Count; i++) {
                var ring = i == 0
                    ? Polygon2D.EnsureCcw(area.Circuits[i].Vertices)
                    : Polygon2D.EnsureCw(area.Circuits[i].Vertices);
                loops.Add(new Polygon2D(ring).AsCurvLoop(0));
            }
            try {
                FilledRegion.Create(_document, type.Id, view.Id, loops);
            } catch(Autodesk.Revit.Exceptions.ArgumentsInconsistentException) {
                // Вырожденные контуры после Solid-пересечения — пропускаем.
            } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                // FilledRegion не может быть создан на этом виде / для этих контуров.
            }
        }
    }
}
