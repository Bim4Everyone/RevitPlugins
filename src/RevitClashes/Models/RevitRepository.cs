using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.Revit;
using dosymep.SimpleServices;

using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Extensions;
using RevitClashDetective.Models.FilterableValueProviders;
using RevitClashDetective.Models.GraphicView;
using RevitClashDetective.Models.Handlers;

using ParameterValueProvider = RevitClashDetective.Models.FilterableValueProviders.ParameterValueProvider;

namespace RevitClashDetective.Models {
    internal class RevitRepository {
        private readonly Application _application;
        private readonly UIApplication _uiApplication;

        private readonly Document _document;
        private readonly UIDocument _uiDocument;
        private readonly RevitEventHandler _revitEventHandler;
        private static readonly HashSet<string> _endings = new HashSet<string> { "_отсоединено", "_detached" };
        private const string _clashViewName = "BIM_Проверка на коллизии";
        private readonly View3D _view;
        private readonly ParameterFilterProvider _parameterFilterProvider;
        public const string FiltersNamePrefix = "BIM_коллизии_";

        public RevitRepository(Application application, Document document) {
            _application = application ?? throw new ArgumentNullException(nameof(application));
            _uiApplication = new UIApplication(application);

            _document = document ?? throw new ArgumentNullException(nameof(document));
            _uiDocument = new UIDocument(document);

            _revitEventHandler = new RevitEventHandler();
            _endings.Add("_" + _application.Username);

            _view = GetClashView();
            _parameterFilterProvider = new ParameterFilterProvider();

            CommonConfig = RevitClashDetectiveConfig.GetRevitClashDetectiveConfig();

            InitializeDocInfos();
        }

        public RevitRepository(
            UIApplication uiApplication,
            RevitEventHandler revitEventHandler,
            ParameterFilterProvider parameterFilterProvider) {

            _uiApplication = uiApplication
                ?? throw new ArgumentNullException(nameof(uiApplication));
            _revitEventHandler = revitEventHandler
                ?? throw new ArgumentNullException(nameof(revitEventHandler));
            _parameterFilterProvider = parameterFilterProvider
                ?? throw new ArgumentNullException(nameof(parameterFilterProvider));
            _application = _uiApplication.Application;
            _uiDocument = _uiApplication.ActiveUIDocument;
            _document = _uiDocument.Document;
            _endings.Add("_" + _application.Username);
            _view = GetClashView();
            CommonConfig = RevitClashDetectiveConfig.GetRevitClashDetectiveConfig();
            InitializeDocInfos();
        }

        public static string ProfilePath {
            get {
                var path = @"W:\Проектный институт\Отд.стандарт.BIM и RD\BIM-Ресурсы\5-Надстройки\Bim4Everyone\A101";
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

        public RevitClashDetectiveConfig CommonConfig { get; set; }

        public Document Doc => _document;

        public UIApplication UiApplication => _uiApplication;

        public List<DocInfo> DocInfos { get; set; }

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
                    type.DefaultTemplateId = ElementId.InvalidElementId;
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
                    view.DisplayStyle = DisplayStyle.FlatColors;

                    t.Commit();
                }
            }
            return view;
        }

        public string GetDocumentName() {
            return GetDocumentName(_document);
        }

        public string GetFileDialogPath() {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            if(!string.IsNullOrEmpty(CommonConfig.LastRunPath) && Directory.Exists(CommonConfig.LastRunPath)) {
                path = CommonConfig.LastRunPath;
            }
            return path;
        }

        public static string GetDocumentName(Document doc) {
            var title = doc.Title;
            return GetDocumentName(title);
        }

        public static string GetDocumentName(string fileName) {
            foreach(var ending in _endings) {
                if(fileName.IndexOf(ending) > -1) {
                    fileName = fileName.Substring(0, fileName.IndexOf(ending));
                }
            }
            return fileName;
        }

        public string GetObjectName() {
            return _document.Title.Split('_').FirstOrDefault();
        }

