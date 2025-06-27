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

    public bool IsHorizontal(Floor floor) {
        if(floor is null) {
            throw new ArgumentNullException(nameof(floor));
        }

        var bbox = floor.GetBoundingBox();
        double bboxHeight = bbox.Max.Z - bbox.Min.Z;
        double floorHeight = GetFloorThickness(floor);
        return Math.Abs(bboxHeight - floorHeight) <= _revitRepository.Application.ShortCurveTolerance;
    }

    public bool IsVertical(MEPCurve curve) {
        if(curve is null) {
            throw new ArgumentNullException(nameof(curve));
        }

        return ((LocationCurve) curve.Location).Curve is Line line
            && (line.Direction.IsAlmostEqualTo(XYZ.BasisZ) || line.Direction.IsAlmostEqualTo(XYZ.BasisZ.Negate()));
    }

    public bool IsHorizontal(MEPCurve curve) {
        if(curve is null) {
            throw new ArgumentNullException(nameof(curve));
        }

        return ((LocationCurve) curve.Location).Curve is Line line
            && (Math.Abs(line.Direction.Z) < _revitRepository.Application.ShortCurveTolerance);
    }
}
