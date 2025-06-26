using System;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitSleeves.Models;

namespace RevitSleeves.Services.Core;
internal class GeometryUtils : IGeometryUtils {
    private readonly RevitRepository _revitRepository;

    public GeometryUtils(RevitRepository revitRepository) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
    }


    public double GetFloorThickness(Floor floor) {
        if(floor is null) {
            throw new ArgumentNullException(nameof(floor));
        }

        return floor.FloorType.GetCompoundStructure().GetWidth();
    }

    public bool FloorIsHorizontal(Floor floor) {
        var bbox = floor.GetBoundingBox();
        double bboxHeight = bbox.Max.Z - bbox.Min.Z;
        double floorHeight = GetFloorThickness(floor);
        return Math.Abs(bboxHeight - floorHeight) >= _revitRepository.Application.ShortCurveTolerance;
    }
}
