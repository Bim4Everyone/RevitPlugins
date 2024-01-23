using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using dosymep.Revit;

using RevitFinishingWalls.Exceptions;
using RevitFinishingWalls.Models.Enums;
using RevitFinishingWalls.Services.Selection;

namespace RevitFinishingWalls.Models {
    internal class RevitRepository {
        private readonly SpatialElementBoundaryOptions _spatialElementBoundaryOptions;
        private readonly View3D _defaultView3D;
        private readonly Dictionary<ElementId, double> _wallTypesWidthById;


        public RevitRepository(UIApplication uiApplication) {
            UIApplication = uiApplication;

            _spatialElementBoundaryOptions = new SpatialElementBoundaryOptions() {
                SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Finish,
                StoreFreeBoundaryFaces = false
            };
            _defaultView3D = GetDefaultView3D();
            _wallTypesWidthById = new Dictionary<ElementId, double>();
        }

        public UIApplication UIApplication { get; }
        public UIDocument ActiveUIDocument => UIApplication.ActiveUIDocument;

        public Application Application => UIApplication.Application;
        public Document Document => ActiveUIDocument.Document;


        public ICollection<WallType> GetWallTypes() {
            return new FilteredElementCollector(Document)
                .WhereElementIsElementType()
                .OfClass(typeof(WallType))
                .Cast<WallType>()
                .Where(wallType => wallType.Kind == WallKind.Basic)
                .ToArray();
        }

        public WallType GetWallType(ElementId wallTypeId) {
            return GetWallTypes().FirstOrDefault(wt => wt.Id == wallTypeId);
        }

        public ICollection<Room> GetSelectedRooms() {
            return ActiveUIDocument.Selection
                .GetElementIds()
                .Select(id => Document.GetElement(id))
                .Where(element => element is Room room && room.Area > 0)
                .Cast<Room>()
                .ToArray();
        }

        public ICollection<Room> PickRooms() {
            ISelectionFilter filter = new SelectionFilterRooms(Document);
            IList<Reference> references = ActiveUIDocument.Selection.PickObjects(
                ObjectType.Element,
                filter,
                "Выберите помещения");

            List<Room> rooms = new List<Room>();
            foreach(var reference in references) {
                if((reference != null) && (Document.GetElement(reference) is Room room) && (room.Area > 0)) {
                    rooms.Add(room);
                }
            }
            return rooms;
        }

        public ICollection<Room> GetRoomsOnActiveView() {
            var activeView = Document.ActiveView;
            try {
                return new FilteredElementCollector(Document, Document.ActiveView.Id)
                    .WhereElementIsNotElementType()
                    .WherePasses(new RoomFilter())
                    .OfClass(typeof(Room))
                    .Where(element => element is Room room && room.Area > 0)
                    .Cast<Room>()
                    .ToArray();
            } catch(Autodesk.Revit.Exceptions.ArgumentException) {
                return Array.Empty<Room>();
            }
        }

        public ICollection<Room> GetRooms(RoomGetterMode roomGetterMode) {
            switch(roomGetterMode) {
                case RoomGetterMode.AlreadySelectedRooms: {
                    return GetSelectedRooms();
                }
                case RoomGetterMode.RoomsOnActiveView: {
                    return GetRoomsOnActiveView();
                }
                case RoomGetterMode.ManuallySelectedRooms: {
                    return PickRooms();
                }
                default: {
                    return Array.Empty<Room>();
                }
            }
        }

        /// <summary>
        /// Возвращает коллекцию данных для построения стен в помещении 
        /// в соответствии с заданными настройками расстановки стен
        /// </summary>
        /// <param name="room">Помещение, в котором будут создаваться отделочные стены</param>
        /// <param name="config"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public IList<WallCreationData> GetWallCreationData(Room room, PluginConfig config) {

            if(room is null) { throw new ArgumentNullException(nameof(room)); }
            if(config is null) { throw new ArgumentNullException(nameof(config)); }

            List<WallCreationData> wallCreationData = new List<WallCreationData>();
            WallCreationData lastWallCreationData = null;
            double wallHeight = CalculateFinishingWallHeight(room, config);
            double wallBaseOffset = ConvertMmToFeet(config.WallBaseOffset);

