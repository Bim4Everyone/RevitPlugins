using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;

using dosymep.Revit;

namespace RevitLintelPlacement.Models {

    internal class RevitRepository {
        private readonly string _lintelTypeName = "155_Перемычка"; //TODO: название типа перемычки

        private readonly Application _application;
        private readonly UIApplication _uiApplication;

        private readonly Document _document;
        private readonly UIDocument _uiDocument;

        public RevitRepository(Application application, Document document) {
            _application = application;
            _uiApplication = new UIApplication(application);

            _document = document;
            _uiDocument = new UIDocument(document);
        }

        public FamilySymbol GetLintelType() {
            return new FilteredElementCollector(_document)
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .WhereElementIsElementType()
                .First(e => e.Name == _lintelTypeName) as FamilySymbol; 

        }

        public Element GetElementById(ElementId id) {
            return _document.GetElement(id);
        }

        public Category GetCategory(BuiltInCategory builtInCategory) {
            return Category.GetCategory(_document, builtInCategory);
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

        //не работает, если нет геометрии у FamilyInsatnce
        //пока проверка только по центру, нужно еще пытаться найти элементы на границах перемычки (ситуация, когда несколько проемов под ней)
        public ElementId GetNearestElement(FamilyInstance fi) {
            var view3D = new FilteredElementCollector(_document)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .First(v => !v.IsTemplate); //не любой 3D-вид подойдет (нужен со всей геометрией) //TODO: создать свой 3D-вид (для FindNearest и подрезки)
            var exclusionList = new List<ElementId> { fi.Id };
            exclusionList.AddRange(fi.GetDependentElements(new ElementClassFilter(typeof(FamilyInstance))));
            var exclusionFilter = new ExclusionFilter(exclusionList);
            var classFilter = new ElementClassFilter(typeof(FamilyInstance));
            var logicalFilter = new LogicalAndFilter(new List<ElementFilter> { exclusionFilter, classFilter });
            var refIntersector = new ReferenceIntersector(logicalFilter, FindReferenceTarget.All, view3D);

            var refWithContext1 = refIntersector.FindNearest(GetViewStartPoint(fi, true), new XYZ(0, 0, -1));

            var refWithContext2 = refIntersector.FindNearest(GetViewStartPoint(fi, false), new XYZ(0, 0, -1));

            if(refWithContext1 == null && refWithContext2 == null)
                return ElementId.InvalidElementId;

            ReferenceWithContext neededRef = refWithContext1 == null ? refWithContext2 : refWithContext1;

            if(refWithContext1 != null && refWithContext2 != null) {
                neededRef = refWithContext1.Proximity > refWithContext2.Proximity
                ? refWithContext2
                : refWithContext1;
            }
            if(neededRef.Proximity < 0.1) { //TODO: придумать расстояние, при котором считается еще, что перемычка над проемом
                return neededRef.GetReference().ElementId;
            }
            return ElementId.InvalidElementId;
        }


        public IEnumerable<Element> GetElements(IEnumerable<ElementId> ids) {
            foreach(var id in ids)
                yield return _document.GetElement(id);
        }

        public IEnumerable<FamilyInstance> GetLintels() {

            return new FilteredElementCollector(_document)
                .OfCategory(BuiltInCategory.OST_GenericModel)
                .OfClass(typeof(FamilyInstance))
                .Where(e => e.Name == _lintelTypeName)
                .Cast<FamilyInstance>()
                .ToList();
        }

        

        public IEnumerable<FamilyInstance> GetAllElementsInWall() {
            var categoryFilter = new ElementMulticategoryFilter(new List<BuiltInCategory> { BuiltInCategory.OST_Doors, BuiltInCategory.OST_Windows });

            return new FilteredElementCollector(_document)
                .WherePasses(categoryFilter)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>();
        }

        //проверка, есть ли сверху элемента стена, у которой класс материала не кладка (в таком случает перемычку ставить не надо)
        public bool CheckUp(FamilyInstance elementInWall) {

            XYZ viewPoint = GetLocationPoint(elementInWall);
            var wall = GetNearestElement(elementInWall, viewPoint, typeof(Wall), new XYZ(0, 0, 1));
            if(wall == ElementId.InvalidElementId) {
                return false;
            }

            foreach(var materialClass in GetMaterialClasses(_document.GetElement(wall))) {
                if(materialClass.Equals("Кладка", StringComparison.CurrentCultureIgnoreCase)) //TODO: класс материала
                    return true;
            }
            return false;
        }

        public Transaction StartTransaction(string transactionName) {
            var transaction = new Transaction(_document);
            transaction.BIMStart(transactionName);

            return transaction;
        }

        public ViewOrientation3D GetOrientation3D() {
            var view3D = _document.ActiveView as View3D;
            return view3D.GetOrientation();
        }

        public bool IsActivView3D() {
            return _document.ActiveView is View3D;
        }

        public void SelectAndShowElement(ElementId id, ViewOrientation3D orientation) {
            var element = _document.GetElement(id);
            var view3D = _document.ActiveView as View3D; //TODO: тут тоже нужен свой 3D вид, но лучше нижняя реализация с асинхронностью

            using(TransactionGroup tg = new TransactionGroup(_document)) {
                tg.Start("BIM: Подрезка");
                using(var t = StartTransaction("Подрезка")) {
                    view3D.IsSectionBoxActive = false;
                    view3D.SetOrientation(orientation);
                    t.Commit();
                }

                using(var t = StartTransaction("Подрезка")) {

                    var bb = element.get_BoundingBox(view3D);
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
            var dimensionids = fi.GetDependentElements(new ElementClassFilter(typeof(Dimension)));
            if(dimensionids.Any()) {
                foreach(var id in dimensionids) {
                    var dimension = (Dimension) _document.GetElement(id);
                    var references = dimension.References;
                    foreach(Reference refer in references) {
                        if(refer.ElementId == fi.Id)
                            continue;
                        if(_document.GetElement(refer.ElementId) is FamilyInstance allignFi)
                            return allignFi;
                    }
                }
            }
            return null;

        }


        private IEnumerable<string> GetMaterialClasses(Element element) {
            var materialIds = element.GetMaterialIds(false);
            foreach(var id in materialIds) {
                yield return ((Material) _document.GetElement(id)).MaterialClass;
            }
        }

       
        private ElementId GetNearestElement(FamilyInstance fi, XYZ viewPoint, Type elementType, XYZ direction) {
            //создавать свой 3D - вид
            var view3D = new FilteredElementCollector(_document)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .First(v => !v.IsTemplate);
            view3D = (View3D)_document.ActiveView;
            var exclusionList = new List<ElementId> { fi.Id };
            exclusionList.AddRange(fi.GetDependentElements(new ElementClassFilter(typeof(FamilyInstance))));
            var exclusionFilter = new ExclusionFilter(exclusionList);
            var classFilter = new ElementClassFilter(typeof(Wall));
            var logicalFilter = new LogicalAndFilter(new List<ElementFilter> { exclusionFilter, classFilter });
            var refIntersector = new ReferenceIntersector(logicalFilter, FindReferenceTarget.All, view3D);

            var refWithContext = refIntersector.FindNearest(viewPoint, direction);

            if(refWithContext == null)
                return ElementId.InvalidElementId;

            if(refWithContext.Proximity < 0.01) { 
                return refWithContext.GetReference().ElementId;
            }
            return ElementId.InvalidElementId;
        }

        public void LockLintel(FamilyInstance lintel, FamilyInstance elementInWall) {
            
            var LeftRightElement = elementInWall.GetReferences(FamilyInstanceReferenceType.CenterLeftRight);
            var LeftRightLintel = lintel.GetReferences(FamilyInstanceReferenceType.CenterLeftRight);
            if (LeftRightElement.Any() && LeftRightLintel.Any())
                _document.Create.NewAlignment(_document.ActiveView, LeftRightLintel.First(), LeftRightElement.First());

            var topElement = elementInWall.GetReferences(FamilyInstanceReferenceType.Top);
            var bottomLintel = lintel.GetReferences(FamilyInstanceReferenceType.CenterElevation);

            var elevation = new FilteredElementCollector(_document)
                .OfClass(typeof(View))
                .Cast<View>()
                .First(v => v.ViewType == ViewType.Elevation);
            if (topElement.Any() && bottomLintel.Any())
                _document.Create.NewAlignment(elevation, topElement.First(), bottomLintel.First());

            var leftL = lintel.GetReferences(FamilyInstanceReferenceType.Front);
            var rightL = lintel.GetReferences(FamilyInstanceReferenceType.Back);
            var wallReferences1 = HostObjectUtils.GetSideFaces((Wall) elementInWall.Host, ShellLayerType.Interior);
            var wallReferences2 = HostObjectUtils.GetSideFaces((Wall) elementInWall.Host, ShellLayerType.Exterior);
            var plan = new FilteredElementCollector(_document)
               .OfClass(typeof(View))
               .Cast<View>()
               .First(v => v.ViewType == ViewType.FloorPlan);

            //возможно, ошибка возникает при устновке параметра половина толщины, поэтому нет геометричкого выравнивания
            try {
                if(leftL.Any() && wallReferences1.Any()) {
                    _document.Create.NewAlignment(plan, leftL.First(), wallReferences1.First());
                }

                if(rightL.Any() && wallReferences2.Any()) {
                    _document.Create.NewAlignment(plan, rightL.First(), wallReferences2.First());
                }
            } catch {

            }
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
            return Math.PI + transform.BasisX.AngleOnPlaneTo(vectorX, transform.BasisZ);
        }

        private XYZ GetLocationPoint(FamilyInstance elementInWall) {
            if(elementInWall is null) {
                throw new ArgumentNullException(nameof(elementInWall));
            }

            var topBarHeight = (double) elementInWall.GetParamValueOrDefault(BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM); //TODO: возможно, не всегда этот параметр
            //var bottomBarHeight = elementInWall.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM);
            //var levelHeight = ((Level) _document.GetElement(elementInWall.LevelId)).Elevation;
            var location = ((LocationPoint) elementInWall.Location).Point;
            var z = location.Z + topBarHeight; //+ bottomBarHeight.AsDouble(); //TODO: тут, наверное, нужен параметр, пока так
            return new XYZ(location.X, location.Y, z);
        }

        private XYZ GetViewStartPoint(FamilyInstance lintel, bool plusDirection) //изменить название метода
        {
            if(lintel is null) {
                throw new ArgumentNullException(nameof(lintel));
            }

            var normal = new XYZ(0, 1, 0);
            var direction = lintel.GetTransform().OfVector(normal);
            var demiWidth = (double) lintel.GetParamValueOrDefault("Половина толщины стены"); //TODO: Параметр!
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
                    var geometryInstace = geometryObject as GeometryInstance;
                    if(geometryInstace == null)
                        continue;

                    foreach(var instObj in geometryInstace.GetInstanceGeometry()) {
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

        public IEnumerable<Element> GetElementTest() {


            //var element = _document.GetElement(new ElementId(7855887)) as FamilyInstance;
            //if(!(element.Host == null || !(element.Host is Wall wall1))) {
            //    var materials = GetElements(wall1.GetMaterialIds(false)); //TODO: может быть и true, проверить
            //    foreach(var m in materials) {
            //        if("Кладка".Equals(((Material) m).MaterialClass, StringComparison.CurrentCultureIgnoreCase)) {
            //            if(!wall1.Name.ToLower().Contains("невозводим")) {
            //                var elementWidth = (double) element.Symbol.GetParamValueOrDefault(BuiltInParameter.FAMILY_WIDTH_PARAM);
            //                double openingWidth = UnitUtils.ConvertFromInternalUnits(elementWidth, DisplayUnitType.DUT_MILLIMETERS);
            //                if(400 <= openingWidth && openingWidth < 2500)
            //                    yield return element;
            //            }
            //        } //TODO: для английской версии дожен быть config

            //    }
            //}


            var smth = GetAllElementsInWall();
            foreach(var s in smth) {
                if(s.Host == null || !(s.Host is Wall wall))
                    continue;
                var materials = GetElements(wall.GetMaterialIds(false)); //TODO: может быть и true, проверить
                foreach(var m in materials) {
                    if("Кладка".Equals(((Material) m).MaterialClass, StringComparison.CurrentCultureIgnoreCase)) {
                        if(!wall.Name.ToLower().Contains("невозводим")) {
                            var elementWidth = (double) s.GetParamValueOrDefault("ADSK_Размер_Ширина");
                            double openingWidth = UnitUtils.ConvertFromInternalUnits(elementWidth, DisplayUnitType.DUT_MILLIMETERS);
                            if (200<= openingWidth && openingWidth< 2500)
                                yield return s;
                        }
                    } //TODO: для английской версии дожен быть config
                        
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
                    case Orientation.VertiacalUp: {
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
        VertiacalUp,
        VerticalDown,
        Horizontal
    }
}
