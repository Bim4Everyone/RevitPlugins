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
using dosymep.Revit;

using RevitLintelPlacement.Handlers;
using RevitLintelPlacement.ViewModels;

namespace RevitLintelPlacement.Models {

    internal class RevitRepository {
        private readonly string _view3DName = "3D_Перемычки";
        private static readonly string _settingsPath = @"T:\Проектный институт\Отдел стандартизации BIM и RD\BIM-Ресурсы\5-Надстройки\Bim4Everyone\A101\";

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

            CreateView3DIfNotExisted();
        }

        public LintelsConfig LintelsConfig { get; set; }
        public LintelsCommonConfig LintelsCommonConfig { get; set; }

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
            return new FilteredElementCollector(_document)
              .OfClass(typeof(View3D))
              .Cast<View3D>()
              .First(v => !v.IsTemplate && v.Name == _view3DName);
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
                .Cast<RevitLinkType>();
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

        public IEnumerable<FamilyInstance> GetLintels(SampleMode sampleMode) {
            FilteredElementCollector collector;
            switch(sampleMode) {
                case SampleMode.AllElements: {
                    collector = new FilteredElementCollector(_document);
                    break;
                }
                case SampleMode.CurrentView: {
                    collector = new FilteredElementCollector(_document, _document.ActiveView.Id);
                    break;
                }
                case SampleMode.SelectedElements: {
                    var ids = _uiDocument.Selection.GetElementIds();
                    if(ids.Count == 0) {
                        return new List<FamilyInstance>();
                    }
                    collector = new FilteredElementCollector(_document, ids);
                    break;
                }
                default:
                throw new ArgumentException(nameof(sampleMode), $"Способ выборки \"{nameof(sampleMode)}\" не найден.");
            }

            return collector
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(e => LintelsCommonConfig.LintelFamily.Equals(e.Symbol?.Family?.Name, StringComparison.CurrentCultureIgnoreCase))
                .ToList();
        }

