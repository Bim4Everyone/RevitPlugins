using System;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitSleeves.Models;
using RevitSleeves.Models.Config;
using RevitSleeves.Models.Navigator;
using RevitSleeves.Models.Placing;
using RevitSleeves.Services.Core;

namespace RevitSleeves.Services.Navigator;
internal class SleeveStatusFinder : ISleeveStatusFinder {
    private readonly RevitRepository _revitRepository;
    private readonly SleevePlacementSettingsConfig _config;
    private readonly IGeometryUtils _geometryUtils;
    private readonly IStructureLinksProvider _structureLinksProvider;
    private readonly IOpeningGeometryProvider _openingGeometryProvider;
    private readonly BuiltInCategory[] _unacceptableConstructions
        = [BuiltInCategory.OST_StructuralColumns, BuiltInCategory.OST_StructuralFraming];
    private Transform _structureDocumentTransformCache;
    private Pipe[] _intersectingPipesCache;
    private FamilyInstance[] _intersectingOpeningsCache;
    private Wall[] _intersectingWallsCache;
    private Floor[] _intersectingFloorsCache;
    private Element[] _intersectingUnacceptableStructuresCache;

    public SleeveStatusFinder(
        RevitRepository revitRepository,
        SleevePlacementSettingsConfig config,
        IGeometryUtils geometryUtils,
        IStructureLinksProvider structureLinksProvider,
        IOpeningGeometryProvider openingGeometryProvider) {

        _revitRepository = revitRepository
            ?? throw new ArgumentNullException(nameof(revitRepository));
        _config = config
            ?? throw new ArgumentNullException(nameof(config));
        _geometryUtils = geometryUtils
            ?? throw new ArgumentNullException(nameof(geometryUtils));
        _structureLinksProvider = structureLinksProvider
            ?? throw new ArgumentNullException(nameof(structureLinksProvider));
        _openingGeometryProvider = openingGeometryProvider
            ?? throw new ArgumentNullException(nameof(openingGeometryProvider));
    }


    public SleeveStatus GetStatus(SleeveModel sleeve) {
        ClearCache();
        try {
            if(SleeveIsInvalid(sleeve)) {
                return SleeveStatus.Invalid;
            }
            if(SleeveIsEmpty(sleeve)) {
                return SleeveStatus.Empty;
            }
            if(SleeveIsOutsideOfStructure(sleeve)) {
                return SleeveStatus.OutsideOfStructure;
            }
            if(SleeveIsInUnacceptableStructures(sleeve)) {
                return SleeveStatus.UnacceptableConstructions;
            }
            if(SleeveIsInDifferentStructures(sleeve)) {
                return SleeveStatus.DifferentConstructions;
            }
            if(SleeveBeyondOpening(sleeve)) {
                return SleeveStatus.BeyondOpening;
            }
            if(SleeveIntersectsWithManyMepElements(sleeve)) {
                return SleeveStatus.MultipleMepElements;
            }
            if(SleeveIsIrrelevant(sleeve)) {
                return SleeveStatus.Irrelevant;
            }
            if(SleeveDiameterNotFound(sleeve, out var diameterRange)) {
                return SleeveStatus.DiameterNotFound;
            }
            if(SleeveIsTooBig(sleeve, diameterRange)) {
                return SleeveStatus.TooBigDiameter;
            }
            if(SleeveIsTooSmall(sleeve, diameterRange)) {
                return SleeveStatus.TooSmallDiameter;
            }
            if(SleeveEndFaceFarAwayFromStructure(sleeve)) {
                return SleeveStatus.EndFaceFarAwayFromStructure;
            }
            if(SleeveEndFaceInsideStructure(sleeve)) {
                return SleeveStatus.EndFaceInsideStructure;
            }
            if(SleeveAxisNotParallelToMep(sleeve)) {
                return SleeveStatus.AxisNotParallelToMepElement;
            }
            if(SleeveAxisDistanceTooBig(sleeve)) {
                return SleeveStatus.AxisDistanceTooBig;
            }
            if(SleeveIsIntersectsOther(sleeve)) {
                return SleeveStatus.IntersectSleeve;
            }
            return SleeveStatus.Correct;

        } catch(Autodesk.Revit.Exceptions.ApplicationException) {
            return SleeveStatus.Invalid;
        }
    }

