using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;

namespace RevitPunchingRebar.Models;
internal class Slab {
    // Допуск сравнения отметок верхних граней во внутренних единицах Revit (футах).
    private const double _elevationTolerance = 1e-6;

    internal Slab(Element element) {
        if(element != null) {
            SlabInstance = element as Floor;
            TopElevation = GetTopElevation();
            Thickness = element.get_Parameter(BuiltInParameter.FLOOR_ATTR_THICKNESS_PARAM).AsDouble();

            RebarCoverTop = GetRebarCover(BuiltInParameter.CLEAR_COVER_TOP);
            RebarCoverBottom = GetRebarCover(BuiltInParameter.CLEAR_COVER_BOTTOM);
        }
    }

    public Floor SlabInstance { get; }

    /// <summary>
    /// Защитный слой сверху из типа защитного слоя плиты. null, если тип защитного слоя не назначен.
    /// </summary>
    public double? RebarCoverTop { get; }

    /// <summary>
    /// Защитный слой снизу из типа защитного слоя плиты. null, если тип защитного слоя не назначен.
    /// </summary>
    public double? RebarCoverBottom { get; }

    public double Thickness { get; }

    /// <summary>
    /// Отметка горизонтального верха выбранной плиты в координатах проекта.
    /// null, если верх не найден, наклонен или содержит грани на разных отметках.
    /// </summary>
    public double? TopElevation { get; }

    /// <summary>
    /// Назначены ли плите типы защитного слоя сверху и снизу.
    /// </summary>
    public bool HasRebarCover => RebarCoverTop.HasValue && RebarCoverBottom.HasValue;

    /// <summary>
    /// Находит общую отметку верхних граней плиты, учитывая ее фактическое смещение от уровня.
    /// </summary>
    private double? GetTopElevation() {
        double? elevation = null;
        foreach(Reference reference in HostObjectUtils.GetTopFaces(SlabInstance)) {
            if(SlabInstance.GetGeometryObjectFromReference(reference) is not PlanarFace face
               || !face.FaceNormal.IsAlmostEqualTo(XYZ.BasisZ)) {
                return null;
            }

            // Несколько верхних граней допустимы только в одной горизонтальной плоскости.
            if(elevation.HasValue && Math.Abs(face.Origin.Z - elevation.Value) > _elevationTolerance) {
                return null;
            }

            elevation = face.Origin.Z;
        }

        return elevation;
    }

    private double? GetRebarCover(BuiltInParameter coverParam) {
        ElementId coverTypeId = SlabInstance?.get_Parameter(coverParam)?.AsElementId();
        if(coverTypeId == null) {
            return null;
        }

        RebarCoverType coverType = SlabInstance.Document.GetElement(coverTypeId) as RebarCoverType;
        return coverType?.CoverDistance;
    }
}
