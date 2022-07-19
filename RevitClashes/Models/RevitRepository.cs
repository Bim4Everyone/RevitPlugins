using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitClashDetective.Models.Handlers;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.FilterableValueProviders;

using ParameterValueProvider = RevitClashDetective.Models.FilterableValueProviders.ParameterValueProvider;

namespace RevitClashDetective.Models {
    internal class RevitRepository {
        private readonly Application _application;
        private readonly UIApplication _uiApplication;

        private readonly Document _document;
        private readonly UIDocument _uiDocument;
        private readonly RevitEventHandler _revitEventHandler;
        private List<string> _endings = new List<string> { "_отсоединено", "_detached" };
        private string _clashViewName = "BIM_Проверка на коллизии";
        private View3D _view;

        public RevitRepository(Application application, Document document) {
            _application = application;
            _uiApplication = new UIApplication(application);

            _document = document;
            _uiDocument = new UIDocument(document);

            _revitEventHandler = new RevitEventHandler();
            _endings.Add("_" + _application.Username);

            _view = GetClashView();
        }

        public static string ProfilePath {
            get {
                var path = @"T:\Проектный институт\Отдел стандартизации BIM и RD\BIM-Ресурсы\5-Надстройки\Bim4Everyone\A101";
                if(!Directory.Exists(path)) {
                    path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "dosymep");
                }
                return path;
            }
        }

        public static List<BuiltInParameter> BaseLevelParameters => new List<BuiltInParameter>() {
            BuiltInParameter.MULTISTORY_STAIRS_REF_LEVEL,
            BuiltInParameter.DPART_BASE_LEVEL,
            BuiltInParameter.STAIRS_BASE_LEVEL,
            BuiltInParameter.FABRICATION_LEVEL_PARAM,
            BuiltInParameter.TRUSS_ELEMENT_REFERENCE_LEVEL_PARAM,
            BuiltInParameter.GROUP_LEVEL,
            BuiltInParameter.SPACE_REFERENCE_LEVEL_PARAM,
            BuiltInParameter.RBS_START_LEVEL_PARAM,
            BuiltInParameter.STAIRS_RAILING_BASE_LEVEL_PARAM,
            BuiltInParameter.IMPORT_BASE_LEVEL,
            BuiltInParameter.STAIRS_BASE_LEVEL_PARAM,
            BuiltInParameter.SCHEDULE_BASE_LEVEL_PARAM,
            BuiltInParameter.FACEROOF_LEVEL_PARAM,
            BuiltInParameter.ROOF_BASE_LEVEL_PARAM,
            BuiltInParameter.ROOF_CONSTRAINT_LEVEL_PARAM,
            BuiltInParameter.INSTANCE_REFERENCE_LEVEL_PARAM };

        public Document Doc => _document;

        public UIApplication UiApplication => _uiApplication;

        public View3D GetClashView() {
            var view = new FilteredElementCollector(_document)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .FirstOrDefault(item => item.Name.Equals(_clashViewName + "_" + _application.Username, StringComparison.CurrentCultureIgnoreCase));
            if(view == null) {
                using(Transaction t = _document.StartTransaction("Создание 3D-вида")) {
                    var type = new FilteredElementCollector(_document)
                        .OfClass(typeof(ViewFamilyType))
                        .Cast<ViewFamilyType>()
                        .First(v => v.ViewFamily == ViewFamily.ThreeDimensional);
                    view = View3D.CreateIsometric(_document, type.Id);
                    view.Name = _clashViewName + "_" + _application.Username;
                    var categories = new[] { new ElementId(BuiltInCategory.OST_Levels),
                        new ElementId(BuiltInCategory.OST_WallRefPlanes),
                        new ElementId(BuiltInCategory.OST_Grids),
                        new ElementId(BuiltInCategory.OST_VolumeOfInterest) };

                    foreach(var category in categories) {
                        if(view.CanCategoryBeHidden(category)) {
                            view.SetCategoryHidden(category, true);
                        }
                    }

                    var bimGroup = new FilteredElementCollector(_document)
                        .OfClass(typeof(View3D))
                        .Cast<View3D>()
                        .Where(item => !item.IsTemplate)
                        .Select(item => item.GetParamValueOrDefault<string>(ProjectParamsConfig.Instance.ViewGroup))
                        .FirstOrDefault(item => item != null && item.Contains("BIM"));
                    if(bimGroup != null) {
                        view.SetParamValue(ProjectParamsConfig.Instance.ViewGroup, bimGroup);
                    }

                    t.Commit();
                }
            }
            return view;
        }

