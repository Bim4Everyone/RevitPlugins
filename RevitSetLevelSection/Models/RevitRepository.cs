using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using DevExpress.Xpf.Core.FilteringUI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

using RevitSetLevelSection.Models.LevelDefinitions;

using InvalidOperationException = Autodesk.Revit.Exceptions.InvalidOperationException;

namespace RevitSetLevelSection.Models {
    internal class RevitRepository {
        public static readonly string AdskSectionNumberName = "ADSK_Номер секции";
        public static readonly string AdskBuildingNumberName = "ADSK_Номер здания";
        
        private readonly Application _application;
        private readonly UIApplication _uiApplication;

        private readonly Document _document;
        private readonly UIDocument _uiDocument;

        private Dictionary<ElementId, LevelDefinition> _algorithms
            = new Dictionary<ElementId, LevelDefinition>() {
                {
                    new ElementId(BuiltInCategory.OST_Walls),
                    new LevelDefinition() {
                        BBPosition = new BBPositionBottom(), LevelProvider = new LevelNearestProvider()
                    }
                }, {
                    new ElementId(BuiltInCategory.OST_Floors),
                    new LevelDefinition() {BBPosition = new BBPositionTop(), LevelProvider = new LevelNearestProvider()}
                }, {
                    new ElementId(BuiltInCategory.OST_Doors),
                    new LevelDefinition() {
                        BBPosition = new BBPositionBottom(), LevelProvider = new LevelNearestProvider()
                    }
                }, {
                    new ElementId(BuiltInCategory.OST_Windows),
                    new LevelDefinition() {
                        BBPosition = new BBPositionMiddle(), LevelProvider = new LevelBottomProvider()
                    }
                }, {
                    new ElementId(BuiltInCategory.OST_GenericModel),
                    new LevelDefinition() {
                        BBPosition = new BBPositionBottom(), LevelProvider = new LevelNearestProvider()
                    }
                }, {
                    new ElementId(BuiltInCategory.OST_Roofs),
                    new LevelDefinition() {
                        BBPosition = new BBPositionBottom(), LevelProvider = new LevelNearestProvider()
                    }
                }, {
                    new ElementId(BuiltInCategory.OST_Ceilings),
                    new LevelDefinition() {
                        BBPosition = new BBPositionBottom(), LevelProvider = new LevelNearestProvider()
                    }
                }, {
                    new ElementId(BuiltInCategory.OST_Columns),
                    new LevelDefinition() {
                        BBPosition = new BBPositionBottom(), LevelProvider = new LevelNearestProvider()
                    }
                }, {
                    new ElementId(BuiltInCategory.OST_Parts),
                    new LevelDefinition() {
                        BBPosition = new BBPositionBottom(), LevelProvider = new LevelNearestProvider()
                    }
                }, {
                    new ElementId(BuiltInCategory.OST_Gutter),
                    new LevelDefinition() {
                        BBPosition = new BBPositionBottom(), LevelProvider = new LevelNearestProvider()
                    }
                }, {
                    new ElementId(BuiltInCategory.OST_StairsRailing),
                    new LevelDefinition() {
                        BBPosition = new BBPositionBottom(), LevelProvider = new LevelNearestProvider()
                    }
                },
            };

        public RevitRepository(Application application, Document document) {
            _application = application;
            _uiApplication = new UIApplication(application);

            _document = document;
            _uiDocument = new UIDocument(document);
        }

        public Document Document => _document;
        public Application Application => _application;

        public ProjectInfo ProjectInfo => _document.ProjectInformation;

        public Element GetElements(ElementId elementId) {
            return _document.GetElement(elementId);
        }

        public TransactionGroup StartTransactionGroup(string transactionGroupName) {
            return _document.StartTransactionGroup(transactionGroupName);
        }

        public IEnumerable<RevitLinkType> GetRevitLinkTypes() {
            return new FilteredElementCollector(_document)
                .WhereElementIsElementType()
                .OfClass(typeof(RevitLinkType))
                .OfType<RevitLinkType>()
                .ToList();
        }
        
        public IEnumerable<RevitLinkInstance> GetLinkInstances() {
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(RevitLinkInstance))
                .OfType<RevitLinkInstance>()
                .ToList();
        }

        public void SetLevelParam(RevitParam revitParam) {
            using(Transaction transaction =
                  _document.StartTransaction($"Установка уровня/секции \"{revitParam.Name}\"")) {

                List<Level> levels = GetLevels();
                IEnumerable<Element> elements = GetElements(revitParam);
                foreach(Element element in elements) {
                    try {
                        string paramValue = GetLevelName(element, levels);
                        element.SetParamValue(revitParam, paramValue);
                    } catch { }
                }

                transaction.Commit();
            }
        }