    private void ClearCache() {
        _intersectingPipesCache = null;
        _intersectingOpeningsCache = null;
        _intersectingWallsCache = null;
        _intersectingFloorsCache = null;
        _intersectingUnacceptableStructuresCache = null;
        _structureDocumentTransformCache = null;
    }

    private bool SleeveIsInvalid(SleeveModel sleeve) {
        return GetSolid(sleeve.GetFamilyInstance())?.GetVolumeOrDefault(0)
            <= _revitRepository.Application.ShortCurveTolerance
            * _revitRepository.Application.ShortCurveTolerance
            * _revitRepository.Application.ShortCurveTolerance;
    }

    private bool SleeveIsEmpty(SleeveModel sleeve) {
        if(_intersectingPipesCache is null) {
            FindIntersectionsWithMepElements(sleeve);
        }
        return _intersectingPipesCache.Length == 0;
    }

    private bool SleeveIsOutsideOfStructure(SleeveModel sleeve) {
        if(_intersectingFloorsCache is null || _intersectingWallsCache is null || _intersectingOpeningsCache is null) {
            FindIntersectionsWithStructures(sleeve);
        }
        return _intersectingFloorsCache.Length == 0
            && _intersectingWallsCache.Length == 0
            && _intersectingOpeningsCache.Length == 0;
    }

    private bool SleeveIsInUnacceptableStructures(SleeveModel sleeve) {
        if(_intersectingUnacceptableStructuresCache is null) {
            FindIntersectionsWithStructures(sleeve);
        }
        return _intersectingUnacceptableStructuresCache.Length > 0;
    }

    private bool SleeveIsInDifferentStructures(SleeveModel sleeve) {
        if(_intersectingFloorsCache is null || _intersectingWallsCache is null || _intersectingOpeningsCache is null) {
            FindIntersectionsWithStructures(sleeve);
        }
        int walls = _intersectingWallsCache.Length + _intersectingOpeningsCache.Count(o => o.Host is Wall);
        int floors = _intersectingFloorsCache.Length + _intersectingOpeningsCache.Count(o => o.Host is Floor);
        return (walls > 0) && (floors > 0);
    }

    private bool SleeveBeyondOpening(SleeveModel sleeve) {
        if(_intersectingFloorsCache is null || _intersectingWallsCache is null || _intersectingOpeningsCache is null) {
            FindIntersectionsWithStructures(sleeve);
        }
        if(_intersectingOpeningsCache.Length == 0) {
            return false;
        }
        return _intersectingFloorsCache.Length > 0 || _intersectingWallsCache.Length > 0;
    }

    private bool SleeveIntersectsWithManyMepElements(SleeveModel sleeve) {
        if(_intersectingPipesCache is null) {
            FindIntersectionsWithMepElements(sleeve);
        }
        return _intersectingPipesCache.Length > 1;
    }

    private bool SleeveIsIrrelevant(SleeveModel sleeve) {
        if(_intersectingPipesCache is null) {
            FindIntersectionsWithMepElements(sleeve);
        }
        var pipeSolid = GetSolid(_intersectingPipesCache.FirstOrDefault());
        var sleeveSolid = GetSolid(sleeve.GetFamilyInstance());
        if(pipeSolid?.GetVolumeOrDefault() > 0 && sleeveSolid?.GetVolumeOrDefault() > 0) {
            var intersection = BooleanOperationsUtils.ExecuteBooleanOperation(
                pipeSolid, sleeveSolid, BooleanOperationsType.Intersect);
            var difference = BooleanOperationsUtils.ExecuteBooleanOperation(
                pipeSolid, sleeveSolid, BooleanOperationsType.Difference);
            return !IsCylinder(intersection)
                || SolidUtils.SplitVolumes(difference).Count != 2;
        } else {
            return true;
        }
    }

    private bool SleeveDiameterNotFound(SleeveModel sleeve, out DiameterRange range) {
        range = GetSleeveDiameterRange(sleeve);
        return range is null;
    }