        public Element GetElement(string fileName, ElementId id) {
            Document doc;
            if(fileName == null) {
                doc = _document;
            } else {
                doc = DocInfos.FirstOrDefault(item => item.Name.Equals(GetDocumentName(fileName), StringComparison.CurrentCultureIgnoreCase))?.Doc;
            }
            var elementId = id;
            if(doc == null || elementId.IsNull()) {
                return null;
            }

            return doc.GetElement(elementId);
        }

        public List<Collector> GetCollectors() {
            return DocInfos
                .Select(item => new Collector(item.Doc))
                .ToList();
        }

        public List<RevitLinkInstance> GetRevitLinkInstances() {
            return new FilteredElementCollector(_document)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>()
                .Where(item => item.GetLinkDocument() != null)
                .ToList();
        }

        public bool IsParentLink(RevitLinkInstance link) {
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

        public void InitializeDocInfos() {
            DocInfos = GetRevitLinkInstances()
               .Select(item => new DocInfo(GetDocumentName(item.GetLinkDocument()), item.GetLinkDocument(), item.GetTransform()))
               .ToList();
            DocInfos.Add(new DocInfo(GetDocumentName(), _document, Transform.Identity));
        }

        public List<Category> GetCategories() {
            return _parameterFilterProvider.GetAllModelCategories(Doc, _view)
                .Select(c => Category.GetCategory(Doc, c))
                .ToList();
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

        public IEnumerable<Element> GetFilteredElements(Document doc, IEnumerable<ElementId> categories, ElementFilter filter) {
            return new FilteredElementCollector(doc)
                .WherePasses(new ElementMulticategoryFilter(categories.ToList()))
                .WhereElementIsNotElementType()
                .WherePasses(filter);
        }

        public void ShowErrorMessage(string message) {
            var dialog = GetPlatformService<IMessageBoxService>();
            dialog.Show(
                message,
                $"BIM",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error,
                System.Windows.MessageBoxResult.OK);
        }

        /// <summary>
        /// Выбирает элементы на 3D виде и делает подрезку
        /// </summary>
        /// <param name="elements">Элементы для выбора</param>
        /// <param name="view">3D вид, на котором надо выбрать и показать элементы</param>
        public void SelectAndShowElement(IEnumerable<ElementModel> elements, View3D view = null) {
            SelectAndShowElement(elements, 10, view);
        }

        public void SelectAndShowElement(IEnumerable<ElementModel> elements, double additionalSize, View3D view = null) {
            if(elements is null) { throw new ArgumentNullException(nameof(elements)); }

            var bbox = GetCommonBoundingBox(elements);
            SelectAndShowElement(elements.Select(item => item.GetElement(DocInfos)), bbox, additionalSize, view);
        }

        public void SelectAndShowElement(ClashModel clashModel, bool isolateClashElements) {
            if(clashModel is null || clashModel.OtherElement is null || clashModel.MainElement is null) {
                throw new ArgumentNullException(nameof(clashModel));
            }

            try {
                var view = _view;
                _uiDocument.ActiveView = view;
                var bbox = GetCommonBoundingBox(clashModel);

                if(bbox != null) {
                    _revitEventHandler.TransactAction = () => {
                        SetSectionBox(bbox, view, 10);
                        try {
                            if(isolateClashElements) {
                                HighlightClashElements(view, clashModel);
                            } else {
                                ClearViewFilters(view);
                            }
                        } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                            ShowErrorMessage("Не удалось выделить элементы коллизии");
                        }
                    };
                    _revitEventHandler.Raise();
                }
                _uiDocument.Selection.SetElementIds(GetElementsToSelect(clashModel, view));
            } catch(AccessViolationException) {
                ShowErrorMessage("Окно плагина было открыто в другом документе Revit, который был закрыт, нельзя показать элемент.");
            } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                ShowErrorMessage("Окно плагина было открыто в другом документе Revit, который сейчас не активен, нельзя показать элемент.");
            }
        }

