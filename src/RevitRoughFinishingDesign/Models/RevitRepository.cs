using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.Revit.Geometry;

namespace RevitRoughFinishingDesign.Models {
    internal class RevitRepository {
        private readonly SpatialElementBoundaryOptions _spatialElementBoundaryOptions;
        private ICollection<ElementId> _wallsIds;
        private IList<GraphicsStyle> _lineStyles;
        private double _activeViewZPoint = -100000;
        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;
            _spatialElementBoundaryOptions = new SpatialElementBoundaryOptions() {
                SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish,
                StoreFreeBoundaryFaces = false
            };
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;
        public Application Application => UIApplication.Application;
        public View ActiveView => ActiveUIDocument.ActiveView;
        public Document Document => ActiveUIDocument.Document;

        public IList<GraphicsStyle> GetAllLineStyles() {
            _lineStyles = _lineStyles ?? new FilteredElementCollector(Document)
                .OfClass(typeof(GraphicsStyle))
                .ToElements()
                .OfType<GraphicsStyle>()
                .Where(gs => gs.GraphicsStyleCategory.CategoryType == CategoryType.Model &&
                             gs.GraphicsStyleCategory.Parent != null &&
                             gs.GraphicsStyleCategory.Parent.Id ==
                             Document.Settings.Categories.get_Item(BuiltInCategory.OST_Lines).Id &&
                             !gs.Id.ToString().StartsWith("-"))
                .ToList();
            return _lineStyles;
        }

        public ICollection<Element> GetRoomsOnActiveView() {
            return new FilteredElementCollector(Document, ActiveView.Id)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Rooms)
                .Where(el => el.LookupParameter("ФОП_Тип квартиры").AsString() != null)
                .ToList();
        }

        public ICollection<ElementId> GetWallsIds() {
            Options options = new Options() { DetailLevel = ViewDetailLevel.Fine };
            _wallsIds = _wallsIds ?? new FilteredElementCollector(Document, ActiveView.Id)
                .WhereElementIsNotElementType()
                .OfClass(typeof(Wall))
                .Where(el => el.GetSolids().First().Volume > 0)
                .Select(el => el.Id)
                .ToList();
            return _wallsIds;
        }

        /// <summary>
        /// Возвращает список замкнутых контуров, составляющих границы помещения
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public IList<IList<BoundarySegment>> GetBoundarySegments(Room room) {
            if(room is null) { throw new ArgumentNullException(nameof(room)); }
            return room.GetBoundarySegments(_spatialElementBoundaryOptions);
        }

        public double GetVerticalPointFromActiveView() {
            if(_activeViewZPoint != -100000) {
                return _activeViewZPoint;
            } else {
                ViewPlan viewPlan = ActiveView as ViewPlan;
                PlanViewRange viewRange = viewPlan.GetViewRange();
                double cutRange = viewRange.GetOffset(PlanViewPlane.CutPlane);
                double levelVerticalOffset = viewPlan.GenLevel.ProjectElevation;
                _activeViewZPoint = cutRange + levelVerticalOffset; // Z-координата секущей плоскости
                return _activeViewZPoint;
            }
        }

        public IList<Room> GetTestRooms() {
            return ActiveUIDocument
                .GetSelectedElements()
                .Where(el => el.Category.IsId(BuiltInCategory.OST_Rooms))
                .Cast<Room>()
                .ToList();
        }

        public Curve TransformCurveToExactZ(Curve curve, double exactZ) {
            // Получаем начало и конец кривой
            XYZ start = curve.GetEndPoint(0);
            XYZ end = curve.GetEndPoint(1);

            // Создаем новые точки с заданной координатой Z
            XYZ newStart = new XYZ(start.X, start.Y, exactZ);
            XYZ newEnd = new XYZ(end.X, end.Y, exactZ);

            // Создаем новую кривую на основе измененных точек
            return Line.CreateBound(newStart, newEnd);
        }