    private bool SleeveIsTooBig(SleeveModel sleeve, DiameterRange range) {
        return _revitRepository.ConvertFromInternal(sleeve.Diameter) > range.SleeveDiameter;
    }

    private bool SleeveIsTooSmall(SleeveModel sleeve, DiameterRange range) {
        return _revitRepository.ConvertFromInternal(sleeve.Diameter) < range.SleeveDiameter;
    }

    private DiameterRange GetSleeveDiameterRange(SleeveModel sleeve) {
        if(_intersectingPipesCache is null) {
            FindIntersectionsWithMepElements(sleeve);
        }
        var pipe = _intersectingPipesCache.First();
        double diameter = pipe.GetParamValue<double>(BuiltInParameter.RBS_PIPE_OUTER_DIAMETER);
        return _config.PipeSettings.DiameterRanges.FirstOrDefault(
            r => _revitRepository.ConvertToInternal(r.StartMepSize) <= diameter
            && diameter <= _revitRepository.ConvertToInternal(r.EndMepSize));
    }

    private bool SleeveEndFaceFarAwayFromStructure(SleeveModel sleeve) {
        if(_intersectingFloorsCache is null
            || _intersectingWallsCache is null
            || _intersectingOpeningsCache is null
            || _structureDocumentTransformCache is null) {
            FindIntersectionsWithStructures(sleeve);
        }
        Wall[] walls = [.. _intersectingWallsCache,
            .. _intersectingOpeningsCache.Where(o => o.Host is Wall).Select(o => (Wall) o.Host)];
        if(walls.Length > 0) {
            return SleeveEndFaceFarAwayFromWall(sleeve, walls);
        }
        Floor[] floors = [.. _intersectingFloorsCache,
            .. _intersectingOpeningsCache.Where(o => o.Host is Floor).Select(o => (Floor) o.Host)];
        if(floors.Length > 0) {
            return SleeveEndFaceFarAwayFromFloor(sleeve, floors);
        }
        return false;
    }