        public IEnumerable<FamilyInstance> GetAllElementsInWall(SampleMode sampleMode, ElementInfosViewModel elementInfos, IEnumerable<string> wallTypes = null) {
            var categoryFilter = new ElementMulticategoryFilter(
                new List<BuiltInCategory> { BuiltInCategory.OST_Doors, BuiltInCategory.OST_Windows });

            FilteredElementCollector collector;
            switch(sampleMode) {
                case SampleMode.AllElements: {
                    collector = new FilteredElementCollector(_document);
                    break;
                }
                case SampleMode.CurrentView: {
                    collector = new FilteredElementCollector(_document, _document.ActiveView.Id);
                    break;
                }
                case SampleMode.SelectedElements: {
                    var ids = _uiDocument.Selection.GetElementIds();
                    if(ids.Count == 0) {
                        return new List<FamilyInstance>();
                    }
                    collector = new FilteredElementCollector(_document, ids);
                    break;
                }
                default:
                throw new ArgumentException(nameof(sampleMode), $"Способ выборки \"{nameof(sampleMode)}\" не найден.");
            }

            var smth = collector.OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(f => f?.Symbol != null && f.Symbol?.Family != null && f.Symbol.Family.Name != null &&
                f.Symbol.Family.Name.ToLower().Contains(LintelsCommonConfig.HolesFilter.ToLower())).ToList();

            var smth2 = collector
                .WherePasses(categoryFilter)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(e => e.Host is Wall && e.Location != null).ToList();

            return smth2.Union(smth, new FamilyInstanceComparer()).Where(e => CheckElementInWallParameter(e, elementInfos)).ToList();

            //return collector
            //    .WherePasses(categoryFilter)
            //    .OfClass(typeof(FamilyInstance))
            //    .Cast<FamilyInstance>()
            //    .Where(e => e.Host is Wall && e.Location != null)
            //    .ToList()
            //    .Union(
            //     collector.OfClass(typeof(FamilyInstance))
            //    .Cast<FamilyInstance>()
            //    .Where(f => f?.Symbol != null && f.Symbol?.Family != null && f.Symbol.Family.Name != null &&
            //    f.Symbol.Family.Name.ToLower().Contains(LintelsCommonConfig.HolesFilter.ToLower())).ToList(), new FamilyInstanceComparer())
            //    .Where(e => CheckElementInWallParameter(e, elementInfos));
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
            viewPoint = new XYZ(viewPoint.X, viewPoint.Y, viewPoint.Z + (((Level) _document.GetElement(elementInWall.LevelId))?.Elevation ?? 0));
            ReferenceWithContext refWithContext =
                GetNearestWallOrColumn(view3D, elementInWall, new XYZ(viewPoint.X, viewPoint.Y, viewPoint.Z - 0.32), new XYZ(0, 0, 1), false); //чтобы точка точно была под гранью стены
            if(refWithContext == null)
                return false;
            if(refWithContext.Proximity > 0.52) { //10 см
                return false;
            }
            var wallOrColumn = _document.GetElement(refWithContext.GetReference().ElementId);
            if(LintelsCommonConfig.ReinforcedConcreteFilter.Any(f => wallOrColumn.Name.ToLower().Contains(f.ToLower()))) {
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
                return !LintelsCommonConfig.ReinforcedConcreteFilter.Any(f => wall.Name.ToLower().Contains(f.ToLower()));
            if(wallOrColumn.Category.Id == new ElementId(BuiltInCategory.OST_StructuralColumns) || wallOrColumn.Category.Id == new ElementId(BuiltInCategory.OST_StructuralFraming))
                return false;
            if(wallOrColumn is RevitLinkInstance linkedInstance) {
                return !linkNames.Any(l => l.Equals(linkedInstance.GetLinkDocument().Title, StringComparison.CurrentCultureIgnoreCase));
            }
            return true;
        }

        public bool DoesRightCornerNeeded(View3D view3D, FamilyInstance elementInWall, IEnumerable<string> linkNames, ElementInfosViewModel elementInfos, out double offset) {
            return DoesCornerNeeded(view3D, elementInWall, new XYZ(-1, 0, 0), linkNames, elementInfos, out offset);
        }

        public bool DoesLeftCornerNeeded(View3D view3D, FamilyInstance elementInWall, IEnumerable<string> linkNames, ElementInfosViewModel elementInfos, out double offset) {
            return DoesCornerNeeded(view3D, elementInWall, new XYZ(1, 0, 0), linkNames, elementInfos, out offset);
        }

        private bool DoesCornerNeeded(View3D view3D, FamilyInstance elementInWall, XYZ direction, IEnumerable<string> linkNames, ElementInfosViewModel elementInfos, out double realOffset) {
            realOffset = 0;
            //получение предполагаемой точки вставки перемычки,
            //из которой проводится поиск жб-элементов
            XYZ viewPoint = GetLocationPoint(elementInWall);
            viewPoint = new XYZ(viewPoint.X, viewPoint.Y, viewPoint.Z + (((Level) _document.GetElement(elementInWall.LevelId))?.Elevation ?? 0));

            //направление, в котором будет проводиться поиск
            direction = elementInWall.GetTransform().OfVector(direction);

            //получение ссылки на ближайшую жб-стену, колонну или связанный файл и расстояние до них
            ReferenceWithContext refWithContext = GetNearestWallOrColumn(view3D, elementInWall, viewPoint, direction, true);

            //Если жб-стены или колонны не найдены, то уголок не нужен
            if(refWithContext == null)
                return false;
            var elementWidth = elementInWall.GetParamValueOrDefault(LintelsCommonConfig.OpeningWidth)
                ?? elementInWall.Symbol.GetParamValueOrDefault(LintelsCommonConfig.OpeningWidth)
                ?? elementInWall.Symbol.GetParamValueOrDefault(BuiltInParameter.FAMILY_WIDTH_PARAM);

            if(elementWidth == null) {
                elementInfos.ElementInfos.Add(new ElementInfoViewModel(elementInWall.Id,
                    InfoElement.MissingOpeningParameter.FormatMessage(LintelsCommonConfig.OpeningWidth)) {
                    Name = elementInWall.Name,
                    LevelName = elementInWall.LevelId != null ? GetElementById(elementInWall.LevelId)?.Name : null
                });
                return false;
            }

#if REVIT_2020_OR_LESS
            var offset = UnitUtils.ConvertToInternalUnits(200, DisplayUnitType.DUT_MILLIMETERS);
#else
            var offset = UnitUtils.ConvertToInternalUnits(200, UnitTypeId.Millimeters);
#endif
            //получение ближайшего элемента и его проверка
            var wallOrColumn = _document.GetElement(refWithContext.GetReference().ElementId);

            if((wallOrColumn is Wall wall && LintelsCommonConfig.ReinforcedConcreteFilter.Any(f => wall.Name.IndexOf(f, StringComparison.CurrentCultureIgnoreCase) > 0))
            || (wallOrColumn.Category.Id == new ElementId(BuiltInCategory.OST_StructuralColumns) || wallOrColumn.Category.Id == new ElementId(BuiltInCategory.OST_StructuralFraming))
            || (wallOrColumn is RevitLinkInstance linkedInstance && linkNames.Any(l => l.Equals(linkedInstance.GetLinkDocument().Title, StringComparison.CurrentCultureIgnoreCase)))){
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
              .First(v => !v.IsTemplate && v.Name == _view3DName);
            return view3D.GetOrientation();
        }

        public bool IsActiveView3D() {
            return _document.ActiveView is View3D;
        }

        public async Task SelectAndShowElement(ElementId id, ViewOrientation3D orientation) {
            _revitEventHandler.TransactAction = () => {
                _uiDocument.Selection.SetElementIds(new[] { id });
                var commandId = RevitCommandId.LookupCommandId("ID_VIEW_APPLY_SELECTION_BOX");
                if(!(commandId is null) && _uiDocument.Application.CanPostCommand(commandId)) {
                    _uiApplication.PostCommand(commandId);
                }
            };
            await _revitEventHandler.Raise();
        }

        public FamilyInstance GetDimensionFamilyInstance(FamilyInstance fi) {
            var dimensionIds = fi.GetDependentElements(new ElementClassFilter(typeof(Dimension)));
            if(!dimensionIds.Any()) {
                return null;
            }

            foreach(var id in dimensionIds) {
                var dimension = (Dimension) _document.GetElement(id);
                var references = dimension.References;
                foreach(Reference refer in references) {
                    if(refer.ElementId == fi.Id)
                        continue;
                    if(_document.GetElement(refer.ElementId) is FamilyInstance allignFi)
                        return allignFi;
                }
            }
            return null;
        }

        public void CreateView3DIfNotExisted() {
            var view3D = new FilteredElementCollector(_document)
              .OfClass(typeof(View3D))
              .Cast<View3D>()
              .FirstOrDefault(v => !v.IsTemplate && v.Name == _view3DName);
            if(view3D != null) {
                //_uiDocument.ActiveView = view3D;
                return;
            }
            using(Transaction t = new Transaction(_document)) {
                t.BIMStart("Создание 3D-вида");
                var type = new FilteredElementCollector(_document)
                    .OfClass(typeof(ViewFamilyType))
                    .Cast<ViewFamilyType>()
                    .First(v => v.ViewFamily == ViewFamily.ThreeDimensional);
                view3D = View3D.CreateIsometric(_document, type.Id);
                view3D.Name = _view3DName;
                t.Commit();
            }
            _uiDocument.ActiveView = view3D;
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

        public XYZ GetLocationPoint(FamilyInstance elementInWall) {
            if(elementInWall is null) {
                throw new ArgumentNullException(nameof(elementInWall));
            }
            var location = ((LocationPoint) elementInWall.Location).Point;
            //var level = _document.GetElement(elementInWall.LevelId) as Level;
            var height = elementInWall.GetParamValueOrDefault(LintelsCommonConfig.OpeningHeight)
               ?? elementInWall.Symbol.GetParamValueOrDefault(LintelsCommonConfig.OpeningHeight);

            var bottomBarHeight = (double) elementInWall.GetParamValueOrDefault(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM);
            if(bottomBarHeight == 0) {
                bottomBarHeight = (double) elementInWall.GetParamValueOrDefault(BuiltInParameter.INSTANCE_ELEVATION_PARAM);
            }
            if(bottomBarHeight == 0) {
                bottomBarHeight = (double) elementInWall.GetParamValueOrDefault(BuiltInParameter.INSTANCE_FREE_HOST_OFFSET_PARAM);
            }

            double z;
            if(height != null) {
                z = (double) height + (double) bottomBarHeight /*+ (level?.Elevation ?? 0)*/;
            } else {
                var topBarHeight = (double) elementInWall.GetParamValueOrDefault(BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM);
                z = topBarHeight /*+ (level?.Elevation ?? 0)*/;
            }
            return new XYZ(location.X, location.Y, z);
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
            if(string.IsNullOrEmpty(LintelsCommonConfig.OpeningHeight)) {
                result = false;
                AddBlankParameterInfo(elementInfos, nameof(LintelsCommonConfig.OpeningHeight));
            }
            if(string.IsNullOrEmpty(LintelsCommonConfig.OpeningWidth)) {
                result = false;
                AddBlankParameterInfo(elementInfos, nameof(LintelsCommonConfig.OpeningWidth));
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
