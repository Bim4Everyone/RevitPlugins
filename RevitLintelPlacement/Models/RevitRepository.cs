using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;

using dosymep.Revit;

using RevitLintelPlacement.ViewModels;

namespace RevitLintelPlacement.Models {

    internal class RevitRepository {
        private readonly string _view3DName = "3D_Перемычки"; 

        private readonly Application _application;
        private readonly UIApplication _uiApplication;

        private readonly Document _document;
        private readonly UIDocument _uiDocument;

        public RevitRepository(Application application, Document document, LintelsConfig lintelsConfig) {
            _application = application;
            _uiApplication = new UIApplication(application);

            _document = document;
            _uiDocument = new UIDocument(document);

            LintelsConfig = lintelsConfig;
            LintelsCommonConfig = LintelsCommonConfig.GetLintelsCommonConfig(lintelsConfig.LintelsConfigPath);
            RuleConfig = RuleConfig.GetRuleConfig(lintelsConfig.RulesCongigPaths.FirstOrDefault());

            CreateView3DIfNotExisted();
        }

        public LintelsConfig LintelsConfig { get; set; }
        public LintelsCommonConfig LintelsCommonConfig { get; set; }
        public RuleConfig RuleConfig { get; set; }

        public string GetDocumentName() {
            return _document.Title;
        }

        public FamilySymbol GetLintelType(string lintelTypeName) {
            return new FilteredElementCollector(_document)
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .WhereElementIsElementType()
                .First(e => e.Name == lintelTypeName) as FamilySymbol; 
        }

