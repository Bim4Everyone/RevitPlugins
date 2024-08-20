using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Revit;

using RevitLintelPlacement.Handlers;
using RevitLintelPlacement.ViewModels;

namespace RevitLintelPlacement.Models {

    internal class RevitRepository {
        private readonly string _view3DName = "3D_Перемычки";
        private static readonly string _settingsPath = @"W:\Проектный институт\Отд.стандарт.BIM и RD\BIM-Ресурсы\5-Надстройки\Bim4Everyone\A101\";
        
        private readonly BuiltInParameter[] _bottomBarHeightsParams = new[] {
            BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM,
            BuiltInParameter.INSTANCE_ELEVATION_PARAM,
            BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM
        };

        private readonly Application _application;
        private readonly UIApplication _uiApplication;

        private readonly Document _document;
        private readonly UIDocument _uiDocument;
        private readonly RevitEventHandler _revitEventHandler;

        public RevitRepository(Application application, Document document, LintelsConfig lintelsConfig) {
            _application = application;
            _uiApplication = new UIApplication(application);

            _document = document;
            _uiDocument = new UIDocument(document);

            _revitEventHandler = new RevitEventHandler();

            LintelsConfig = lintelsConfig;
            LintelsCommonConfig = LintelsCommonConfig.GetLintelsCommonConfig(GetDocumentName());

            GetView3D();
        }

        public LintelsConfig LintelsConfig { get; set; }
        public LintelsCommonConfig LintelsCommonConfig { get; set; }

        public string View3dName => string.Concat(_view3DName, "_", _application.Username);

        public static string ProfilePath {
            get {
                var path = _settingsPath;
                if(!Directory.Exists(path)) {
                    path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "dosymep");
                }
                return path;
            }
        }

