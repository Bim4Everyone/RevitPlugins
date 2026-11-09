using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitBuildCoordVolumes.Models.Enums;
using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Utilites;

namespace RevitBuildCoordVolumes.Models.Services;

internal class SlabAnalyzeSlopeService(SystemPluginConfig systemPluginConfig) : ISlabAnalyzeSlopeService {
    public SlabType GetSlabType(Floor floor) {
        if(HasRuledFaces(floor)) {
            return SlabType.Ruled;
        }
        if( IsShapeEdited(floor) || HasSlopeBySlopeLine(floor)) {
            return SlabType.SlopedPlanar;
        }
        return SlabType.Planar;
    }
    
    // Метод проверки редактирована ли плита
    private static bool IsShapeEdited(Floor floor) {
#if REVIT_2023_OR_LESS
        var slabShapeEditor = floor.SlabShapeEditor;
#else
        var slabShapeEditor = floor.GetSlabShapeEditor();
#endif
        if(slabShapeEditor == null) {
            return false;
        }
        var vertices = slabShapeEditor.SlabShapeVertices
            .Cast<SlabShapeVertex>()
            .ToList();

        if(vertices.Count == 0) {
            return false;
        }

        double firstZ = vertices[0].Position.Z;

        return vertices.Any(v =>
            Math.Abs(v.Position.Z - firstZ) > GeometryTolerance.Model);
    }
    
    // Метод проверки наклонена ли плита линией уклона
    private bool HasSlopeBySlopeLine(Floor floor) {
        var doc = floor.Document;
        var filter = new ElementCategoryFilter(BuiltInCategory.OST_SketchLines);
        var depIds = floor.GetDependentElements(filter);

        var lines = depIds
            .Select(doc.GetElement)
            .Where(element => element.Name.Equals(systemPluginConfig.SlopeLineName));

        if(!lines.Any()) {
            return false;
        }
        var slopeLine = lines.First();
        double start = slopeLine.GetParamValueOrDefault<double>(BuiltInParameter.SLOPE_START_HEIGHT);
        double end = slopeLine.GetParamValueOrDefault<double>(BuiltInParameter.SLOPE_END_HEIGHT);

        return Math.Abs(start - end) < GeometryTolerance.Model;
    }

    private static bool HasRuledFaces(Floor floor) {
        var solids = floor.GetSolids();
        return solids
            .SelectMany(commonSolid => commonSolid.Faces
                .Cast<Face>())
            .Any(face => face is RuledFace);
    }
    
}
