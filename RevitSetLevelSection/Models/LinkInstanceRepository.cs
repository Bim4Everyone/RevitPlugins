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
using dosymep.Revit.Geometry;

namespace RevitSetLevelSection.Models {
    internal class LinkInstanceRepository {
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

        public IEnumerable<DesignOption> GetDesignOptions() {
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(DesignOption))
                .OfType<DesignOption>()
                .ToList();
        }

        public IEnumerable<FamilyInstance> GetMassElements(IDesignOption designOption) {
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Mass)
                .Where(item => GetDesignOptionId(item) == designOption.Id)
                .OfType<FamilyInstance>()
                .ToList();
        }

        public bool HasIntersects(IEnumerable<FamilyInstance> massObjects) {
            return massObjects
                .SelectMany(item => item.GetSolids())
                .Where(item => item.Volume > 0)
                .HasIntersects();
        }

        public bool LinkIsLoaded() {
            return _revitLinkType.GetLinkedFileStatus() == LinkedFileStatus.Loaded;
        }

        public bool LoadLinkDocument() {
            if(_revitLinkType.GetLinkedFileStatus() == LinkedFileStatus.InClosedWorkset) {
                Workset workset = _revitRepository.GetWorkset(_revitLinkType);
                TaskDialog.Show("Предупреждение!", $"Откройте рабочий набор \"{workset.Name}\"."
                                                   + Environment.NewLine
                                                   + "Загрузка связанного файла из закрытого рабочего набора не поддерживается!");

                return false;
            }

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
        
        public IEnumerable<Area> GetAreas() {
            var areaFilter = new AreaFilter(GetAreaScheme());
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Areas)
                .OfType<Area>()
                .Where(item => areaFilter.AllowElement(item));
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