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
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Revit;
using dosymep.Revit.Geometry;
using dosymep.SimpleServices;

using RevitSetLevelSection.Models.LevelDefinitions;
using RevitSetLevelSection.Models.LevelDefinitions.BBPositions;
using RevitSetLevelSection.Models.LevelDefinitions.LevelProviders;

using InvalidOperationException = Autodesk.Revit.Exceptions.InvalidOperationException;

namespace RevitSetLevelSection.Models {
    internal class RevitRepository {
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

        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;
        
        public ProjectInfo ProjectInfo => Document.ProjectInformation;

        public Element GetElements(ElementId elementId) {
            return Document.GetElement(elementId);
        }

        public TransactionGroup StartTransactionGroup(string transactionGroupName) {
            return Document.StartTransactionGroup(transactionGroupName);
        }

        public IEnumerable<RevitLinkType> GetRevitLinkTypes() {
            return new FilteredElementCollector(Document)
                .WhereElementIsElementType()
                .OfClass(typeof(RevitLinkType))
                .OfType<RevitLinkType>()
                .ToList();
        }
        
        public IEnumerable<RevitLinkInstance> GetLinkInstances() {
            return new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfClass(typeof(RevitLinkInstance))
                .OfType<RevitLinkInstance>()
                .ToList();
        }

        public void SetLevelParam(RevitParam revitParam) {
            using(Transaction transaction =
                  Document.StartTransaction($"Установка уровня/секции \"{revitParam.Name}\"")) {

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
            return new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Levels)
                .OfType<Level>()
                .ToList();
        }

        private string GetLevelName(Element element, List<Level> levels) {
            // var outline = GetOutline(element, Transform.Identity);
            // if(_algorithms.TryGetValue(element.Category.Id, out var levelDefinition)) {
            //     return levelDefinition.GetLevelName(outline, levels);
            // }

            return null;
        }

        public void UpdateElements(RevitParam revitParam, string paramValue) {
            using(Transaction transaction = Document.StartTransaction($"Установка уровня/секции \"{revitParam.Name}\"")) {
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
            List<Element> elements = GetElements(paramOption.RevitParam);
            var cashedElements = elements.ToDictionary(item => item.Id);

            using(Transaction transaction =
                  Document.StartTransaction($"Установка уровня/секции \"{paramOption.RevitParam.Name}\"")) {
                
                var logger = ServicesProvider.GetPlatformService<ILoggerService>()
                    .ForPluginContext("Установка уровня\\секции");

                var intersectImpl = new IntersectImpl() {LinkedTransform = transform, Application = Application};
                foreach(Element element in elements) {
                    if(!cashedElements.ContainsKey(element.Id)) {
                        continue;
                    }

                    int? skip = element.GetParamValueOrDefault<int?>(SharedParamsConfig.Instance.FixBuildingWorks);
                    if(skip == 1) {
                        cashedElements.Remove(element.Id);
                        continue;
                    }

                    foreach(FamilyInstance massObject in massElements) {
                        if(intersectImpl.IsIntersect(massObject, element)) {
                            try {
                                string paramValue = massObject.GetParamValue<string>(paramOption);
                                element.SetParamValue(paramOption.RevitParam, paramValue);

                                if(!string.IsNullOrEmpty(paramOption.AdskParamName)
                                   && element.IsExistsSharedParam(paramOption.AdskParamName)) {
                                    element.SetSharedParamValue(paramOption.AdskParamName, paramValue);
                                }
                            } catch(InvalidOperationException ex) {
                                // решили что существует много вариантов,
                                // когда параметр не может заполнится из-за настроек в ревите
                                // Например: базовая стена внутри составной

                                logger.Warning(ex, 
                                    "Не был обновлен элемент {@elementId} в документе {documentId}.",
                                    element.Id.IntegerValue, Document.GetUniqId());
                            }

                            cashedElements.Remove(element.Id);
                            break;
                        }
                    }
                }

                foreach(Element element in cashedElements.Values) {
                    element.RemoveParamValue(paramOption.RevitParam);
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
            return new FilteredElementCollector(Document)
                .WhereElementIsNotElementType()
                .WherePasses(catFilter)
                .ToList();
        }
        
        private ElementId[] GetCategories(RevitParam revitParam) {
            return Document.GetParameterBindings()
                .Where(item => item.Binding.IsInstanceBinding())
                .Where(item => revitParam.IsRevitParam(Document, item.Definition))
                .SelectMany(item => item.Binding.GetCategories())
                .Select(item => item.Id)
                .ToArray();
        }

        public Workset GetWorkset(RevitLinkType revitLinkType) {
            return Document.GetWorksetTable().GetWorkset(revitLinkType.WorksetId);
        }
    }
}