        public string GetDocumentName() {
            return GetDocumentName(_document);
        }

        public string GetDocumentName(Document doc) {
            var title = doc.Title;
            foreach(var ending in _endings) {
                if(title.IndexOf(ending) > -1) {
                    title = title.Substring(0, title.IndexOf(ending));
                }
            }
            return title;
        }

        public string GetObjectName() {
            return _document.Title.Split('_').FirstOrDefault();
        }

        public Element GetElement(ElementId id) {
            return _document.GetElement(id);
        }

        public Element GetElement(Document doc, ElementId id) {
            return doc.GetElement(id);
        }

        public List<Collector> GetCollectors() {
            return GetDocuments()
                .Select(item => new Collector(item))
                .ToList();
        }

        public List<WorksetCollector> GetWorksetCollectors() {
            return GetDocuments()
                .Select(item => new WorksetCollector(item))
                .ToList();
        }

        public View3D GetNavisworksView(Document doc) {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .FirstOrDefault(item => item.Name == "Navisworks");
        }

        public View3D Get3DView(Document doc) {
            return GetNavisworksView(doc) ?? new FilteredElementCollector(doc).OfClass(typeof(View3D)).Cast<View3D>().Where(item => !item.IsTemplate).FirstOrDefault();
        }

        public LanguageType GetLanguage() {
            return _application.Language;
        }

        public List<ParameterFilterElement> GetFilters() {
            return new FilteredElementCollector(_document)
                .OfClass(typeof(ParameterFilterElement))
                .Cast<ParameterFilterElement>()
                .Where(item => item.Name.StartsWith("BIM"))
                .ToList();
        }

        public List<RevitLinkInstance> GetRevitLinkInstances() {
            return new FilteredElementCollector(_document)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>()
                .Where(item => item.GetLinkDocument() != null && IsParentLink(item))
                .ToList();
        }

        private bool IsParentLink(RevitLinkInstance link) {
            if(link.GetTypeId().IsNotNull()) {
                var type = _document.GetElement(link.GetTypeId());
                if(type is RevitLinkType linkType) {
                    return !linkType.IsNestedLink;
                } else {
                    return false;
                }
            }
            return false;
        }

        public List<DocInfo> GetDocInfos() {
            var linkedDocuments = new FilteredElementCollector(_document)
               .OfClass(typeof(RevitLinkInstance))
               .Cast<RevitLinkInstance>()
               .Where(item => item.GetLinkDocument() != null)
               .Select(item => new DocInfo(GetDocumentName(item.GetLinkDocument()), item.GetLinkDocument(), item.GetTransform()))
               .ToList();
            linkedDocuments.Add(new DocInfo(GetDocumentName(), _document, Transform.Identity));
            return linkedDocuments;
        }

        public bool IsValidElement(Document doc, ElementId elementId) {
            return doc.GetElement(elementId) != null;
        }

        public void SelectElements(IEnumerable<Element> elements) {
            var selection = _uiApplication.ActiveUIDocument.Selection;
            selection.SetElementIds(elements.Select(e => e.Id).ToList());
        }

        public List<Category> GetCategories() {
            return ParameterFilterUtilities.GetAllFilterableCategories()
                .Select(item => Category.GetCategory(_document, item))
                .OfType<Category>()
                .ToList();
        }

        public Category GetCategory(BuiltInCategory builtInCategory) {
            return Category.GetCategory(_document, builtInCategory);
        }