        public static string LocalRulePath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                                            "dosymep",
                                                            ModuleEnvironment.RevitVersion,
                                                            nameof(RevitLintelPlacement),
                                                            "Rules");

        public string GetTemplatePath() => Path.Combine(_settingsPath, ModuleEnvironment.RevitVersion, nameof(RevitLintelPlacement), "Rules", "АТР.json");


        public string GetProjectPath() => Path.Combine(_settingsPath, ModuleEnvironment.RevitVersion, nameof(RevitLintelPlacement), "Rules", GetDocumentName() + ".json");


        public static bool HasEmptyProjectPath() => Directory.Exists(_settingsPath);

        public string GetDocumentName() {
            var documentName = string.IsNullOrEmpty(_document.Title)
                ? "Без имени"
                : _document.Title.Split('_').FirstOrDefault();

            return documentName;
        }
        
        public BasePoint GetBasePoint() {
            return (BasePoint) new FilteredElementCollector(_document)
                .OfClass(typeof(BasePoint))
                .FirstElement();
        }

        public FamilySymbol GetLintelType(string lintelTypeName) {
            return new FilteredElementCollector(_document)
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .WhereElementIsElementType()
                .FirstOrDefault(e => e.Name == lintelTypeName) as FamilySymbol;
        }

        public IEnumerable<FamilySymbol> GetLintelTypes() {
            return new FilteredElementCollector(_document)
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .WhereElementIsElementType()
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .Where(e => LintelsCommonConfig.LintelFamily.Equals(e.Family?.Name, StringComparison.CurrentCultureIgnoreCase));
        }

        public Element GetElementById(ElementId id) {
            return _document.GetElement(id);
        }

        public Category GetCategory(BuiltInCategory builtInCategory) {
            return Category.GetCategory(_document, builtInCategory);
        }

        public View3D GetView3D() {
            var view = new FilteredElementCollector(_document)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .FirstOrDefault(item => item.Name.Equals(View3dName, StringComparison.OrdinalIgnoreCase));

            if(view == null) {
                using(Transaction transaction = _document.StartTransaction("Создание 3D-вида")) {
                    view = CreateView3d();

                    UpdatePhase(view);
                    HideCategories(view);
                    UpdateViewGroup(view);

                    transaction.Commit();
                }
            }

            return view;
        }

        private void UpdatePhase(View3D view) {
            View activeView = _uiDocument.ActiveGraphicalView;
            view.SetParamValue(BuiltInParameter.VIEW_PHASE,
                activeView.GetParamValue<ElementId>(BuiltInParameter.VIEW_PHASE));
            view.SetParamValue(BuiltInParameter.VIEW_PHASE_FILTER,
                activeView.GetParamValue<ElementId>(BuiltInParameter.VIEW_PHASE_FILTER));
        }

        private View3D CreateView3d() {
            ViewFamilyType type = new FilteredElementCollector(_document)
                .OfClass(typeof(ViewFamilyType))
                .Cast<ViewFamilyType>()
                .First(v => v.ViewFamily == ViewFamily.ThreeDimensional);

            type.DefaultTemplateId = ElementId.InvalidElementId;
            View3D view = View3D.CreateIsometric(_document, type.Id);
            view.Name = View3dName;
            return view;
        }

        private void UpdateViewGroup(View3D view) {
            string bimGroup = new FilteredElementCollector(_document)
                .OfClass(typeof(View))
                .Cast<View>()
                .Where(item => item.Category != null)
                .Where(item => !item.InAnyCategory(
                    BuiltInCategory.OST_Schedules, BuiltInCategory.OST_Sheets))
                .Select(item => item.GetParamValueOrDefault<string>(ProjectParamsConfig.Instance.ViewGroup))
                .FirstOrDefault(item => !string.IsNullOrEmpty(item) && item.Contains("BIM"));

            if(bimGroup != null) {
                view.SetParamValue(ProjectParamsConfig.Instance.ViewGroup, bimGroup);
            }
        }

        private static void HideCategories(View3D view) {
            var categories = new[] {
                new ElementId(BuiltInCategory.OST_Levels), new ElementId(BuiltInCategory.OST_WallRefPlanes),
                new ElementId(BuiltInCategory.OST_Grids), new ElementId(BuiltInCategory.OST_VolumeOfInterest)
            };

            foreach(ElementId category in categories) {
                if(view.CanCategoryBeHidden(category)) {
                    view.SetCategoryHidden(category, true);
                }
            }
        }

        public IEnumerable<WallType> GetWallTypes() {
            return new FilteredElementCollector(_document)
                .OfClass(typeof(WallType))
                .Cast<WallType>()
                .Where(w => w.Kind == WallKind.Basic);
        }

        public IEnumerable<Element> GetGenericModelFamilies() {
            var categoryId = Category.GetCategory(_document, BuiltInCategory.OST_GenericModel).Id;
            return new FilteredElementCollector(_document)
                .OfClass(typeof(Family))
                .Cast<Family>()
                .Where(f => GetFamilyCategoryId(f) == categoryId)
                .ToList();
        }

        public IEnumerable<RevitLinkType> GetLinkTypes() {
            return new FilteredElementCollector(_document)
                .OfClass(typeof(RevitLinkType))
                .Cast<RevitLinkType>()
                .Where(item => item.GetLinkedFileStatus() == LinkedFileStatus.Loaded);
        }

        public FamilyInstance PlaceLintel(FamilySymbol lintelType, FamilyInstance elementInWall) {
            if(!lintelType.IsActive)
                lintelType.Activate();
            XYZ center = GetLocationPoint(elementInWall);
            FamilyInstance lintel = null;
            if(elementInWall.LevelId != null && elementInWall.LevelId != ElementId.InvalidElementId) {
                lintel = _document.Create.NewFamilyInstance(center, lintelType, (Level) _document.GetElement(elementInWall.LevelId), StructuralType.NonStructural);
            } else {
                lintel = _document.Create.NewFamilyInstance(center, lintelType, StructuralType.NonStructural);
            }


            RotateLintel(lintel, elementInWall, center);
            return lintel;
        }

        public void DeleteLintel(FamilyInstance lintel) {
            _document.Delete(lintel.Id);
        }

        public IEnumerable<Element> GetElements(IEnumerable<ElementId> ids) {
            foreach(var id in ids)
                yield return _document.GetElement(id);
        }

        public List<FamilyInstance> GetLintels(FilteredElementCollector collector) {
            return collector
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(e => LintelsCommonConfig.LintelFamily.Equals(e.Symbol?.Family?.Name, StringComparison.CurrentCultureIgnoreCase))
                .ToList();
        }

        public FilteredElementCollector GetAllElementsCollector() {
            return new FilteredElementCollector(_document);
        }

        public FilteredElementCollector GetViewElementCollector(View view) {
            return new FilteredElementCollector(_document, view.Id);
        }

        public View GetCurrentView() {
            return _document.ActiveView;
        }

        public FilteredElementCollector GetSelectedElementsCollector() {
            var ids = _uiDocument.Selection.GetElementIds();
            if(ids.Count == 0) {
                throw new ArgumentException("Нет выбранных элементов");
            }
            return new FilteredElementCollector(_document, ids);
        }

        public List<FamilyInstance> GetElementsInWall(FilteredElementCollector collector1, FilteredElementCollector collector2, ElementInfosViewModel elementInfos) {
            var categoryFilter = new ElementMulticategoryFilter(
                new List<BuiltInCategory> { BuiltInCategory.OST_Doors, BuiltInCategory.OST_Windows });

            var openings = collector1.OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(f => f?.Symbol != null && f.Symbol?.Family != null && f.Symbol.Family.Name != null &&
                f.Symbol.Family.Name.ToLower().Contains(LintelsCommonConfig.HolesFilter.ToLower())).ToList();

            var wallsAndDoors = collector2
                .WherePasses(categoryFilter)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(e => e.Host is Wall && e.Location != null).ToList();

            return openings.Union(wallsAndDoors, new FamilyInstanceComparer()).Where(e => CheckElementInWallParameter(e, elementInfos)).ToList();
        }

        public View GetElevation() {
            return new FilteredElementCollector(_document)
                .OfClass(typeof(View))
                .Cast<View>()
                .FirstOrDefault(v => v.ViewType == ViewType.Elevation);
        }

        public View GetPlan() {
            return new FilteredElementCollector(_document)
               .OfClass(typeof(View))
               .Cast<View>()
               .FirstOrDefault(v => v.ViewType == ViewType.FloorPlan);
        }

        //проверка, есть ли сверху элемента стена, у которой тип железобетон (в таком случает перемычку ставить не надо)
        public bool CheckUp(View3D view3D, FamilyInstance elementInWall, IEnumerable<string> linkNames) {
            XYZ viewPoint = GetLocationPoint(elementInWall);
            viewPoint = new XYZ(viewPoint.X, viewPoint.Y, viewPoint.Z + GetElevation(elementInWall));
            ReferenceWithContext refWithContext =
                GetNearestWallOrColumn(view3D, elementInWall, new XYZ(viewPoint.X, viewPoint.Y, viewPoint.Z - 0.32), new XYZ(0, 0, 1), false); //чтобы точка точно была под гранью стены
            if(refWithContext == null)
                return false;
            if(refWithContext.Proximity > 0.52) { //10 см
                return false;
            }
            var wallOrColumn = _document.GetElement(refWithContext.GetReference().ElementId);
            if(ContainsReinforce(wallOrColumn.Name)) {
                return false;
            }
            refWithContext = GetNearestWallOrColumn(view3D, elementInWall, new XYZ(viewPoint.X, viewPoint.Y, viewPoint.Z - 0.32), new XYZ(0, 0, 1), true);
            if(refWithContext == null)
                return true;
            if(refWithContext.Proximity > 0.52) {
                return true;
            }
            wallOrColumn = _document.GetElement(refWithContext.GetReference().ElementId);
            if(wallOrColumn is Wall wall)
                return !ContainsReinforce(wall.Name);
            if(wallOrColumn.Category.Id == new ElementId(BuiltInCategory.OST_StructuralColumns) || wallOrColumn.Category.Id == new ElementId(BuiltInCategory.OST_StructuralFraming))
                return false;
            if(wallOrColumn is RevitLinkInstance linkedInstance) {
                return !linkNames.Any(l => l.Equals(linkedInstance.GetLinkDocument().Title, StringComparison.CurrentCultureIgnoreCase));
            }
            return true;
        }

        private bool ContainsReinforce(string name) {
            return LintelsCommonConfig.ReinforcedConcreteFilter
                .Where(item => !string.IsNullOrEmpty(item))
                .Any(item => name.IndexOf(item, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        public bool DoesRightCornerNeeded(View3D view3D, FamilyInstance elementInWall, IEnumerable<string> linkNames,
            ElementInfosViewModel elementInfos, double offset, out double realOffset) {
            return DoesCornerNeeded(view3D, elementInWall, new XYZ(-1, 0, 0), linkNames, elementInfos, offset, out realOffset);
        }

        public bool DoesLeftCornerNeeded(View3D view3D, FamilyInstance elementInWall, IEnumerable<string> linkNames,
            ElementInfosViewModel elementInfos, double offset, out double realOffset) {
            return DoesCornerNeeded(view3D, elementInWall, new XYZ(1, 0, 0), linkNames, elementInfos, offset, out realOffset);
        }

        public void ActivateView() {
            _uiDocument.ActiveView = GetView3D();
        }

        private bool DoesCornerNeeded(View3D view3D, FamilyInstance elementInWall, XYZ direction, IEnumerable<string> linkNames, ElementInfosViewModel elementInfos, double offset, out double realOffset) {
            realOffset = 0;
            //получение предполагаемой точки вставки перемычки,
            //из которой проводится поиск жб-элементов
            XYZ viewPoint = GetLocationPoint(elementInWall);
            viewPoint = new XYZ(viewPoint.X, viewPoint.Y, viewPoint.Z + GetElevation(elementInWall));

            //направление, в котором будет проводиться поиск
            direction = elementInWall.GetTransform().OfVector(direction);

            //получение ссылки на ближайшую жб-стену, колонну или связанный файл и расстояние до них
            ReferenceWithContext refWithContext = GetNearestWallOrColumn(view3D, elementInWall, viewPoint, direction, true);

            //Если жб-стены или колонны не найдены, то уголок не нужен
            if(refWithContext == null)
                return false;
            
            var elementWidth = elementInWall.GetParamValueOrDefault(LintelsCommonConfig.OpeningWidth)
                               ?? elementInWall.Symbol.GetParamValueOrDefault(LintelsCommonConfig.OpeningWidth);

            if(elementWidth == null) {
                elementInfos.ElementInfos.Add(new ElementInfoViewModel(elementInWall.Id,
                    InfoElement.MissingOpeningParameter.FormatMessage(LintelsCommonConfig.OpeningWidth)) {
                    Name = elementInWall.Name,
                    LevelName = elementInWall.LevelId != null ? GetElementById(elementInWall.LevelId)?.Name : null
                });
                return false;
            }
            
            //получение ближайшего элемента и его проверка
            var wallOrColumn = _document.GetElement(refWithContext.GetReference().ElementId);

            if((wallOrColumn is Wall wall && ContainsReinforce(wall.Name))
            || (wallOrColumn.Category.Id == new ElementId(BuiltInCategory.OST_StructuralColumns) || wallOrColumn.Category.Id == new ElementId(BuiltInCategory.OST_StructuralFraming))
            || (wallOrColumn is RevitLinkInstance linkedInstance && linkNames.Any(l => l.Equals(linkedInstance.GetLinkDocument().Title, StringComparison.CurrentCultureIgnoreCase)))) {
                //получение смещения от края проема
                realOffset = refWithContext.Proximity - (double) elementWidth / 2;
                return refWithContext.Proximity <= ((double) elementWidth / 2 + offset);
            }

            return false;
        }

        public Transaction StartTransaction(string transactionName) {
            var transaction = new Transaction(_document);
            transaction.BIMStart(transactionName);
            return transaction;
        }

        public ViewOrientation3D GetOrientation3D() {
            var view3D = new FilteredElementCollector(_document)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .First(v => !v.IsTemplate && v.Name.Equals(View3dName, StringComparison.CurrentCultureIgnoreCase));
            return view3D.GetOrientation();
        }

        public bool IsActiveView3D() {
            return _document.ActiveView is View3D;
        }

        public async Task SelectAndShowElement(ElementId id, ViewOrientation3D orientation) {
            for(int i = 0; i < 2; i++) {
                _revitEventHandler.TransactAction = () => {
                    _uiDocument.Selection.SetElementIds(new[] { id });
                    var commandId = RevitCommandId.LookupCommandId("ID_VIEW_APPLY_SELECTION_BOX");
                    if(!(commandId is null) && _uiDocument.Application.CanPostCommand(commandId)) {
                        _uiApplication.PostCommand(commandId);
                    }
                };
                await _revitEventHandler.Raise();
            }
        }

        public FamilyInstance GetDimensionFamilyInstance(FamilyInstance fi) {
            return fi.GetDependentElements(new ElementClassFilter(typeof(Dimension)))
                .Select(item => GetElementById(item))
                .OfType<Dimension>()
                .SelectMany(d => d.References.OfType<Reference>())
                .Select(r => GetElementById(r.ElementId))
                .OfType<FamilyInstance>()
                .FirstOrDefault(i => i.Host != null);
        }

        public async Task MirrorLintel(FamilyInstance lintel, FamilyInstance elementInWall) {
            if(lintel is null) {
                throw new ArgumentNullException(nameof(lintel));
            }

            if(elementInWall is null) {
                throw new ArgumentNullException(nameof(elementInWall));
            }
            _revitEventHandler.TransactAction = () => {
                using(Transaction t = _document.StartTransaction("Поворот перемычки")) {
                    var ids = lintel
                    .GetDependentElements(new ElementClassFilter(typeof(Dimension)));
                    _document.Delete(ids);
                    var center = ((LocationPoint) lintel.Location).Point;
                    var line = Line.CreateBound(center, new XYZ(center.X, center.Y, center.Z + 1));
                    ElementTransformUtils.RotateElement(_document, lintel.Id, line, Math.PI);
                    LockLintel(GetView3D(), GetElevation(), GetPlan(), lintel, elementInWall);
                    t.Commit();
                }
            };
            await _revitEventHandler.Raise();
        }

        private IEnumerable<string> GetMaterialClasses(Element element) {
            var materialIds = element.GetMaterialIds(false);
            foreach(var id in materialIds) {
                yield return ((Material) _document.GetElement(id)).MaterialClass;
            }
        }

        public void SetActiveView() {
            _uiDocument.ActiveView = GetView3D();
        }

        private ReferenceWithContext GetNearestWallOrColumn(View3D view3D, FamilyInstance fi, XYZ viewPoint, XYZ direction, bool excludeHost) {
            var exclusionList = new List<ElementId> { fi.Id };
            exclusionList.AddRange(fi.GetDependentElements(new ElementClassFilter(typeof(FamilyInstance))));
            if(fi.Host != null && excludeHost) {
                exclusionList.Add(fi.Host.Id);
            }
            var exclusionFilter = new ExclusionFilter(exclusionList);
            var classFilter = new ElementClassFilter(typeof(Wall));
            var classFilter2 = new ElementClassFilter(typeof(RevitLinkInstance));
            var categoryFilter = new ElementMulticategoryFilter(new[] { BuiltInCategory.OST_StructuralColumns, BuiltInCategory.OST_StructuralFraming });
            var logicalAndFilter = new LogicalAndFilter(new List<ElementFilter> { exclusionFilter, classFilter });
            var logicalOrFilter = new LogicalOrFilter(new List<ElementFilter> { logicalAndFilter, categoryFilter, classFilter2 });
            var refIntersector = new ReferenceIntersector(logicalOrFilter, FindReferenceTarget.All, view3D);

            return refIntersector.FindNearest(viewPoint, direction);
        }

        public void LockLintel(View3D view3D, View elevation, View plan, FamilyInstance lintel, FamilyInstance elementInWall) {
            var leftRightElement = elementInWall.GetReferences(FamilyInstanceReferenceType.CenterLeftRight);
            var leftRightLintel = lintel.GetReferences(FamilyInstanceReferenceType.CenterLeftRight);
            if(leftRightElement.Count > 0 && leftRightLintel.Count > 0)
                _document.Create.NewAlignment(view3D, leftRightLintel.First(), leftRightElement.First());

            var topElement = elementInWall.GetReferences(FamilyInstanceReferenceType.Top);
            var bottomLintel = lintel.GetReferences(FamilyInstanceReferenceType.CenterElevation);

            try {
                if(topElement.Count > 0 && bottomLintel.Count > 0) {
                    _document.Create.NewAlignment(elevation, topElement.First(), bottomLintel.First());
                }
            } catch { }


            var leftL = lintel.GetReferences(FamilyInstanceReferenceType.Front);
            var rightL = lintel.GetReferences(FamilyInstanceReferenceType.Back);
            var wallReferences1 = HostObjectUtils.GetSideFaces((Wall) elementInWall.Host, ShellLayerType.Interior);
            var wallReferences2 = HostObjectUtils.GetSideFaces((Wall) elementInWall.Host, ShellLayerType.Exterior);

            // возможно, ошибка возникает при установке параметра половина толщины,
            // поэтому нет геометричкого выравнивания
            try {
                if(leftL.Count > 0 && wallReferences1.Count > 0) {
                    _document.Create.NewAlignment(plan, leftL.First(), wallReferences1.First());
                }
                if(rightL.Count > 0 && wallReferences2.Count > 0) {
                    _document.Create.NewAlignment(plan, rightL.First(), wallReferences2.First());
                }
            } catch(Exception e) {
                Debug.WriteLine(e.Message);
                try {
                    if(leftL.Count > 0 && wallReferences2.Count > 0) {
                        _document.Create.NewAlignment(plan, leftL.First(), wallReferences2.First());
                    }
                    if(rightL.Count > 0 && wallReferences1.Count > 0) {
                        _document.Create.NewAlignment(plan, rightL.First(), wallReferences1.First());
                    }
                } catch(Exception ex) {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        public bool CheckElementInWallParameter(FamilyInstance elementInWall, ElementInfosViewModel elementInfos, IEnumerable<string> wallTypes = null) {
            if(!elementInWall.IsExistsParam(LintelsCommonConfig.OpeningHeight)
                && !elementInWall.Symbol.IsExistsParam(LintelsCommonConfig.OpeningHeight)
                && (wallTypes == null || wallTypes.Any(w => w.Equals(elementInWall.Host.Name, StringComparison.CurrentCultureIgnoreCase)))) {
                elementInfos.ElementInfos.Add(new ElementInfoViewModel(elementInWall.Id,
                    InfoElement.MissingOpeningParameter.FormatMessage(LintelsCommonConfig.OpeningHeight)) {
                    Name = elementInWall.Name,
                    LevelName = elementInWall.LevelId != null ? GetElementById(elementInWall.LevelId)?.Name : null
                });
                return false;
            }
            return true;
        }

        /// <summary>
        /// Возвращает высоту уровня с учетом базовой точки.
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Возвращает высоту уровня с учетом базовой точки.</returns>
        private double GetElevation(FamilyInstance element) {
            return ((Level) _document.GetElement(element.LevelId)).ProjectElevation;
        }

        public XYZ GetLocationPoint(FamilyInstance elementInWall) {
            if(elementInWall is null) {
                throw new ArgumentNullException(nameof(elementInWall));
            }

            var location = ((LocationPoint) elementInWall.Location).Point;
            var height = elementInWall.GetParamValueOrDefault<double?>(LintelsCommonConfig.OpeningHeight)
                         ?? elementInWall.Symbol.GetParamValueOrDefault<double?>(LintelsCommonConfig.OpeningHeight);

            return new XYZ(location.X, location.Y, height ?? 0);
        }

        public bool CheckLintelType(FamilySymbol lintelType, ElementInfosViewModel elementInfos) {
            if(lintelType.Family == null)
                return false;
            var familyDocument = _document.EditFamily(lintelType.Family);
            try {
                var familyManager = familyDocument.FamilyManager;
                var parameterNames = familyManager.GetParameters().Select(p => p.Definition.Name).ToList();
                bool result = true;
                var configParameterNames = new List<string>() {
                LintelsCommonConfig.LintelFixation,
                LintelsCommonConfig.LintelLeftCorner,
                LintelsCommonConfig.LintelLeftOffset,
                LintelsCommonConfig.LintelRightCorner,
                LintelsCommonConfig.LintelRightOffset,
                LintelsCommonConfig.LintelThickness,
                LintelsCommonConfig.LintelWidth
                };
                foreach(var configParameter in configParameterNames) {
                    if(!parameterNames.Any(p => p.Equals(configParameter, StringComparison.CurrentCultureIgnoreCase))) {
                        result = false;
                        elementInfos.ElementInfos.Add(new ElementInfoViewModel(lintelType.Id, InfoElement.MissingLintelParameter.FormatMessage(configParameter)) {
                            Name = lintelType.Name
                        });
                    }
                }
                return result;
            } finally {
                familyDocument.Close(false);
            }
        }

        public IEnumerable<ParameterViewModel> GetParametersFromFamilies(string familyName) {
            var family = new FilteredElementCollector(_document)
                .OfClass(typeof(Family))
                .Cast<Family>()
                .FirstOrDefault(f => !string.IsNullOrEmpty(familyName) && familyName.Equals(f.Name, StringComparison.CurrentCultureIgnoreCase));

            Document familyDocument = null;
            List<ParameterViewModel> parameters = new List<ParameterViewModel>();
            try {
                familyDocument = _document.EditFamily(family);
                var familyManager = familyDocument.FamilyManager;
                parameters = familyManager.GetParameters()
                    .Where(p => !p.IsReadOnly)
                    .Select(p => new ParameterViewModel {
                        Name = p.Definition.Name,
                        StorageType = p.StorageType
                    })
                    .ToList();
            } catch {
                // pass
            } finally {
                if(familyDocument != null) {
                    familyDocument.Close(false);
                }
            }
            foreach(var parameter in parameters) {
                yield return parameter;
            }
        }

        public bool CheckConfigParameters(ElementInfosViewModel elementInfos) {
            bool result = true;
            if(string.IsNullOrEmpty(LintelsCommonConfig.LintelFixation)) {
                result = false;
                AddBlankParameterInfo(elementInfos, nameof(LintelsCommonConfig.LintelFixation));
            }
            if(string.IsNullOrEmpty(LintelsCommonConfig.LintelLeftCorner)) {
                result = false;
                AddBlankParameterInfo(elementInfos, nameof(LintelsCommonConfig.LintelLeftCorner));
            }
            if(string.IsNullOrEmpty(LintelsCommonConfig.LintelLeftOffset)) {
                result = false;
                AddBlankParameterInfo(elementInfos, nameof(LintelsCommonConfig.LintelLeftOffset));
            }
            if(string.IsNullOrEmpty(LintelsCommonConfig.LintelRightCorner)) {
                result = false;
                AddBlankParameterInfo(elementInfos, nameof(LintelsCommonConfig.LintelRightCorner));
            }
            if(string.IsNullOrEmpty(LintelsCommonConfig.LintelRightOffset)) {
                result = false;
                AddBlankParameterInfo(elementInfos, nameof(LintelsCommonConfig.LintelRightOffset));
            }
            if(string.IsNullOrEmpty(LintelsCommonConfig.LintelThickness)) {
                result = false;
                AddBlankParameterInfo(elementInfos, nameof(LintelsCommonConfig.LintelThickness));
            }
            if(string.IsNullOrEmpty(LintelsCommonConfig.LintelWidth)) {
                result = false;
                AddBlankParameterInfo(elementInfos, nameof(LintelsCommonConfig.LintelWidth));
            }
            if(string.IsNullOrEmpty(LintelsCommonConfig.OpeningFixation)) {
                result = false;
                AddBlankParameterInfo(elementInfos, nameof(LintelsCommonConfig.OpeningFixation));
            }
            if(string.IsNullOrEmpty(LintelsCommonConfig.HolesFilter)) {
                result = false;
                AddBlankParameterInfo(elementInfos, nameof(LintelsCommonConfig.HolesFilter));
            }
            return result;
        }

        private ElementId GetFamilyCategoryId(Family family) {
            var typesId = family.GetFamilySymbolIds();
            if(typesId.Count > 0) {
                return _document.GetElement(typesId.First()).Category?.Id;
            }
            return ElementId.InvalidElementId;
        }

        /// <summary>
        /// Поворачивает первый элемент на угол поворота второго элемента вокруг Oz
        /// </summary>
        /// <param name="lintel">вращаемый элемент</param>
        /// <param name="elementInWall">повернутый элемент</param>
        /// <param name="center">точка поворота</param>
        private void RotateLintel(FamilyInstance lintel, FamilyInstance elementInWall, XYZ center) {
            if(lintel is null) {
                throw new ArgumentNullException(nameof(lintel));
            }

            if(elementInWall is null) {
                throw new ArgumentNullException(nameof(elementInWall));
            }

            if(center is null) {
                throw new ArgumentNullException(nameof(center));
            }

            var line = Line.CreateBound(center, new XYZ(center.X, center.Y, center.Z + 1));
            ElementTransformUtils.RotateElement(_document, lintel.Id, line, Math.PI + GetAngle(elementInWall));
        }

        /// <summary>
        /// Возвращает угол поворота элемента вокруг Oz
        /// </summary>
        /// <param name="elementInWall">элемент</param>
        /// <returns></returns>
        private double GetAngle(FamilyInstance elementInWall) {
            if(elementInWall is null) {
                throw new ArgumentNullException(nameof(elementInWall));
            }

            var transform = elementInWall.GetTransform();
            var vectorX = transform.OfVector(transform.BasisX);
            return transform.BasisX.AngleOnPlaneTo(vectorX, transform.BasisZ);
        }

        private void AddBlankParameterInfo(ElementInfosViewModel elementInfos, string parameterName) {
            var description = TypeDescriptor
                .GetProperties(LintelsCommonConfig)[parameterName]
                .Attributes
                .OfType<DescriptionAttribute>()
                .FirstOrDefault()?.Description;
            elementInfos.ElementInfos.Add(new ElementInfoViewModel(ElementId.InvalidElementId, InfoElement.BlankParamter.FormatMessage(description)));
        }
    }
}
