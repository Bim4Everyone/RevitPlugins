using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using dosymep.Revit;
using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit.Geometry;

using RevitSetLevelSection.Models.Repositories;

namespace RevitSetLevelSection.Models {
    internal class LinkInstanceRepository : IZoneRepository, IMassRepository {
        public static readonly string AreaSchemeName = "Назначение этажа СМР";

        private readonly RevitLinkType _revitLinkType;
        private readonly RevitRepository _revitRepository;

        private Document _document;
        private RevitLinkInstance _linkInstance;

        public LinkInstanceRepository(RevitRepository revitRepository, RevitLinkType revitLinkType) {
            _revitRepository = revitRepository;
            _revitLinkType = revitLinkType;

            Update();
        }

        public Transform Transform { get; private set; }

        public Workset GetWorkset() {
            return _revitRepository.Document.GetWorksetTable().GetWorkset(_revitLinkType.WorksetId);
        }

        public IEnumerable<DesignOption> GetDesignOptions() {
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(DesignOption))
                .OfType<DesignOption>()
                .ToList();
        }

        public List<FamilyInstance> GetElements() {
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Mass)
                .OfType<FamilyInstance>()
                .ToList();
        }

        public List<FamilyInstance> GetElements(IDesignOption designOption) {
            return GetElements()
                .Where(item => GetDesignOptionId(item) == designOption.Id)
                .ToList();
        }

        public bool HasIntersects(IDesignOption designOption) {
            return GetElements(designOption)
                .SelectMany(item => item.GetSolids())
                .Where(item => item.Volume > 0)
                .HasIntersects();
        }

        public bool LinkIsLoaded() {
            return _revitLinkType.GetLinkedFileStatus() == LinkedFileStatus.Loaded;
        }

        public bool LoadLinkDocument() {
            var loadResult = _revitLinkType.Load();
            if(loadResult.LoadResult == LinkLoadResultType.LinkLoaded) {
                Update();
                return true;
            }

            return false;
        }

        public IEnumerable<string> GetPartNames() {
            return GetPartNames(GetParamNames());
        }

        public IEnumerable<string> GetPartNames(IEnumerable<string> paramNames) {
            if(_document == null) {
                yield break;
            }

            yield return "Без раздела";

            foreach(string paramName in paramNames) {
                foreach(ParameterElement paramElement in _document.GetProjectParams().OrderBy(item => item.Name)) {
                    if(paramElement.Name.StartsWith(paramName, StringComparison.CurrentCultureIgnoreCase)) {
                        yield return paramElement.Name.Replace(paramName, string.Empty);
                    }
                }
            }
        }

        public AreaScheme GetAreaScheme() {
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_AreaSchemes)
                .OfType<AreaScheme>()
                .FirstOrDefault(item => item.Name.Equals(AreaSchemeName));
        }

        public List<ZoneInfo> GetZones() {
            var areaFilter = new AreaFilter(GetAreaScheme());
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Areas)
                .OfType<Area>()
                .Where(item => areaFilter.AllowElement(item))
                .Where(item => HasAreaLevel(item))
                .Select(item => new ZoneInfo() {Area = item, Level = GetLevel(item), Solid = CreateSolid(item)})
                .ToList();
        }

        /// <summary>
        /// Возвращает трансформированный <see cref="Solid"/> по границам зоны расположенный на Z=0.
        /// </summary>
        /// <param name="area">Зона.</param>
        /// <returns>Возвращает трансформированный <see cref="Solid"/> по границам зоны расположенный на Z=0.</returns>
        private Solid CreateSolid(Area area) {
            // Зоны являются замкнутыми и с простым одним контуром
            var boundarySegments = area.GetBoundarySegments(SpatialElementExtensions.DefaultBoundaryOptions)
                .First();

            Transform transform = CreateAreaTransform(area);
            var curves = boundarySegments
                .Select(item => item.GetCurve())
                .Select(item => item.CreateTransformed(Transform))
                .Select(item => item.CreateTransformed(transform))
                .ToList();

            var curveLoops = new[] {CurveLoop.Create(curves)};
            return GeometryCreationUtilities.CreateExtrusionGeometry(curveLoops, XYZ.BasisZ, 10);
        }

        private Transform CreateAreaTransform(Area area) {
            XYZ areaPoint = ((LocationPoint) area.Location).Point;
            areaPoint = Transform.OfPoint(areaPoint);
            return Transform.CreateTranslation(new XYZ(0, 0, -areaPoint.Z));
        }

        public bool HasAreaLevel(Area area) {
            return GetLevel(area) != null;
        }

        public Level GetLevel(Area area) {
            var paramValue = area.GetParamValue<string>(SharedParamsConfig.Instance.FixComment);

#if REVIT_2023_OR_LESS
            if(int.TryParse(paramValue, out int elementId)) {
                return _document.GetElement(new ElementId(elementId)) as Level;
            }
#else
            if(long.TryParse(paramValue, out long elementId)) {
                return _document.GetElement(new ElementId(elementId)) as Level;
            }
#endif

            return null;
        }

        private void Update() {
            _linkInstance = _revitRepository.GetLinkInstances()
                .FirstOrDefault(item => item.GetTypeId() == _revitLinkType.Id);

            if(_linkInstance != null) {
                Transform = _linkInstance.GetTransform();
                _document = _linkInstance.GetLinkDocument();
            }
        }

        private IEnumerable<string> GetParamNames() {
            yield return ParamOption.BuildingWorksTypingName;
            yield return ParamOption.BuildingWorksSectionName;
            yield return ParamOption.BuildingWorksBlockName;
        }

        private ElementId GetDesignOptionId(Element element) {
            return element.DesignOption?.Id ?? ElementId.InvalidElementId;
        }
    }

    internal static class Extensions {
        public static bool HasIntersects(this IEnumerable<Solid> args) {
            List<Solid> solids = args.ToList();
            if(solids.Count == 0) {
                return false;
            }

            Solid solid0 = solids[0];
            for(int index = 1; index < solids.Count; index++) {
                Solid solid1 = solids[index];
                try {
                    if(HasIntersection(solid0, solid1)) {
                        return true;
                    }
                } catch {
                    continue;
                }
            }

            return false;
        }

        private static bool HasIntersection(Solid solid0, Solid solid1) {
            try {
                var intersection =
                    BooleanOperationsUtils.ExecuteBooleanOperation(solid0, solid1, BooleanOperationsType.Intersect);
                return intersection.Volume > 0;
            } catch {
                return false;
            }
        }
    }

    internal class AreaFilter : ISelectionFilter {
        private readonly AreaScheme _areaScheme;

        public AreaFilter(AreaScheme areaScheme) {
            _areaScheme = areaScheme;
        }

        public bool AllowElement(Element elem) {
            var area = elem as Area;
            return area != null && _areaScheme?.Id == area.AreaScheme.Id;
        }

        public bool AllowReference(Reference reference, XYZ position) {
            return false;
        }
    }
}