    private bool SleeveEndFaceFarAwayFromWall(
        SleeveModel sleeve,
        Wall[] intersectingWalls) {

        var wallOrientation = intersectingWalls[0].Orientation;
        var sleeveOrientation = sleeve.GetOrientation();
        double angle = wallOrientation.AngleTo(sleeveOrientation);
        var sleeveSolid = SolidUtils.CreateTransformed(GetSolid(sleeve.GetFamilyInstance()),
            _structureDocumentTransformCache.Inverse);
        var wallSolids = intersectingWalls.Select(w => _geometryUtils.CreateWallSolid(w)).ToArray();
        var intersection = sleeveSolid;
        foreach(var wallSolid in wallSolids) {
            try {
                intersection = BooleanOperationsUtils.ExecuteBooleanOperation(
                    intersection, wallSolid, BooleanOperationsType.Difference);
            } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                // будем считать, что ошибка произошла из-за слегка смещенных граней солидов,
                // поэтому в этом случае с большой вероятностью грани гильзы находятся близко к граням стены.
                return false;
            }
        }
        return intersection.GetVolumeOrDefault(0)
            > Math.Abs(Math.Tan(angle)) * Math.PI * Math.Pow(sleeve.Diameter, 3) / 4;
    }

    private bool SleeveEndFaceFarAwayFromFloor(SleeveModel sleeve, Floor[] intersectingFloors) {
        double floorThickness = intersectingFloors.Sum(_geometryUtils.GetFloorThickness);
        double sleeveEndToTopOffset = _revitRepository.ConvertToInternal(
            _config.PipeSettings.Offsets[OffsetType.FromSleeveEndToTopFloorFace]);
        double sleeveRequiredLength = floorThickness + sleeveEndToTopOffset;
        double distanceTolerance = _revitRepository.Application.ShortCurveTolerance;

        var sleeveBbox = sleeve.GetFamilyInstance()
            .GetBoundingBox()
            .TransformBoundingBox(_structureDocumentTransformCache.Inverse);
        var floorBboxes = intersectingFloors.Select(f => f.GetBoundingBox()).ToList();
        var floorsBbox = new BoundingBoxXYZ() { Max = floorBboxes.GetMaxPoint(), Min = floorBboxes.GetMinPoint() };

        return sleeve.Length > (sleeveRequiredLength + distanceTolerance)
            || (sleeveBbox.Max.Z - floorsBbox.Max.Z) > (sleeveEndToTopOffset + distanceTolerance)
            || (floorsBbox.Min.Z - sleeveBbox.Min.Z) > distanceTolerance;
    }

    private bool SleeveEndFaceInsideStructure(SleeveModel sleeve) {
        if(_intersectingFloorsCache is null
            || _intersectingWallsCache is null
            || _intersectingOpeningsCache is null
            || _structureDocumentTransformCache is null) {
            FindIntersectionsWithStructures(sleeve);
        }
        // пересечение слегка увеличенного солида гильзы со всеми конструкциями
        var transform = Transform.CreateTranslation(-sleeve.Location)
            .Multiply(Transform.Identity.ScaleBasis(1.001))
            .Multiply(Transform.CreateTranslation(sleeve.Location))
            .Multiply(_structureDocumentTransformCache.Inverse);
        var sleeveSolid = SolidUtils.CreateTransformed(GetSolid(sleeve.GetFamilyInstance()), transform);
        Solid[] solids = [.. _intersectingFloorsCache.Select(GetSolid),
            .. _intersectingWallsCache.Select(GetSolid),
            .. _intersectingOpeningsCache.Select(_openingGeometryProvider.GetSolid)];
        var intersection = sleeveSolid;
        foreach(var solid in solids) {
            intersection = BooleanOperationsUtils.ExecuteBooleanOperation(
                intersection, solid, BooleanOperationsType.Difference);
        }
        double tolerance = _revitRepository.Application.ShortCurveTolerance
            * _revitRepository.Application.ShortCurveTolerance
            * _revitRepository.Application.ShortCurveTolerance;
        return intersection.GetVolumeOrDefault(0) <= tolerance;
    }

    private bool SleeveAxisNotParallelToMep(SleeveModel sleeve) {
        if(_intersectingPipesCache is null) {
            FindIntersectionsWithMepElements(sleeve);
        }
        var pipeDir = ((Line) ((LocationCurve) _intersectingPipesCache.First().Location).Curve).Direction;
        var sleeveDir = sleeve.Location;
        double angle = pipeDir.AngleTo(sleeveDir);
        return 0 <= angle && angle <= _revitRepository.Application.AngleTolerance
            || (Math.PI - _revitRepository.Application.AngleTolerance) <= angle && angle <= Math.PI;
    }

    private bool SleeveAxisDistanceTooBig(SleeveModel sleeve) {
        if(_intersectingPipesCache is null) {
            FindIntersectionsWithMepElements(sleeve);
        }
        var pipeLine = (Line) ((LocationCurve) _intersectingPipesCache.First().Location).Curve;
        var sleeveLocation = sleeve.Location;
        return pipeLine.Project(sleeveLocation).Distance
            > _revitRepository.ConvertToInternal(_config.PipeSettings.Offsets[OffsetType.FromSleeveAxisToMepAxis]);
    }

    private bool SleeveIsIntersectsOther(SleeveModel sleeve) {
        var bbox = sleeve.GetFamilyInstance().GetBoundingBox();
        return new FilteredElementCollector(_revitRepository.Document,
            [.. _revitRepository.GetSleeves().Select(s => s.Id)])
            .Excluding([sleeve.Id])
            .WherePasses(new BoundingBoxIntersectsFilter(new Outline(bbox.Min, bbox.Max)))
            .WherePasses(new ElementIntersectsElementFilter(sleeve.GetFamilyInstance()))
            .ToElementIds()
            .Count > 0;
    }

    private void FindIntersectionsWithMepElements(SleeveModel sleeve) {
        var famInst = sleeve.GetFamilyInstance();
        var bbox = famInst.GetBoundingBox();
        _intersectingPipesCache = [.. new FilteredElementCollector(_revitRepository.Document)
            .WhereElementIsNotElementType()
            .OfClass(typeof(Pipe))
            .WherePasses(new BoundingBoxIntersectsFilter(new Outline(bbox.Min, bbox.Max)))
            .WherePasses(new ElementIntersectsElementFilter(famInst))
            .ToElements()
            .OfType<Pipe>()];
    }

    private void FindIntersectionsWithStructures(SleeveModel sleeve) {
        var links = _structureLinksProvider.GetLinks();
        string[] openingFamNames = _structureLinksProvider.GetOpeningFamilyNames();
        foreach(var link in links) {
            _structureDocumentTransformCache = link.GetTransform();
            var sleeveBBox = sleeve.GetFamilyInstance()
                .GetBoundingBox()
                .TransformBoundingBox(_structureDocumentTransformCache.Inverse);
            var sleeveOutline = new Outline(sleeveBBox.Min, sleeveBBox.Max);
            var sleeveSolid = SolidUtils.CreateTransformed(
                GetSolid(sleeve.GetFamilyInstance()),
                _structureDocumentTransformCache.Inverse);
            var bboxFilter = new BoundingBoxIntersectsFilter(sleeveOutline);
            var solidFilter = new ElementIntersectsSolidFilter(sleeveSolid);

            _intersectingWallsCache = [.. new FilteredElementCollector(link.GetLinkDocument())
                .WhereElementIsNotElementType()
                .OfClass(typeof(Wall))
                .WherePasses(bboxFilter)
                .WherePasses(solidFilter)
                .ToElements()
                .OfType<Wall>()];
            _intersectingFloorsCache = [.. new FilteredElementCollector(link.GetLinkDocument())
                .WhereElementIsNotElementType()
                .OfClass(typeof(Floor))
                .WherePasses(bboxFilter)
                .WherePasses(solidFilter)
                .ToElements()
                .OfType<Floor>()];
            _intersectingOpeningsCache = [.. new FilteredElementCollector(link.GetLinkDocument())
                .WhereElementIsNotElementType()
                .OfClass(typeof(FamilyInstance))
                .WherePasses(bboxFilter)
                .ToElements()
                .OfType<FamilyInstance>()
                .Where(f => openingFamNames.Contains(f?.Symbol?.FamilyName))
                .Where(f => SolidIntersects(_openingGeometryProvider.GetSolid(f), sleeveSolid))];
            _intersectingUnacceptableStructuresCache = [.. new FilteredElementCollector(link.GetLinkDocument())
                .WhereElementIsNotElementType()
                .WherePasses(new ElementMulticategoryFilter(_unacceptableConstructions))
                .WherePasses(bboxFilter)
                .WherePasses(solidFilter)
                .ToElements()];

            if(_intersectingWallsCache.Any() || _intersectingFloorsCache.Any() || _intersectingOpeningsCache.Any()) {
                return;
            }
        }
    }

    private bool SolidIntersects(Solid solid1, Solid solid2) {
        try {
            return BooleanOperationsUtils.ExecuteBooleanOperation(
                solid1, solid2, BooleanOperationsType.Intersect)?.GetVolumeOrDefault() > 0;
        } catch(Autodesk.Revit.Exceptions.ApplicationException) {
            return false;
        }
    }

    private Solid GetSolid(Element element) {
        return element?.GetSolids().OrderByDescending(s => s.GetVolumeOrDefault() ?? 0).FirstOrDefault();
    }

    private bool IsCylinder(Solid solid) {
        if(solid is null || solid.GetVolumeOrDefault(0) == 0) {
            return false;
        }
        double areaTolerance = _revitRepository.Application.ShortCurveTolerance
            * _revitRepository.Application.ShortCurveTolerance;
        var faces = solid.Faces.OfType<Face>().ToArray();
        var planarFaces = faces.OfType<PlanarFace>().ToArray();
        var cylindricFaces = faces.OfType<CylindricalFace>().ToArray();
        return planarFaces.Length == 2
            && cylindricFaces.Length >= 2
            && cylindricFaces.Length % 2 == 0
            && Math.Abs(planarFaces[1].Area - planarFaces[0].Area) <= areaTolerance
            && Math.Abs(cylindricFaces[1].Area - cylindricFaces[0].Area) <= areaTolerance;
    }
}
