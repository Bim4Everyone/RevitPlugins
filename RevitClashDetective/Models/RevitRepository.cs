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

using dosymep.Bim4Everyone.SystemParams;
using dosymep.Revit;

using RevitClashDetective.Handlers;
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
        private readonly RevitEventHandler _revitEventHandler2;
        private List<string> _endings = new List<string> { "_отсоединено", "_detached" };
        private string _clashViewName = "BIM_Проверка на коллизии";
        private View3D _view;

        public RevitRepository(Application application, Document document) {
            _application = application;
            _uiApplication = new UIApplication(application);

            _document = document;
            _uiDocument = new UIDocument(document);

            _revitEventHandler = new RevitEventHandler();
            _revitEventHandler2 = new RevitEventHandler();
            _endings.Add("_" + _application.Username);

            _view = GetClashView();
        }

        public static string ProfilePath { get; } = @"T:\Проектный институт\Отдел стандартизации BIM и RD\BIM-Ресурсы\5-Надстройки\Bim4Everyone\A101";
        public static List<BuiltInParameter> BaseLevelParameters = new List<BuiltInParameter>() {
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

        public View3D GetClashView() {
            var view = new FilteredElementCollector(_document)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .FirstOrDefault(item => item.Name.Equals(_clashViewName, StringComparison.CurrentCultureIgnoreCase));
            if(view == null) {
                using(Transaction t = _document.StartTransaction("Создание 3D-вида")) {
                    var type = new FilteredElementCollector(_document)
                        .OfClass(typeof(ViewFamilyType))
                        .Cast<ViewFamilyType>()
                        .First(v => v.ViewFamily == ViewFamily.ThreeDimensional);
                    view = View3D.CreateIsometric(_document, type.Id);
                    view.Name = _clashViewName;
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

        public View GetNavisworksView(Document doc) {
            return new FilteredElementCollector(_document)
                .OfClass(typeof(View))
                .Cast<View>()
                .FirstOrDefault(item => item.Name == "Navisworks");
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
                .Where(item => item.GetLinkDocument() != null)
                .ToList();
        }

        public IEnumerable<ClashModel> GetClashes(ClashDetector detector) {
            return detector.FindClashes(_document);
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

        public void CreateFilter(IEnumerable<ElementId> categories, ElementFilter filter, string name) {
            using(Transaction t = _document.StartTransaction("Создание фильтра")) {
                ParameterFilterElement pfe = ParameterFilterElement.Create(_document, name, categories.ToList());
                pfe.SetElementFilter(filter);
                t.Commit();
            }

        }

        public async Task SelectAndShowElement(ElementId id) {
            if(_document.ActiveView != _view) {
                _uiDocument.ActiveView = _view;
            }
            _revitEventHandler.TransactAction = () => {
                SetViewOrientation(_document.GetElement(id).get_BoundingBox(_view));
                _uiDocument.Selection.SetElementIds(new[] { id });
            };

            await _revitEventHandler.Raise();
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

        private void SetViewOrientation(BoundingBoxXYZ bb) {
            using(Transaction t = _document.StartTransaction("Подрезка")) {
                bb.Max = bb.Max + new XYZ(1, 1, 1);
                bb.Min = bb.Min - new XYZ(1, 1, 1);
                _view.SetSectionBox(bb);
                var uiView = _uiDocument.GetOpenUIViews().FirstOrDefault(item => item.ViewId == _view.Id);
                if(uiView != null) {
                    uiView.ZoomAndCenterRectangle(bb.Min - new XYZ(1, 1, 1), bb.Max + new XYZ(1, 1, 1));
                }
                t.Commit();
            }
        }
    }
}