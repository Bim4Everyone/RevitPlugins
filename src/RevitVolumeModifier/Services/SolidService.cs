using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitVolumeModifier.Models;

namespace RevitVolumeModifier.Services;

internal class SolidService {
    public const double VolumeEpsilon = 1e-6;

    /// <summary>
    /// Метод объединения солидов
    /// </summary>
    public IList<Solid> JoinElementSolids(IList<Element> elements, out bool success) {
        success = true;
        var allSolids = new List<Solid>();

        foreach(var element in elements) {
            var solids = element.GetSolids().Where(IsValidSolid).ToList();
            if(!solids.Any()) {
                continue;
            }

            var unitedSolids = CreateUnitedSolids(solids, out bool localSuccess);
            success &= localSuccess;
            allSolids.AddRange(unitedSolids);
        }

        if(!allSolids.Any()) {
            success = false;
            return [];
        }

        var resultSolids = CreateUnitedSolids(allSolids, out bool fullSuccess);
        success &= fullSuccess;
        return resultSolids;
    }

    /// <summary>
    /// Метод вырезания солидов
    /// </summary>
    public Dictionary<ElementId, List<Solid>> CutElementSolids(
        IList<Element> elements,
        IList<Element> elementsToCut,
        out bool success,
        out List<ElementId> processedElements) {

        success = true;
        processedElements = [];
        var result = new Dictionary<ElementId, List<Solid>>();

        var solidsToCut = elementsToCut.SelectMany(e => e.GetSolids()).Where(IsValidSolid).ToList();
        if(!solidsToCut.Any()) {
            success = false;
            return result;
        }

        foreach(var element in elements) {
            var solids = element.GetSolids().Where(IsValidSolid).ToList();
            if(!solids.Any()) {
                continue;
            }

            var elementResultSolids = new List<Solid>();
            bool elementWasCut = false;

            foreach(var solid in solids) {
                double originalVolume = solid.Volume;
                var cutSolid = CreateCutSolid(solid, solidsToCut);
                if(!IsValidSolid(cutSolid)) {
                    success = false;
                    continue;
                }

                elementResultSolids.Add(cutSolid);
                if(Math.Abs(cutSolid.Volume - originalVolume) > VolumeEpsilon) {
                    elementWasCut = true;
                }
            }

            if(elementWasCut) {
                processedElements.Add(element.Id);
                result[element.Id] = elementResultSolids;
            }
        }

        return result;
    }

    /// <summary>
    /// Метод построения горизонтальной секущей плоскости
    /// </summary>
    public DividePlane GetHorizontalPlane(Reference reference) {
        return CreateDividePlane(new XYZ(0, 0, 1), reference.GlobalPoint);
    }

    /// <summary>
    /// Метод построения вертикальной секущей плоскости
    /// </summary>
    public DividePlane GetVerticalPlane(Document doc, Reference reference) {
        if(!TryGetFaceData(doc, reference, out var face, out var transform)) {
            return null;
        }

        XYZ faceNormal;

        if(face is PlanarFace planarFace) {
            faceNormal = planarFace.FaceNormal;
        } else {
            var bbox = face.GetBoundingBox();
            var uv = (bbox.Min + bbox.Max) / 2;
            faceNormal = face.ComputeNormal(uv);
        }
        faceNormal = transform.OfVector(faceNormal);

        var horizontalNormal = new XYZ(faceNormal.X, faceNormal.Y, 0);
        horizontalNormal = horizontalNormal.IsZeroLength()
            ? XYZ.BasisX
            : horizontalNormal.Normalize();

        var perpendicular = horizontalNormal.CrossProduct(XYZ.BasisZ);
        perpendicular = perpendicular.IsZeroLength()
            ? XYZ.BasisY
            : perpendicular.Normalize();

        var origin = reference.GlobalPoint;

        return CreateDividePlane(perpendicular, origin);
    }

