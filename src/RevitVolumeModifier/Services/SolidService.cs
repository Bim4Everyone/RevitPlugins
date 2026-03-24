using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitVolumeModifier.Models;

namespace RevitVolumeModifier.Services;

internal class SolidService {

    public const double VolumeEpsilon = 1e-6;

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

    public IList<Solid> DivideElementSolids(Element element, Plane plane) {
        var solids = element.GetSolids().Where(IsValidSolid).ToList();
        return solids.Select(s => DivideSolid(s, plane)).ToList();
    }

    public Dictionary<ElementId, List<Solid>> CutElementSolids(
        IList<Element> elements,
        IList<Element> elementsToCut,
        out bool success,
        out List<ElementId> processedElements) {

        success = true;
        processedElements = [];
        var result = new Dictionary<ElementId, List<Solid>>();

        var solidsToCut = elementsToCut.SelectMany(e => e.GetSolids()).Where(IsValidSolid).ToList();
        if(!solidsToCut.Any()) { success = false; return result; }

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
                if(!IsValidSolid(cutSolid)) { success = false; continue; }

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

    public DividePlane GetHorizontalPlane(Reference reference) {
        return CreateDividePlane(new XYZ(0, 0, 1), reference.GlobalPoint);
    }

    public DividePlane GetVerticalPlane(Document document, Reference reference) {
        var element = document.GetElement(reference.ElementId);
        var face = element?.GetGeometryObjectFromReference(reference) as Face;
        if(face == null) {
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

        var horizontalNormal = new XYZ(faceNormal.X, faceNormal.Y, 0);
        horizontalNormal = horizontalNormal.IsZeroLength() ? XYZ.BasisX : horizontalNormal.Normalize();
        var perpendicular = horizontalNormal.CrossProduct(XYZ.BasisZ);
        perpendicular = perpendicular.IsZeroLength() ? XYZ.BasisY : perpendicular.Normalize();

        return CreateDividePlane(perpendicular, reference.GlobalPoint);
    }

    public DividePlane GetPlaneFromFace(Document doc, Reference reference) {
        var elem = doc.GetElement(reference.ElementId);
        var transform = Transform.Identity;

        if(elem is RevitLinkInstance linkInst) {
            transform = linkInst.GetTotalTransform();
            elem = linkInst.GetLinkDocument().GetElement(reference.LinkedElementId);
        }

        if(elem.GetGeometryObjectFromReference(reference) is not PlanarFace planarFace) {
            return null;
        }

        var normal = transform.OfVector(planarFace.FaceNormal);
        var origin = transform.OfPoint(planarFace.Origin);
        return CreateDividePlane(normal, origin);
    }

    private DividePlane CreateDividePlane(XYZ normal, XYZ origin) {
        var positivePlane = Plane.CreateByNormalAndOrigin(normal, origin);
        var negativePlane = Plane.CreateByNormalAndOrigin(normal.Negate(), origin);
        return new DividePlane { PositivePlane = positivePlane, NegativePlane = negativePlane };
    }

    private IList<Solid> CreateUnitedSolids(IList<Solid> solids, out bool success) {
        success = true;
        if(!solids.Any()) {
            return [];
        }

        var solid = solids[0];
        var list = new List<Solid>();

        for(int i = 1; i < solids.Count; i++) {
            try { solid = BooleanOperationsUtils.ExecuteBooleanOperation(solid, solids[i], BooleanOperationsType.Union); } catch { success = false; list.Add(solid); solid = solids[i]; }
        }

        list.Add(solid);
        return list;
    }

    private Solid DivideSolid(Solid solid, Plane plane) {
        try { return BooleanOperationsUtils.CutWithHalfSpace(solid, plane); } catch { return solid; }
    }

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
            } catch { return solid; }
        }

        return solid;
    }

    private bool IsValidSolid(Solid solid) {
        return solid != null && solid.Volume > 0;
    }
}
