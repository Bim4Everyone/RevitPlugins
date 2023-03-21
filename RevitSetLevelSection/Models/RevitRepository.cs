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

using RevitSetLevelSection.Models.ElementPositions;
using RevitSetLevelSection.Models.LevelProviders;

using InvalidOperationException = Autodesk.Revit.Exceptions.InvalidOperationException;

namespace RevitSetLevelSection.Models {
    internal class RevitRepository {
        private readonly IBimModelPartsService _bimModelPartsService;

        public RevitRepository(UIApplication uiApplication, IBimModelPartsService bimModelPartsService) {
            UIApplication = uiApplication;
            _bimModelPartsService = bimModelPartsService;
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
            using(Transaction transaction =
                  Document.StartTransaction($"Установка уровня/секции \"{revitParam.Name}\"")) {
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

        public IEnumerable<MainBimBuildPart> GetBuildParts() {
            yield return MainBimBuildPart.ARPart;
            yield return MainBimBuildPart.KRPart;
            yield return MainBimBuildPart.VisPart;
        }
        
        public MainBimBuildPart GetBuildPart() {
            if(_bimModelPartsService.InAnyBimModelParts(Document, BimModelPart.ARPart, BimModelPart.GPPart)) {
                return MainBimBuildPart.ARPart;
            }

            if(_bimModelPartsService.InAnyBimModelParts(Document, BimModelPart.KRPart, BimModelPart.KMPart)) {
                return MainBimBuildPart.KRPart;
            }
            
            if(_bimModelPartsService.InAnyBimModelParts(Document, BimModelPart.KOORDPart)) {
                return MainBimBuildPart.KOORDPart;
            }

            // Будем считать что все остальные это ВИС
            // ошибки будущего - ошибки будущего :D
            return MainBimBuildPart.VisPart;
        }
    }

    internal class MainBimBuildPart : IEquatable<MainBimBuildPart> {
        public static readonly MainBimBuildPart ARPart = new MainBimBuildPart() {Id = 0, Name = "АР"};
        public static readonly MainBimBuildPart KRPart = new MainBimBuildPart() {Id = 1, Name = "КР"};
        public static readonly MainBimBuildPart VisPart = new MainBimBuildPart() {Id = 2, Name = "ВИС"};
        public static readonly MainBimBuildPart KOORDPart = new MainBimBuildPart() {Id = 3, Name = "КООРД"};

        public int Id { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        #region IEquatable<MainBimBuildPart>

        public bool Equals(MainBimBuildPart other) {
            if(ReferenceEquals(null, other)) {
                return false;
            }

            if(ReferenceEquals(this, other)) {
                return true;
            }

            return Id == other.Id;
        }

        public override bool Equals(object obj) {
            if(ReferenceEquals(null, obj)) {
                return false;
            }

            if(ReferenceEquals(this, obj)) {
                return true;
            }

            if(obj.GetType() != this.GetType()) {
                return false;
            }

            return Equals((MainBimBuildPart) obj);
        }

        public override int GetHashCode() {
            return Id;
        }

        public static bool operator ==(MainBimBuildPart left, MainBimBuildPart right) {
            return Equals(left, right);
        }

        public static bool operator !=(MainBimBuildPart left, MainBimBuildPart right) {
            return !Equals(left, right);
        }

        #endregion
    }
}