        /// <summary>
        /// Скрывает элементы на 3D виде коллизий, которые попадают в заданный фильтр.
        /// Использовать для изоляции элементов инвертированного фильтра на виде.
        /// </summary>
        /// <param name="filterToHide">Фильтр, элементы в котором будут скрыты</param>
        /// <param name="categoriesToShow">Категории, которые должны остаться видимыми</param>
        /// <exception cref="ArgumentNullException">Исключение, если один из обязательных параметров null</exception>
        /// <exception cref="InvalidOperationException">Исключение, если не удалось применить настройки видимости</exception>
        public void ShowElements(
            ElementFilter filterToHide,
            ICollection<BuiltInCategory> categoriesToShow) {

            if(filterToHide is null) { throw new ArgumentNullException(nameof(filterToHide)); }
            if(categoriesToShow is null) { throw new ArgumentNullException(nameof(categoriesToShow)); }
            string error = string.Empty;

            try {
                var view = GetClashView();
                _uiApplication.ActiveUIDocument.ActiveView = view;

                _revitEventHandler.TransactAction = () => {
                    try {
                        HighlightFilter(view, filterToHide, categoriesToShow);
                    } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                        // если здесь выбросить исключение, оно заглушится в RevitEventHandler.Execute
                        error = "Не удалось изолировать поисковый набор";
                    }
                };
                _revitEventHandler.Raise();
                if(!string.IsNullOrWhiteSpace(error)) {
                    throw new InvalidOperationException(error);
                }
            } catch(AccessViolationException) {
                error = "Окно плагина было открыто в другом документе Revit, который был закрыт, " +
                    "нельзя показать элемент.";
                throw new InvalidOperationException(error);
            } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                error = "Окно плагина было открыто в другом документе Revit, который сейчас не активен, " +
                    "нельзя показать элемент.";
                throw new InvalidOperationException(error);
            }
        }

        /// <summary>
        /// Конвертирует кубические футы в кубические метры
        /// </summary>
        /// <param name="cubeFeet">Кубические футы</param>
        /// <returns>Кубические метры</returns>
        public double ConvertToM3(double cubeFeet) {
#if REVIT_2020_OR_LESS
            return UnitUtils.ConvertFromInternalUnits(cubeFeet, DisplayUnitType.DUT_CUBIC_METERS);
#else
            return UnitUtils.ConvertFromInternalUnits(cubeFeet, UnitTypeId.CubicMeters);
#endif
        }


        /// <summary>
        /// Возвращает коллекцию фильтров по параметрам элементов, в которые попадают элементы, попадающие в фильтр по элементам,
        /// и элементы всех категорий кроме заданных
        /// </summary>
        /// <param name="view">Вид, на котором должны сформироваться фильтры</param>
        /// <param name="filterToHide">Фильтр по параметрам элементов, который надо скрыть</param>
        /// <param name="categoriesToShow">Заданные категории, которые должны оставаться видимыми</param>
        /// <returns></returns>
        private ICollection<ParameterFilterElement> GetHighlightFilters(
            View3D view,
            ElementFilter filterToHide,
            ICollection<BuiltInCategory> categoriesToShow) {

            string username = Doc.Application.Username;
            List<ParameterFilterElement> parameterFilters = new List<ParameterFilterElement>();
            try {
                parameterFilters.Add(_parameterFilterProvider.CreateParameterFilter(
                    Doc,
                    $"{FiltersNamePrefix}поисковый_набор_инвертированный_{username}",
                    filterToHide,
                    categoriesToShow));
            } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                //pass
            }
            parameterFilters.Add(_parameterFilterProvider.GetExceptCategoriesFilter(
                Doc,
                view,
                categoriesToShow,
                $"{FiltersNamePrefix}категории_не_поискового_набора_{username}"));
            return parameterFilters;
        }

        private void HighlightFilter(
            View3D view,
            ElementFilter filterToHide,
            ICollection<BuiltInCategory> categoriesToShow) {

            using(Transaction t = Doc.StartTransaction("Выделение элементов коллизии")) {
                view.IsSectionBoxActive = false;
                var uiView = _uiDocument.GetOpenUIViews().FirstOrDefault(item => item.ViewId == view.Id);
                if(uiView != null) {
                    uiView.ZoomToFit();
                }
                var parameterFiltersToHide = GetHighlightFilters(view, filterToHide, categoriesToShow);
                view = RemoveFilters(view);
                foreach(var parameterFilter in parameterFiltersToHide) {
                    view.AddFilter(parameterFilter.Id);
                    view.SetFilterVisibility(parameterFilter.Id, false);
                }
                foreach(var category in GetLineCategoriesToHide()) {
                    view.SetCategoryHidden(category, true);
                }
                t.Commit();
            }
        }


        private ICollection<ElementId> GetLineCategoriesToHide() {
            return new ElementId[] {
                new ElementId(BuiltInCategory.OST_MEPSpaceSeparationLines),
                new ElementId(BuiltInCategory.OST_RoomSeparationLines),
                new ElementId(BuiltInCategory.OST_Lines)
            };
        }

        private void SelectAndShowElement(
            IEnumerable<Element> elements,
            BoundingBoxXYZ elementsBBox,
            double additionalSize,
            View3D view = null) {

            if(elements is null) { throw new ArgumentNullException(nameof(elements)); }
            try {
                view = view ?? _view;
                _uiDocument.ActiveView = view;

                if(elementsBBox != null) {
                    _revitEventHandler.TransactAction = () => {
                        SetSectionBox(elementsBBox, view, additionalSize);
                    };
                    _revitEventHandler.Raise();
                }
                _uiDocument.Selection.SetElementIds(GetElementsToSelect(elements, view));
            } catch(AccessViolationException) {
                ShowErrorMessage("Окно плагина было открыто в другом документе Revit, который был закрыт, нельзя показать элемент.");
            } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {
                ShowErrorMessage("Окно плагина было открыто в другом документе Revit, который сейчас не активен, нельзя показать элемент.");
            }
        }

        public void DoAction(Action action) {
            _revitEventHandler.TransactAction = action;
            _revitEventHandler.Raise();
        }

        public static string GetLevelName(Element element) {
            return GetLevel(element)?.Name;
        }

        public static Level GetLevel(Element element) {
            ElementId levelId;
            foreach(var paramName in BaseLevelParameters) {
                levelId = element.GetParamValueOrDefault<ElementId>(paramName);
                if(levelId.IsNotNull()) {
                    return element.Document.GetElement(levelId) as Level;
                }
            }
            if(element.LevelId.IsNotNull()) {
                return element.Document.GetElement(element.LevelId) as Level;
            }
            return null;
        }

        public T RemoveFilters<T>(T view) where T : View {
            var existedFilterIds = view.GetFilters();
            foreach(var filter in existedFilterIds) {
                view.RemoveFilter(filter);
            }
            return view;
        }


        private ICollection<ElementId> GetElementsToSelect(ClashModel clashModel, View3D view = null) {
            return GetElementsToSelect(
                new Element[] {
                    clashModel.MainElement.GetElement(DocInfos),
                    clashModel.OtherElement.GetElement(DocInfos) },
                view);
        }

        private ICollection<ElementId> GetElementsToSelect(IEnumerable<Element> elements, View3D view = null) {
            if(elements is null) { throw new ArgumentNullException(nameof(elements)); }

            List<ElementId> elementsFromThisDoc = elements
                .Where(item => item.IsFromDocument(_document))
                .Select(item => item.Id)
                .ToList();
            if(elementsFromThisDoc.Count > 0) {
                return elementsFromThisDoc;
            } else {
                var view3d = view ?? _view;
                return view3d.GetDependentElements(new ElementCategoryFilter(BuiltInCategory.OST_SectionBox));
            }
        }

        private BoundingBoxXYZ GetCommonBoundingBox(ClashModel clashModel) {
            return GetCommonBoundingBox(new ElementModel[] { clashModel.MainElement, clashModel.OtherElement });
        }

        private BoundingBoxXYZ GetCommonBoundingBox(IEnumerable<ElementModel> elements) {
            return elements
                .Select(item => new {
                    Bb = item.GetElement(DocInfos).get_BoundingBox(null),
                    Transform = item.GetDocInfo(DocInfos).Transform
                })
                .Where(item => item.Bb != null)
                .Select(item => item.Bb.GetTransformedBoundingBox(item.Transform))
                .GetCommonBoundingBox();
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

        /// <summary>
        /// Устанавливает область подрезки, увеличивая размеры ограничивающего бокса на заданное количество футов
        /// <para>Размеры области подрезки будут формироваться как сумма габарита бокса по соответствующей оси и добавочного размера</para>
        /// </summary>
        /// <param name="bb">Ограничивающий бокс, на основе которого будет строиться область подрезки</param>
        /// <param name="view">3D Вид для задания области подрезки</param>
        /// <param name="additionalSize">Добавочный размер в футах, на который нужно увеличить бокс в каждом направлении: OX, OY, OZ</param>
        private void SetSectionBox(BoundingBoxXYZ bb, View3D view, double additionalSize) {
            if(bb == null)
                return;
            using(Transaction t = _document.StartTransaction("Подрезка")) {
                double halfSize = Math.Abs(additionalSize / 2);
                bb.Max += new XYZ(halfSize, halfSize, halfSize);
                bb.Min -= new XYZ(halfSize, halfSize, halfSize);
                view.SetSectionBox(bb);
                var uiView = _uiDocument.GetOpenUIViews().FirstOrDefault(item => item.ViewId == view.Id);
                if(uiView != null) {
                    uiView.ZoomAndCenterRectangle(bb.Min, bb.Max);
                }
                t.Commit();
            }
        }

        private void HighlightClashElements(View3D view, ClashModel clash) {
            using(Transaction t = _document.StartTransaction("Выделение элементов коллизии")) {
                var filtersToHide = GetHighlightFilters(clash, view);
                view = RemoveFilters(view);
                foreach(var filter in filtersToHide) {
                    view.AddFilter(filter.Id);
                    view.SetFilterVisibility(filter.Id, false);
                }
                foreach(var category in GetLineCategoriesToHide()) {
                    view.SetCategoryHidden(category, true);
                }
                t.Commit();
            }
        }

        private ICollection<ParameterFilterElement> GetHighlightFilters(ClashModel clash, View view) {
            string username = _document.Application.Username;
            var filters = new List<ParameterFilterElement>() {
                _parameterFilterProvider.GetExceptCategoriesFilter(
                    _document,
                    view,
                    GetClashCategories(clash),
                    $"{FiltersNamePrefix}не_категории_элементов_коллизии_{username}")
            };
            var firstEl = clash.MainElement.GetElement(DocInfos);
            var secondEl = clash.OtherElement.GetElement(DocInfos);
            if(firstEl.Category.GetBuiltInCategory() == secondEl.Category.GetBuiltInCategory()) {
                filters.Add(
                    _parameterFilterProvider.GetHighlightFilter(
                        _document,
                        firstEl,
                        secondEl,
                        $"{FiltersNamePrefix}не_элементы_категории_коллизии_{username}"));
            } else {
                filters.Add(
                    _parameterFilterProvider.GetHighlightFilter(
                        _document,
                        firstEl,
                        $"{FiltersNamePrefix}не_первый_элемент_{username}"));
                filters.Add(
                    _parameterFilterProvider.GetHighlightFilter(
                        _document,
                        secondEl,
                        $"{FiltersNamePrefix}не_второй_элемент_{username}"));
            }
            return filters;
        }

        private ICollection<BuiltInCategory> GetClashCategories(ClashModel clash) {
            return new HashSet<BuiltInCategory>(
                new BuiltInCategory[] {
                    clash.MainElement.GetElement(DocInfos).Category.GetBuiltInCategory(),
                    clash.OtherElement.GetElement(DocInfos).Category.GetBuiltInCategory()
                });
        }

        private void ClearViewFilters(View3D view) {
            using(Transaction t = _document.StartTransaction("Сброс фильтров элементов коллизии")) {
                RemoveFilters(view);
                t.Commit();
            }
        }

        protected T GetPlatformService<T>() {
            return ServicesProvider.GetPlatformService<T>();
        }
    }
}