            foreach(IList<BoundarySegment> loop in GetBoundarySegments(room)) {
                IList<CurveSegmentElement> curveSegmentsElements = GetCurveSegmentsElements(loop, config.WallTypeId);
                for(int i = 0; i < curveSegmentsElements.Count; i++) {
                    CurveSegmentElement curveSegmentElement = curveSegmentsElements[i];
                    if((lastWallCreationData != null)
                        && IsContinuation(lastWallCreationData.Curve, curveSegmentElement.Curve)) {

                        lastWallCreationData.Curve = CombineCurves(lastWallCreationData.Curve, curveSegmentElement.Curve);
                        lastWallCreationData.ElementsForJoin.Add(curveSegmentElement.Element);
                    } else {
                        lastWallCreationData = new WallCreationData(Document) {
                            Curve = curveSegmentElement.Curve,
                            LevelId = room.LevelId,
                            Height = wallHeight,
                            WallTypeId = config.WallTypeId,
                            BaseOffset = wallBaseOffset
                        };
                        lastWallCreationData.ElementsForJoin.Add(curveSegmentElement.Element);
                        wallCreationData.Add(lastWallCreationData);
                    }
                }
            }
            return wallCreationData;
        }

        /// <summary>
        /// Создает стену с линией привязки "Чистовая поверхность: Наружная", с отключенными границами помещения
        /// </summary>
        /// <param name="wallCreationData"></param>
        /// <param name="notJoinedElements"></param>
        /// <returns></returns>
        /// <exception cref="CannotCreateWallException"></exception>
        public Wall CreateWall(WallCreationData wallCreationData, out ICollection<ElementId> notJoinedElements) {
            Wall wall;
            try {
                wall = Wall.Create(
                    wallCreationData.Document,
                    wallCreationData.Curve,
                    wallCreationData.WallTypeId,
                    wallCreationData.LevelId,
                    wallCreationData.Height,
                    wallCreationData.BaseOffset,
                    false,
                    false);
            } catch(Autodesk.Revit.Exceptions.ArgumentOutOfRangeException) {
                throw new CannotCreateWallException($"Нельзя создать стену по заданной линии");
            } catch(Autodesk.Revit.Exceptions.ArgumentException) {
                throw new CannotCreateWallException($"Нельзя создать стену с заданной высотой");
            }
            //параметр "Location Line" или "Линия привязки"
            wall.SetParamValue(BuiltInParameter.WALL_KEY_REF_PARAM, (int) WallLocationLine.FinishFaceInterior);
            //параметр "Room Bounding" или "Граница помещения"
            wall.SetParamValue(BuiltInParameter.WALL_ATTR_ROOM_BOUNDING, 0);

            notJoinedElements = new List<ElementId>();
            foreach(Element item in wallCreationData.ElementsForJoin) {
                try {
                    JoinGeometryUtils.JoinGeometry(Document, wall, item);
                } catch(Autodesk.Revit.Exceptions.ArgumentException) {
                    notJoinedElements.Add(item.Id);
                }
            }
            return wall;
        }


