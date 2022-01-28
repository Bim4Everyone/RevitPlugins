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
                .First(e => e.Name == "155_Перемычка") as FamilySymbol;

        }

        public Element GetElementById(ElementId id) {
            return _document.GetElement(id);
        }

        public FamilyInstance PlaceLintel(FamilySymbol lintelType, ElementId elementInWallId) {
            FamilyInstance lintel = null;
            XYZ center;
            var elementInWall = _document.GetElement(elementInWallId) as FamilyInstance;

            if(!lintelType.IsActive)
                lintelType.Activate();
            center = GetLocationPoint(elementInWall);
            lintel = _document.Create.NewFamilyInstance(center, lintelType, StructuralType.NonStructural);

            RotateLintel(lintel, elementInWall, center);
            return lintel;
        }

        public ElementId GetNearestElement(FamilyInstance fi) {
            var view3D = new FilteredElementCollector(_document)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .First(v => !v.IsTemplate);
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
                .Where(e => e.Name == "155_Перемычка")
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

        public bool CheckUp(FamilyInstance elementInWall) {
            if(elementInWall.Id == new ElementId(4815652)) {
                var smrh = 3;
            }
            var viewPoint = GetLocationPoint(elementInWall);
            var wall = GetNearestElement(elementInWall, viewPoint, typeof(Wall), new XYZ(0, 0, 1));
            if(wall == ElementId.InvalidElementId)
                return false;
            foreach(var materialClass in GetMaterialClasses(_document.GetElement(wall))) {
                if(materialClass.Equals("Кладка", StringComparison.InvariantCultureIgnoreCase))
                    return true;
            }
            return false;
        }

        public Transaction StartTransaction(string transactionName) {
            var transaction = new Transaction(_document);
            transaction.BIMStart(transactionName);

            return transaction;
        }

        

        private IEnumerable<string> GetMaterialClasses(Element element) {
            var materialIds = element.GetMaterialIds(false);
            foreach(var id in materialIds) {
                yield return ((Material) _document.GetElement(id)).MaterialClass;
            }
        }

        //пока проверка только по центру, нужно еще пытаться найти элементы на границах перемычки (ситуация, когда несколько проемов под ней)
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

            if(refWithContext.Proximity < 0.1) { //TODO: придумать расстояние, при котором считается еще, что перемычка над проемом
                return refWithContext.GetReference().ElementId;
            }
            return ElementId.InvalidElementId;
        }

        private void LockLintel(FamilyInstance lintel, FamilyInstance elementInWall) {
            
            var LeftRightElement = elementInWall.GetReferences(FamilyInstanceReferenceType.CenterLeftRight);
            var LeftRightLintel = lintel.GetReferences(FamilyInstanceReferenceType.CenterLeftRight);
            _document.Create.NewAlignment(_document.ActiveView, LeftRightLintel.First(), LeftRightElement.First());

            var topElement = elementInWall.GetReferences(FamilyInstanceReferenceType.Top);
            var bottomLintel = lintel.GetReferences(FamilyInstanceReferenceType.CenterElevation);

            var facade = new FilteredElementCollector(_document)
                .OfClass(typeof(View))
                .Cast<View>()
                .First(v => v.ViewType == ViewType.Elevation);
            _document.Create.NewAlignment(facade, topElement.First(), bottomLintel.First());


            //TODO: поискать другой способ
            var elementInWallVerticalFaces = GetOrientedFaces(elementInWall.Host, Orientation.Horizontal);
            var maxAreaFaces = elementInWallVerticalFaces
                .Where(f => Math.Abs(f.Area - elementInWallVerticalFaces.Max(e => e.Area)) < 0.001).ToList();
            var leftL = lintel.GetReferences(FamilyInstanceReferenceType.Front);
            var rightL = lintel.GetReferences(FamilyInstanceReferenceType.Back);
            var verticalLintelPlanes = new List<Reference> { leftL.First(), rightL.First() };

            var plan = new FilteredElementCollector(_document)
               .OfClass(typeof(View))
               .Cast<View>()
               .First(v => v.ViewType == ViewType.FloorPlan);
            var isAlign = false;


            foreach(var reference in verticalLintelPlanes) {
                foreach(var face in maxAreaFaces) {
                    try {
                        _document.Create.NewAlignment(plan, reference, face.Reference);
                        isAlign = true;
                        break;
                    } catch {

                    }
                }
                if(isAlign)
                    break;
            }

        }

        private bool RotateLintel(FamilyInstance lintel, FamilyInstance elementInWall, XYZ center) {
            
                var line = Line.CreateBound(center, new XYZ(center.X, center.Y, center.Z + 1));
                ElementTransformUtils.RotateElement(_document, lintel.Id, line, GetAngle(elementInWall));
                

            return true;
        }

        private double GetAngle(FamilyInstance elementInWall) {
            var transform = elementInWall.GetTransform();
            var vectorX = transform.OfVector(transform.BasisX);
            return Math.PI + transform.BasisX.AngleOnPlaneTo(vectorX, transform.BasisZ);
        }

        private XYZ GetLocationPoint(FamilyInstance elementInWall) {
            var topBarHeight = (double) elementInWall.GetParamValueOrDefault(BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM); //TODO: возможно, не всегда этот параметр
            //var bottomBarHeight = elementInWall.get_Parameter(BuiltInParameter.INSTANCE_SILL_HEIGHT_PARAM);
            var levelHeight = ((Level) _document.GetElement(elementInWall.LevelId)).Elevation;
            var location = ((LocationPoint) elementInWall.Location).Point;
            var z = location.Z + topBarHeight; //+ bottomBarHeight.AsDouble(); //TODO: параметр!!!
            return new XYZ(location.X, location.Y, z);
        }

        private XYZ GetViewStartPoint(FamilyInstance lintel, bool plusDirection) //изменить название метода
        {
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

            var orientedFaces = new List<Face>();
            var option = new Options();
            option.ComputeReferences = true;
            foreach(GeometryObject geometryObject in element.get_Geometry(option)) {
                if(geometryObject is Solid solid) {
                    var horizontalFacesFromSolid = GetOrientedFacesFromSolid(solid, orientation);
                    if(horizontalFacesFromSolid != null)
                        orientedFaces.AddRange(horizontalFacesFromSolid);
                } else {
                    var geometryInstace = geometryObject as GeometryInstance;
                    if(geometryInstace == null)
                        continue;

                    foreach(var instObj in geometryInstace.GetInstanceGeometry()) {
                        Solid solidFromGeomInst = instObj as Solid;
                        if(solidFromGeomInst == null)
                            continue;
                        var horizontalFacesFromSolid = GetOrientedFacesFromSolid(solidFromGeomInst, orientation);
                        if(horizontalFacesFromSolid != null)
                            orientedFaces.AddRange(horizontalFacesFromSolid);
                    }
                }
            }
            return orientedFaces;
        }

        public IEnumerable<Element> GetElementTest() {


            //var element = _document.GetElement(new ElementId(7855887)) as FamilyInstance;
            //if(!(element.Host == null || !(element.Host is Wall wall1))) {
            //    var materials = GetElements(wall1.GetMaterialIds(false)); //TODO: может быть и true, проверить
            //    foreach(var m in materials) {
            //        if("Кладка".Equals(((Material) m).MaterialClass, StringComparison.InvariantCultureIgnoreCase)) {
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
                    if("Кладка".Equals(((Material) m).MaterialClass, StringComparison.InvariantCultureIgnoreCase)) {
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