    /// <summary>
    /// Метод построения секущей плоскости из Face
    /// </summary>
    public DividePlane GetPlaneFromFace(Document doc, Reference reference) {
        if(!TryGetFaceData(doc, reference, out var face, out var transform)) {
            return null;
        }

        XYZ normal;
        if(face is PlanarFace pf) {
            normal = pf.FaceNormal;
        } else {
            var bbox = face.GetBoundingBox();
            var uv = (bbox.Min + bbox.Max) / 2;
            normal = face.ComputeNormal(uv);
        }

        XYZ originLocal;
        if(face is PlanarFace ppf) {
            originLocal = ppf.Origin;
        } else {
            var bbox = face.GetBoundingBox();
            var uv = (bbox.Min + bbox.Max) / 2;
            originLocal = face.Evaluate(uv);
        }

        var normalHost = transform.OfVector(normal).Normalize();
        var originHost = transform.OfPoint(originLocal);

        return CreateDividePlane(normalHost, originHost);
    }

    // Метод получения Face из Reference
    private bool TryGetFaceData(Document hostDoc, Reference reference, out Face face, out Transform transform) {
        face = null;
        transform = Transform.Identity;
        if(reference == null) {
            return false;
        }

        if(reference.LinkedElementId == ElementId.InvalidElementId) {
            var element = hostDoc.GetElement(reference.ElementId);
            face = element?.GetGeometryObjectFromReference(reference) as Face;
            return face != null;
        }

        if(hostDoc.GetElement(reference.ElementId) is not RevitLinkInstance linkInstance) {
            return false;
        }

        transform = linkInstance.GetTransform();

        var geomObj = linkInstance.GetGeometryObjectFromReference(reference);
        if(geomObj is Face directFace) {
            face = directFace;
            return true;
        }

        var linkDoc = linkInstance.GetLinkDocument();
        if(linkDoc == null) {
            return false;
        }

        var linkedElement = linkDoc.GetElement(reference.LinkedElementId);
        if(linkedElement == null) {
            return false;
        }

        var hostPoint = reference.GlobalPoint;
        if(hostPoint == null) {
            return false;
        }

        var pLinked = transform.Inverse.OfPoint(hostPoint);

        var solids = linkedElement.GetSolids()
            .Where(s => s != null && s.Volume > 0)
            .ToList();

        if(solids.Count == 0) {
            return false;
        }

        Face bestFace = null;
        double bestDist = double.MaxValue;

        foreach(var s in solids) {
            foreach(Face f in s.Faces) {
                var proj = f.Project(pLinked);
                if(proj == null) {
                    continue;
                }

                double d = proj.XYZPoint.DistanceTo(pLinked);
                if(d < bestDist) {
                    bestDist = d;
                    bestFace = f;
                }
            }
        }

        if(bestFace == null) {
            return false;
        }

        face = bestFace;
        return true;
    }

    // Метод построения разрезающих плоскостей
    private DividePlane CreateDividePlane(XYZ normal, XYZ origin) {
        var positivePlane = Plane.CreateByNormalAndOrigin(normal, origin);
        var negativePlane = Plane.CreateByNormalAndOrigin(normal.Negate(), origin);
        return new DividePlane { PositivePlane = positivePlane, NegativePlane = negativePlane };
    }

    // Метод объединения солидов с выводом флага результата
    private IList<Solid> CreateUnitedSolids(IList<Solid> solids, out bool success) {
        success = true;
        if(!solids.Any()) {
            return [];
        }

        var solid = solids[0];
        var list = new List<Solid>();

        for(int i = 1; i < solids.Count; i++) {
            try {
                solid = BooleanOperationsUtils.ExecuteBooleanOperation(solid, solids[i], BooleanOperationsType.Union);
            } catch {
                success = false;
                list.Add(solid);
                solid = solids[i];
            }
        }

        list.Add(solid);
        return list;
    }

    // Метод разрезания солида
    public Solid DivideSolidSafe(Solid solid, Plane plane) {
        try {
            return BooleanOperationsUtils.CutWithHalfSpace(solid, plane);
        } catch {
            return null;
        }
    }

    // Метод получения разрезанного солида
    private Solid CreateCutSolid(Solid solid, IList<Solid> solidsToCut) {
        if(!IsValidSolid(solid)) {
            return solid;
        }

        foreach(var cut in solidsToCut.Where(IsValidSolid)) {
            try {
                var result = BooleanOperationsUtils.ExecuteBooleanOperation(solid, cut, BooleanOperationsType.Difference);
                if(!IsValidSolid(result)) {
                    return solid;
                }

                solid = result;
            } catch {
                return solid;
            }
        }

        return solid;
    }

    // Метод проверки валидности солида
    private bool IsValidSolid(Solid solid) {
        return solid != null && solid.Volume > 0;
    }
}