        /// <summary>
        /// Возвращает толщину стены по Id типа стены из активного документа
        /// </summary>
        /// <param name="wallTypeId"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        private double GetWallTypeWidth(ElementId wallTypeId) {
            if(_wallTypesWidthById.TryGetValue(wallTypeId, out var width)) {
                return width;
            } else {
                var wallType = Document.GetElement(wallTypeId) as WallType;
                if(wallType != null) {
                    double wallTypeWidth = wallType.Width;
                    _wallTypesWidthById.Add(wallTypeId, wallTypeWidth);
                    return _wallTypesWidthById[wallTypeId];
                } else {
                    throw new ArgumentException(nameof(wallTypeId));
                }
            }
        }

        /// <summary>
        /// Определяет пары сегмент границы помещения-элемент, который образует этот сегмент.
        /// </summary>
        /// <param name="segmentsLoop">Исходная коллекция сегментов границ помещения</param>
        /// <param name="finishingWallTypeId">Id типа отделочной стены</param>
        /// <returns>Коллекция классов, в которых содержатся линия границы помещения, 
        /// смещенная вверх на заданное расстояние и элемент, который образует эту границу</returns>
        private IList<CurveSegmentElement> GetCurveSegmentsElements(
            IList<BoundarySegment> segmentsLoop,
            ElementId finishingWallTypeId) {

            List<CurveSegmentElement> curveSegmentsElements = new List<CurveSegmentElement>();

            for(int i = 0; i < segmentsLoop.Count; i++) {
                BoundarySegment segment = segmentsLoop[i];
                Element segmentElement = GetBoundaryElement(segment);
                if(segmentElement != null) {
                    // получение лини оси отделочной стены,
                    // смещенной влево на 1/2 толщины отделочной стены относительно исходной границы помещения
                    double finishingWallTypeHalfWidth = GetWallTypeWidth(finishingWallTypeId) / 2;
                    Curve curveWithOffset = segment.GetCurve().CreateOffset(-finishingWallTypeHalfWidth, XYZ.BasisZ);
                    curveSegmentsElements.Add(new CurveSegmentElement(segmentElement, curveWithOffset));
                }
            }
            return curveSegmentsElements;
        }

        private IList<IList<BoundarySegment>> GetBoundarySegments(Room room) {
            if(room is null) { throw new ArgumentNullException(nameof(room)); }

            return room.GetBoundarySegments(_spatialElementBoundaryOptions);
        }

        /// <summary>
        /// Конвертирует миллиметры в футы (единицы длины ревита)
        /// </summary>
        /// <param name="mm"></param>
        /// <returns></returns>
        private double ConvertMmToFeet(double mm) {
#if REVIT_2021_OR_GREATER
            return UnitUtils.ConvertToInternalUnits(mm, UnitTypeId.Millimeters);
#else
            return UnitUtils.ConvertToInternalUnits(mm, DisplayUnitType.DUT_MILLIMETERS);
#endif
        }

        /// <summary>
        /// Вычисляет высоту стены, чтобы ее верхняя отметка от уровня была в соответствии с настройками
        /// </summary>
        /// <param name="room"></param>
        /// <param name="config"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private double CalculateFinishingWallHeight(Room room, PluginConfig config) {
            if(room is null) { throw new ArgumentNullException(nameof(room)); }
            if(config is null) { throw new ArgumentNullException(nameof(config)); }

            if(config.WallElevationMode == WallElevationMode.ManualHeight) {
                return ConvertMmToFeet(config.WallElevation - config.WallBaseOffset);
            } else {
                return GetRoomTopElevation(room) - ConvertMmToFeet(config.WallBaseOffset);
            }
        }

        /// <summary>
        /// Возвращает отметку верха стены помещения с учетом верхнего ограничивающего элемента в единицах ревита
        /// </summary>
        /// <param name="room"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private double GetRoomTopElevation(Room room) {
            if(room is null) { throw new ArgumentNullException(nameof(room)); }

            double roomVolume = room.GetParamValue<double>(BuiltInParameter.ROOM_VOLUME);
            if(roomVolume > 0) {
                double roomHeight = roomVolume / room.GetParamValue<double>(BuiltInParameter.ROOM_AREA);
                return roomHeight + room.GetParamValue<double>(BuiltInParameter.ROOM_LOWER_OFFSET);
            } else {
                return room.GetParamValue<double>(BuiltInParameter.ROOM_UPPER_OFFSET);
            }
        }

        /// <summary>
        /// Проверяет, является ли второй отрезок продолжением первого.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="second"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private bool IsContinuation(Curve first, Curve second) {
            if(first is null) { throw new ArgumentNullException(nameof(first)); }
            if(second is null) { throw new ArgumentNullException(nameof(second)); }

            if((first is Line firstLine) && (second is Line secondLine)) {
                return firstLine.Direction.IsAlmostEqualTo(secondLine.Direction)
                    && (firstLine.GetEndPoint(0).IsAlmostEqualTo(secondLine.GetEndPoint(1))
                    || firstLine.GetEndPoint(1).IsAlmostEqualTo(secondLine.GetEndPoint(0)));
            } else {
                return false;
            }
        }

        private Curve CombineCurves(Curve curveFirst, Curve curveSecond) {
            if(curveFirst is null) { throw new ArgumentNullException(nameof(curveFirst)); }
            if(curveSecond is null) { throw new ArgumentNullException(nameof(curveSecond)); }

            return Line.CreateBound(curveFirst.GetEndPoint(0), curveSecond.GetEndPoint(1));
        }

        /// <summary>
        /// Возвращает существующий 3D вид по умолчанию
        /// </summary>
        /// <returns></returns>
        private View3D GetDefaultView3D() {
            string defaultViewName = "{3D}";
            var view = new FilteredElementCollector(Document)
                .OfClass(typeof(View3D))
                .Cast<View3D>()
                .FirstOrDefault(item => item.Name.Equals(defaultViewName, StringComparison.CurrentCultureIgnoreCase));
            if(view == null) {
                using(Transaction t = Document.StartTransaction("BIM: Создание 3D-вида")) {
                    var type = new FilteredElementCollector(Document)
                        .OfClass(typeof(ViewFamilyType))
                        .Cast<ViewFamilyType>()
                        .First(v => v.ViewFamily == ViewFamily.ThreeDimensional);
                    type.DefaultTemplateId = ElementId.InvalidElementId;
                    view = View3D.CreateIsometric(Document, type.Id);
                    view.Name = defaultViewName;
                    t.Commit();
                }
            }
            return view;
        }

        /// <summary>
        /// Возвращает элемент, который определяет сегмент границы помещения
        /// </summary>
        /// <param name="boundarySegment"></param>
        /// <returns></returns>
        private Element GetBoundaryElement(BoundarySegment boundarySegment) {
            Element borderEl = Document.GetElement(boundarySegment.ElementId);
            Element element;
            if(borderEl is ModelLine) {
                var categoryFilter = new ElementMulticategoryFilter(
                    new BuiltInCategory[] {
                        BuiltInCategory.OST_Walls,
                        BuiltInCategory.OST_StructuralColumns,
                        BuiltInCategory.OST_Columns });
                element = GetElementByRay(_defaultView3D, boundarySegment.GetCurve(), categoryFilter);
            } else {
                element = borderEl;
            }
            return element;
        }

        /// <summary>
        /// Возвращает элемент модели из активного документа Revit, который определяет заданную линию границы помещения,
        /// предполагая, что помещение расположено слева от линии границы помещения, если смотреть из начала линии в конец.
        /// </summary>
        /// <param name="view3D"></param>
        /// <param name="curve"></param>
        /// <param name="elementFilter"></param>
        /// <returns></returns>
        //https://thebuildingcoder.typepad.com/blog/2013/10/determining-a-room-boundary-segment-generating-element.html
        private Element GetElementByRay(View3D view3D, Curve curve, ElementFilter elementFilter) {
            Element boundaryElement = null;

            const double minTolerance = 0.00000001;
            const double maxTolerance = 0.01;
            const double stepInRoom = 0.1;

            XYZ lineDirection = (curve.GetEndPoint(1) - curve.GetEndPoint(0)).Normalize();

            XYZ toRoomVector = stepInRoom * GetLeftDirection(lineDirection);
            XYZ pointBottomInRoom = curve.Evaluate(0.5, true) + toRoomVector;
            XYZ startPoint = pointBottomInRoom + XYZ.BasisZ;

            ReferenceIntersector intersector
                = new ReferenceIntersector(elementFilter, FindReferenceTarget.Element, view3D) {
                    FindReferencesInRevitLinks = false
                };
            XYZ toElementDirection = GetRightDirection(lineDirection);

            ReferenceWithContext context = intersector.FindNearest(startPoint, toElementDirection);
            Reference closestReference;
            if(context != null) {
                if((context.Proximity > minTolerance) && (context.Proximity < (maxTolerance + stepInRoom))) {
                    closestReference = context.GetReference();
                    if(closestReference != null) {
                        boundaryElement = Document.GetElement(closestReference);
                    }
                }
            }

            return boundaryElement;
        }

        /// <summary>
        /// Возвращает вектор, повернутый на 90 градусов влево (против часовой стрелки) относительно заданного вектора
        /// </summary>
        private XYZ GetLeftDirection(XYZ direction) {
            double x = -direction.Y;
            double y = direction.X;
            double z = direction.Z;
            return new XYZ(x, y, z);
        }

        /// <summary>
        /// Возвращает вектор, повернутый на 90 градусов вправо (по часовой стрелке) относительно заданного вектора
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        private XYZ GetRightDirection(XYZ direction) {
            return GetLeftDirection(direction.Negate());
        }
    }
}
