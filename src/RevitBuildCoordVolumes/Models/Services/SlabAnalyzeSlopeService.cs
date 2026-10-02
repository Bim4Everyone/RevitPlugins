using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Utilites;

namespace RevitBuildCoordVolumes.Models.Services;

internal class SlabAnalyzeSlopeService(SystemPluginConfig systemPluginConfig) : ISlabAnalyzeSlopeService {
    
    public bool IsSloped(Floor floor) {
        var doc = floor.Document;
        return IsShapeEdited(doc, floor) || HasSlopeBySlopeLine(doc, floor);
    }
    
    // Метод проверки редактирована ли плита
    private bool IsShapeEdited(Document doc, Floor floor) {
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
    private bool HasSlopeBySlopeLine(Document doc, Floor floor) {
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
    
}