        public List<ParameterValueProvider> GetParameters(Document doc, IEnumerable<Category> categories) {
            return categories
                .SelectMany(item => ParameterFilterUtilities.GetFilterableParametersInCommon(doc, new[] { item.Id }).Select(p => GetParam(doc, item, p)))
                .Where(item => item.RevitParam.Id != Enum.GetName(typeof(BuiltInParameter), BuiltInParameter.ELEM_PARTITION_PARAM))
                .GroupBy(item => item.Name)
                .Where(item => item.Count() > categories.Count() - 1)
                .SelectMany(item => FilterParamProviders(item))
                .ToList();
        }

        public IEnumerable<Document> GetDocuments() {
            var linkedDocuments = new FilteredElementCollector(_document)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>()
                .Select(item => item.GetLinkDocument())
                .Where(item => item != null)
                .ToList();
            linkedDocuments.Add(_document);
            return linkedDocuments;
        }

        public IEnumerable<Element> GetFilteredElements(Document doc, IEnumerable<ElementId> categories, ElementFilter filter) {
            return new FilteredElementCollector(doc)
                .WherePasses(new ElementMulticategoryFilter(categories.ToList()))
                .WhereElementIsNotElementType()
                .WherePasses(filter);
        }

        public void SelectAndShowElement(IEnumerable<ElementId> ids, BoundingBoxXYZ bb) {
            if(_document.ActiveView != _view) {
                _uiDocument.ActiveView = _view;
            }
            _revitEventHandler.TransactAction = () => {
                SetSectionBox(bb);
                if(ids.Where(item => item != ElementId.InvalidElementId).Any()) {
                    _uiDocument.Selection.SetElementIds(ids.Where(item => item != ElementId.InvalidElementId).ToArray());
                } else {
                    var border = _view.GetDependentElements(new ElementCategoryFilter(BuiltInCategory.OST_SectionBox)).FirstOrDefault();
                    if(border != null) {
                        _uiDocument.Selection.SetElementIds(new[] { border });
                    }
                }
            };

            _revitEventHandler.Raise();
        }

        public void DoAction(Action action) {
            _revitEventHandler.TransactAction = action; 
            _revitEventHandler.Raise();
        }


        public Transform GetLinkedDocumentTransform(string documTitle) {
            if(documTitle.Equals(GetDocumentName(), StringComparison.CurrentCultureIgnoreCase))
                return Transform.Identity;
            return GetRevitLinkInstances()
                .FirstOrDefault(item => GetDocumentName(item.GetLinkDocument()).Equals(documTitle, StringComparison.CurrentCultureIgnoreCase))
                ?.GetTotalTransform();
        }

        private ParameterValueProvider GetParam(Document doc, Category category, ElementId elementId) {
            var revitParam = ParameterInitializer.InitializeParameter(doc, elementId);
            if(elementId.IsSystemId()) {
                return new ParameterValueProvider(this, revitParam, $"{revitParam.Name} ({category.Name})");
            }
            return new ParameterValueProvider(this, revitParam);
        }

        private IEnumerable<ParameterValueProvider> FilterParamProviders(IEnumerable<ParameterValueProvider> providers) {
            if(providers.First().RevitParam is SystemParam) {
                if(providers.All(item => item.RevitParam.Id.Equals(providers.First().RevitParam.Id, StringComparison.CurrentCultureIgnoreCase))) {
                    providers.First().DisplayValue = providers.First().Name;
                    return new[] { providers.First() };
                }
                return providers;
            }
            return new[] { providers.First() };
        }

        private void SetSectionBox(BoundingBoxXYZ bb) {
            if(bb == null)
                return;
            using(Transaction t = _document.StartTransaction("Подрезка")) {
                bb.Max = bb.Max + new XYZ(5, 5, 5);
                bb.Min = bb.Min - new XYZ(5, 5, 5);
                _view.SetSectionBox(bb);
                var uiView = _uiDocument.GetOpenUIViews().FirstOrDefault(item => item.ViewId == _view.Id);
                if(uiView != null) {
                    uiView.ZoomAndCenterRectangle(bb.Min, bb.Max);
                }
                t.Commit();
            }
        }

        protected T GetPlatformService<T>() {
            return ServicesProvider.GetPlatformService<T>();
        }
    }
}