        private List<Level> GetLevels() {
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Levels)
                .OfType<Level>()
                .ToList();
        }

        private string GetLevelName(Element element, List<Level> levels) {
            var outline = GetOutline(element, Transform.Identity);
            if(_algorithms.TryGetValue(element.Category.Id, out var levelDefinition)) {
                return levelDefinition.GetLevelName(outline, levels);
            }

            return null;
        }

        public void UpdateElements(RevitParam revitParam, string paramValue) {
            using(Transaction transaction = _document.StartTransaction($"Установка уровня/секции \"{revitParam.Name}\"")) {
                ProjectInfo.SetParamValue(revitParam, paramValue);
                IEnumerable<Element> elements = GetElements(revitParam);

                foreach(Element element in elements) {
                    element.SetParamValue(revitParam, paramValue);
                }

                transaction.Commit();
            }
        }

        public void UpdateElements(ParamOption paramOption, Transform transform,
            IEnumerable<FamilyInstance> massElements) {
            List<Element> elements = GetElements(paramOption.SharedRevitParam);
            var cashedElements = elements.ToDictionary(item => item.Id);

            using(Transaction transaction =
                  _document.StartTransaction($"Установка уровня/секции \"{paramOption.SharedRevitParam.Name}\"")) {
                
                foreach(Element element in elements) {
                    if(!cashedElements.ContainsKey(element.Id)) {
                        continue;
                    }

                    bool? skip = element.GetParamValueOrDefault<bool?>(SharedParamsConfig.Instance.FixBuildingWorks);
                    if(skip == true) {
                        continue;
                    }

                    foreach(FamilyInstance massObject in massElements) {
                        if(IsIntersectCenterElement(transform, massObject, element)) {
                            try {
                                string paramValue = massObject.GetParamValue<string>(paramOption);
                                element.SetParamValue(paramOption.SharedRevitParam, paramValue);

                                if(!string.IsNullOrEmpty(paramOption.AdskParamName)
                                   && element.IsExistsSharedParam(paramOption.AdskParamName)) {
                                    element.SetSharedParamValue(paramOption.AdskParamName, paramValue);
                                }
                            } catch(InvalidOperationException) {
                                // решили что существует много вариантов,
                                // когда параметр не может заполнится из-за настроек в ревите
                                // Например: базовая стена внутри составной
                            }

                            cashedElements.Remove(element.Id);
                            break;
                        }
                    }
                }

                foreach(Element element in cashedElements.Values) {
                    element.RemoveParamValue(paramOption.SharedRevitParam);
                    if(!string.IsNullOrEmpty(paramOption.AdskParamName)
                       && element.IsExistsSharedParam(paramOption.AdskParamName)) {
                        element.RemoveSharedParamValue(paramOption.AdskParamName);
                    }
                }

                transaction.Commit();
            }
        }

        private List<Element> GetElements(RevitParam revitParam) {
            var catFilter = new ElementMulticategoryFilter(GetCategories(revitParam));
            return new FilteredElementCollector(_document)
                .WhereElementIsNotElementType()
                .WherePasses(catFilter)
                .ToList();
        }

        private bool IsIntersectCenterElement(Transform transform, FamilyInstance massElement, Element element) {
            Solid solid = GetSolid(massElement, transform);
            if(solid == null) {
                return false;
            }

            XYZ elementCenterPoint = GetCenterPoint(element);
            var line = GetLine(elementCenterPoint);

            var result = solid.IntersectWithCurve(line,
                new SolidCurveIntersectionOptions() {ResultType = SolidCurveIntersectionMode.CurveSegmentsInside});

            if(result.ResultType == SolidCurveIntersectionMode.CurveSegmentsInside) {
                return result.Any(item => item.Length > 0);
            }

            return false;
        }

        private XYZ GetCenterPoint(Element element) {
            try {
                Solid solid = GetSolid(element, Transform.Identity);
                if(solid != null) {
                    return solid.ComputeCentroid();
                }
            } catch {
                
            }

            var elementOutline = GetOutline(element, Transform.Identity);
            return (elementOutline.MaximumPoint - elementOutline.MinimumPoint) / 2
                   + elementOutline.MinimumPoint;
        }

        private Outline GetOutline(Element element, Transform transform) {
            var boundingBox = element.get_BoundingBox(null);
            if(boundingBox == null) {
                return new Outline(XYZ.Zero, XYZ.Zero);
            }

            return new Outline(transform.OfPoint(boundingBox.Min), transform.OfPoint(boundingBox.Max));
        }

        private Solid GetSolid(Element element, Transform transform) {
            var geometryElement = element.get_Geometry(new Options() {ComputeReferences = true});
            if(geometryElement == null) {
                return null;
            }

            List<Solid> solids = new List<Solid>();
            foreach(GeometryObject geometryObject in geometryElement.OfType<GeometryObject>()) {
                if(geometryObject is Solid solid) {
                    solids.Add(solid);
                } else if(geometryObject is GeometryInstance instance) {
                    solids.AddRange(instance.GetInstanceGeometry().OfType<Solid>());
                }
            }

            solids = solids
                    .Where(item => item.Volume > 0)
                    .ToList();

            if(solids.Count == 0) {
                return null;
            }

            Solid resultSolid = solids.First();
            solids.Remove(resultSolid);

            try {
                foreach(Solid solid in solids) {
                    resultSolid =
                        BooleanOperationsUtils.ExecuteBooleanOperation(resultSolid, solid, BooleanOperationsType.Union);
                }
            } catch(InvalidOperationException) {
                return null;
            }

            return SolidUtils.CreateTransformed(resultSolid, transform);
        }

        private Line GetLine(XYZ point) {
            XYZ start = point.Subtract(new XYZ(_application.ShortCurveTolerance, 0, 0));
            XYZ finish = point.Add(new XYZ(_application.ShortCurveTolerance, 0, 0));

            return Line.CreateBound(start, finish);
        }

        private ElementId[] GetCategories(RevitParam revitParam) {
            return _document.GetParameterBindings()
                .Where(item => item.Binding.IsInstanceBinding())
                .Where(item => revitParam.IsRevitParam(_document, item.Definition))
                .SelectMany(item => item.Binding.GetCategories())
                .Select(item => item.Id)
                .ToArray();
        }

        public Workset GetWorkset(RevitLinkType revitLinkType) {
            return _document.GetWorksetTable().GetWorkset(revitLinkType.WorksetId);
        }
    }
}