        public RoomBorder GetClosestCurveFromCurveList(Curve currentCurve, IList<Curve> curves) {
            XYZ centerOfCurrentCurve = currentCurve.Evaluate(0.5, true);
            double minDistance = double.PositiveInfinity;
            Curve closestCurve = null;
            foreach(Curve curve in curves) {
                XYZ closestPoint = curve.Project(centerOfCurrentCurve).XYZPoint;
                double distance = centerOfCurrentCurve.DistanceTo(closestPoint);
                if(distance < minDistance) {
                    minDistance = distance;
                    closestCurve = curve;
                }
            }
            return new RoomBorder(closestCurve);
        }

        public RoomBorder GetClosestCurveFromCurveList(Curve currentCurve, IList<CurveLoop> curveLoops) {
            IList<Curve> allCurves = curveLoops.SelectMany(loop => loop.ToList()).ToList();
            return GetClosestCurveFromCurveList(currentCurve, allCurves);
        }

        public XYZ GetOriginFromCurve(Curve curve) {
            if(curve is Arc arc) {
                return arc.Center; // Для дуги возвращаем центр
            }
            if(curve is Line line) {
                return line.Origin; // Для линии возвращаем Origin
            }
            throw new ArgumentException("Не поддерживаемый тип кривой", nameof(curve));
        }

        /// <summary>
        /// Функция для создания тестовой линии модели в ревите на основе линии класса Line
        /// </summary>
        /// <param name="geomLine"></param>
        public void CreateTestModelLine(Line geomLine) {
            XYZ dir = geomLine.Direction.Normalize();
            double x = dir.X;
            double y = dir.Y;
            double z = dir.Z;

            XYZ origin = geomLine.Origin;
            XYZ normal = new XYZ(z - y, x - z, y - x);
            Plane plane = Plane.CreateByNormalAndOrigin(normal, origin);
            SketchPlane sketch = SketchPlane.Create(Document, plane);
            Document.Create.NewModelCurve(geomLine, sketch);
        }

        public void CreateDirectShape(Solid solid) {

            DirectShape directShape = DirectShape.CreateElement(
                Document, new ElementId(BuiltInCategory.OST_GenericModel));
            directShape.SetShape(new List<GeometryObject> { solid });
        }


        public double ConvertToFeetFromMillimeters(double value) {
#if REVIT_2020_OR_LESS
            return UnitUtils.ConvertToInternalUnits(value, DisplayUnitType.DUT_MILLIMETERS);
#else
            return UnitUtils.ConvertToInternalUnits(value, UnitTypeId.Millimeters);
#endif
        }


        public double ConvertToMillimetersFromFeet(double value) {
#if REVIT_2020_OR_LESS
            return UnitUtils.ConvertFromInternalUnits(value, DisplayUnitType.DUT_MILLIMETERS);
#else
            return UnitUtils.ConvertFromInternalUnits(value, UnitTypeId.Millimeters);
#endif
        }

        /// <summary>
        /// Возвращает 3D вид по умолчанию
        /// </summary>
        /// <returns></returns>
        private View3D GetDefaultView3D() {
            // хоть в ревите по умолчанию и присутствует "{3D}" вид, фигурные скобки запрещены в названиях
            const string defaultRevitView3dName = "{3D}";
            const string defaultView3dName = "3D";
            var views3D = new FilteredElementCollector(Document)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .ToArray();

            // ищем 3D вид ревита по умолчанию
            var view = views3D.FirstOrDefault(
                item => item.Name.Equals(defaultRevitView3dName, StringComparison.CurrentCultureIgnoreCase));
            if(view == null) {
                // ищем наш 3D вид по умолчанию
                view = views3D.FirstOrDefault(
                    item => item.Name.Equals(defaultView3dName, StringComparison.CurrentCultureIgnoreCase));
            }
            if(view == null) {
                // создаем наш 3D вид по умолчанию
                var type = new FilteredElementCollector(Document)
                    .OfClass(typeof(ViewFamilyType))
                    .Cast<ViewFamilyType>()
                    .First(v => v.ViewFamily == ViewFamily.ThreeDimensional);
                type.DefaultTemplateId = ElementId.InvalidElementId;
                view = View3D.CreateIsometric(Document, type.Id);
                view.Name = defaultView3dName;
            }
            return view;
        }

    }
}