        public IEnumerable<FamilySymbol> GetLintelTypes() {
            return new FilteredElementCollector(_document)
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .WhereElementIsElementType()
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .Where(e => LintelsCommonConfig.LintelFamilies.Any(l => 
                l.Equals(e.Family?.Name, StringComparison.CurrentCultureIgnoreCase)));
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
                .Cast<WallType>();
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

        public FamilyInstance PlaceLintel(FamilySymbol lintelType, ElementId elementInWallId) {
            var elementInWall = _document.GetElement(elementInWallId) as FamilyInstance;

            if(!lintelType.IsActive)
                lintelType.Activate();
            XYZ center = GetLocationPoint(elementInWall);
            FamilyInstance lintel = _document.Create.NewFamilyInstance(center, lintelType, StructuralType.NonStructural);

            RotateLintel(lintel, elementInWall, center);
            return lintel;
        }

        public void DeleteLintel(FamilyInstance lintel) {
            using(Transaction t = new Transaction(_document)) {
                t.BIMStart("Удаление перемычки");
                _document.Delete(lintel.Id);
                t.Commit();
            }
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
                .Where(e => LintelsCommonConfig.LintelFamilies.Any(f => f.Equals(e.Symbol?.Family?.Name, StringComparison.CurrentCultureIgnoreCase)))
                .ToList();
        }

        public IEnumerable<FamilyInstance> GetAllElementsInWall(SampleMode sampleMode) {
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

            return collector
                .WherePasses(categoryFilter)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .Where(e=>e.Host is Wall && e.Location!=null);
        }

        public View GetElevation() {
            return new FilteredElementCollector(_document)
                .OfClass(typeof(View))
                .Cast<View>()
                .First(v => v.ViewType == ViewType.Elevation);
        }

        public View GetPlan() {
            return new FilteredElementCollector(_document)
               .OfClass(typeof(View))
               .Cast<View>()
               .First(v => v.ViewType == ViewType.FloorPlan);
        }

        //проверка, есть ли сверху элемента стена, у которой тип железобетон (в таком случает перемычку ставить не надо)
        public bool CheckUp(View3D view3D, FamilyInstance elementInWall) {
            XYZ viewPoint = GetLocationPoint(elementInWall);
            ReferenceWithContext refWithContext = 
                GetNearestWallOrColumn(view3D, elementInWall, new XYZ(viewPoint.X, viewPoint.Y, viewPoint.Z-0.1), new XYZ(0, 0, 1), false); //чтобы точка точно была под гранью стены
            if(refWithContext == null)
                return false;
            if(!(refWithContext.Proximity < 0.32)) { //10 см
                return false;
            }
            var wall = _document.GetElement(refWithContext.GetReference().ElementId);
            if(LintelsCommonConfig.ReinforcedConcreteFilter.Any(f=>wall.Name.ToLower().Contains(f))) {
                return false;
            }
            refWithContext = GetNearestWallOrColumn(view3D, elementInWall, viewPoint, new XYZ(0, 0, 1), true);
            if(refWithContext == null)
                return true;
            if(!(refWithContext.Proximity < 0.32)) {
                return false;
            }
            wall = _document.GetElement(refWithContext.GetReference().ElementId);
            return !LintelsCommonConfig.ReinforcedConcreteFilter.Any(f => wall.Name.ToLower().Contains(f));
        }

        public bool CheckHorizontal(View3D view3D, FamilyInstance elementInWall, bool isRight, IEnumerable<string> linkNames, out double offset) {
            offset = 0;
            XYZ viewPoint = GetLocationPoint(elementInWall);
            var direction = elementInWall.GetTransform().OfVector(isRight? new XYZ(1, 0, 0) : new XYZ(-1, 0, 0));
            ReferenceWithContext refWithContext = GetNearestWallOrColumn(view3D, elementInWall, viewPoint, direction, true);
            if(refWithContext == null)
                return false;
            var elementWidth = elementInWall.GetParamValueOrDefault(LintelsCommonConfig.OpeningWidth) 
                ?? elementInWall.GetParamValueOrDefault(BuiltInParameter.FAMILY_WIDTH_PARAM); //Todo: параметр
            if(refWithContext.Proximity > ((double) elementWidth / 2 + 0.4)) {// 0.4 фута примерно = 100 см
                return false;
            }
            offset = refWithContext.Proximity - (double) elementWidth / 2;
            var wallOrColumn = _document.GetElement(refWithContext.GetReference().ElementId);
            if(wallOrColumn is Wall wall)
                return LintelsCommonConfig.ReinforcedConcreteFilter.Any(f => wall.Name.ToLower().Contains(f));  //TODO: часть названия типа стены
            if(wallOrColumn.Category.Id == new ElementId(BuiltInCategory.OST_StructuralColumns))
                return true;
            if(wallOrColumn is RevitLinkInstance linkedInstance) {
                return linkNames.Any(l => l.Equals(linkedInstance.GetLinkDocument().Title, StringComparison.CurrentCultureIgnoreCase));
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

        public void SelectAndShowElement(ElementId id, ViewOrientation3D orientation) {
            var element = _document.GetElement(id);
            var view3D = new FilteredElementCollector(_document)
              .OfClass(typeof(View3D))
              .Cast<View3D>()
              .First(v => !v.IsTemplate && v.Name == _view3DName); //TODO: тут тоже нужен свой 3D вид, но лучше нижняя реализация с асинхронностью

            using(TransactionGroup tg = new TransactionGroup(_document)) {
                tg.Start("BIM: Подрезка");
                using(var t = StartTransaction("Подрезка")) {
                    view3D.IsSectionBoxActive = false;
                    view3D.SetOrientation(orientation);
                    t.Commit();
                }

                using(var t = StartTransaction("Подрезка")) {

                    var bb = element.get_BoundingBox(view3D);
                    bb.Max = new XYZ(bb.Max.X+1, bb.Max.Y+1, bb.Max.Z + 1);
                    bb.Min = new XYZ(bb.Min.X - 1, bb.Min.Y - 1, bb.Min.Z - 1);
                    view3D.SetSectionBox(bb);
                    _uiDocument.SetSelectedElements(element);
                    _uiDocument.ShowElements(element);
                    t.Commit();
                }
                tg.Assimilate();
            }
            // Если будет асинхронный Task
            //_uiDocument.Selection.SetElementIds(new List<ElementId> { id });
            //var commandId = RevitCommandId.LookupCommandId("ID_VIEW_APPLY_SELECTION_BOX");
            //if(!(commandId is null) && _uiDocument.Application.CanPostCommand(commandId)) {
            //    _uiApplication.PostCommand(commandId);
            //}
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
              .FirstOrDefault(v => !v.IsTemplate && v.Name==_view3DName);
            if(view3D != null) {
                //_uiDocument.ActiveView = view3D;
                return;
            }
            using (Transaction t = new Transaction(_document)) {
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
            var categoryFilter = new ElementCategoryFilter(BuiltInCategory.OST_StructuralColumns);
            var logicalAndFilter = new LogicalAndFilter(new List<ElementFilter> { exclusionFilter, classFilter });
            var logicalOrFilter = new LogicalOrFilter(new List<ElementFilter> { logicalAndFilter, categoryFilter, classFilter2 });
            var refIntersector = new ReferenceIntersector(logicalOrFilter, FindReferenceTarget.All, view3D);

            return refIntersector.FindNearest(viewPoint, direction);
        }

        public void LockLintel(View elevation, View plan, FamilyInstance lintel, FamilyInstance elementInWall) {
            
            var leftRightElement = elementInWall.GetReferences(FamilyInstanceReferenceType.CenterLeftRight);
            var leftRightLintel = lintel.GetReferences(FamilyInstanceReferenceType.CenterLeftRight);
            if (leftRightElement.Count > 0 && leftRightLintel.Count > 0)
                _document.Create.NewAlignment(_document.ActiveView, leftRightLintel.First(), leftRightElement.First());

            var topElement = elementInWall.GetReferences(FamilyInstanceReferenceType.Top);
            var bottomLintel = lintel.GetReferences(FamilyInstanceReferenceType.CenterElevation);

            try {
                if(topElement.Count > 0 && bottomLintel.Count > 0)
                    _document.Create.NewAlignment(elevation, topElement.First(), bottomLintel.First());
            } catch 
            { }


            var leftL = lintel.GetReferences(FamilyInstanceReferenceType.Front);
            var rightL = lintel.GetReferences(FamilyInstanceReferenceType.Back);
            var wallReferences1 = HostObjectUtils.GetSideFaces((Wall) elementInWall.Host, ShellLayerType.Interior);
            var wallReferences2 = HostObjectUtils.GetSideFaces((Wall) elementInWall.Host, ShellLayerType.Exterior);


            //возможно, ошибка возникает при устновке параметра половина толщины, поэтому нет геометричкого выравнивания
            try {
                if(leftL.Count > 0 && wallReferences1.Count > 0) {
                    _document.Create.NewAlignment(plan, leftL.First(), wallReferences1.First());
                }               
            } catch {
                try {
                    if(leftL.Count > 0 && wallReferences1.Count > 0) {
                        _document.Create.NewAlignment(plan, leftL.First(), wallReferences2.First());
                    }

                } catch {

                }
            }
        }

        public XYZ GetLocationPoint(FamilyInstance elementInWall) {
            if(elementInWall is null) {
                throw new ArgumentNullException(nameof(elementInWall));
            }
            var location = ((LocationPoint) elementInWall.Location).Point;
            var level = _document.GetElement(elementInWall.LevelId) as Level;
            var bottomBarHeight = (double) elementInWall.GetParamValueOrDefault(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM);
            var height = elementInWall.GetParamValueOrDefault(LintelsCommonConfig.OpeningHeight); //ToDo: параметр
            double z;
            if(height != null) {
                z = (double)height + bottomBarHeight + level.Elevation;
            } else {
                var topBarHeight = (double) elementInWall.GetParamValueOrDefault(BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM);
                z = /*location.Z +*/ topBarHeight + level.Elevation;
            }
            return new XYZ(location.X, location.Y, z);
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
            ElementTransformUtils.RotateElement(_document, lintel.Id, line, GetAngle(elementInWall));
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



        private XYZ GetViewStartPoint(FamilyInstance lintel, bool plusDirection) //изменить название метода
        {
            if(lintel is null) {
                throw new ArgumentNullException(nameof(lintel));
            }

            var normal = new XYZ(0, 1, 0);
            var direction = lintel.GetTransform().OfVector(normal);
            var demiWidth = (double) lintel.GetParamValueOrDefault(LintelsCommonConfig.LintelThickness) / 2; //TODO: Параметр!
            if(plusDirection)
                return ((LocationPoint) lintel.Location).Point + direction * demiWidth;
            return ((LocationPoint) lintel.Location).Point - direction * demiWidth;
        }

        private IEnumerable<Face> GetOrientedFaces(Element element, Orientation orientation) {
            if(element is null) {
                throw new ArgumentNullException(nameof(element));
            }

            foreach(GeometryObject geometryObject in element.get_Geometry(new Options() { ComputeReferences=true})) {
                if(geometryObject is Solid solid) {
                    foreach(var face in GetOrientedFacesFromSolid(solid, orientation))
                        yield return face;
                } else {
                    var geometryInstance = geometryObject as GeometryInstance;
                    if(geometryInstance == null)
                        continue;

                    foreach(var instObj in geometryInstance.GetInstanceGeometry()) {
                        if(!(instObj is Solid solidFromGeomInst)) {
                            continue;
                        }

                        var horizontalFacesFromSolid = GetOrientedFacesFromSolid(solidFromGeomInst, orientation);
                        if(horizontalFacesFromSolid != null) {
                            foreach(var face in GetOrientedFacesFromSolid(solidFromGeomInst, orientation)) {
                                yield return face;
                            }
                        }
                    }
                }            
            }
        }

        private IEnumerable<Face> GetOrientedFacesFromSolid(Solid solid, Orientation orientation) {
            if(solid is null) {
                throw new ArgumentNullException(nameof(solid));
            }

            foreach(Face face in solid.Faces) {
                var normal = face.ComputeNormal(new UV(0.5, 0.5));
                switch(orientation) {
                    case Orientation.VerticalUp: {
                        if(Math.Abs(normal.Z - 1) < 0.001)
                            yield return face;
                        break;
                    }
                    case Orientation.VerticalDown: {
                        if(Math.Abs(normal.Z + 1) < 0.001)
                            yield return face;
                        break;
                    }
                    case Orientation.Horizontal: {
                        if(Math.Abs(Math.Pow(normal.X, 2) + Math.Pow(normal.Y, 2) - 1) < 0.001)
                            yield return face;
                        break;
                    }
                }
            }
        }

    }

    enum Orientation {
        VerticalUp,
        VerticalDown,
        Horizontal
    }
}
