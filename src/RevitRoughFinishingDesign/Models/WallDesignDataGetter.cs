
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using RevitRoughFinishingDesign.Services;

namespace RevitRoughFinishingDesign.Models {
    internal class WallDesignDataGetter {
        private readonly RevitRepository _revitRepository;
        private readonly ICurveLoopsSimplifier _curveLoopsSimplifier;

        public WallDesignDataGetter(RevitRepository revitRepository, ICurveLoopsSimplifier curveLoopsSimplifier) {
            _revitRepository = revitRepository;
            _curveLoopsSimplifier = curveLoopsSimplifier;
        }

        //public IList<WallDesignData> GetWallDesignDatas() {
        //    IList<Room> rooms = _revitRepository.GetTestRooms();
        //    IList<WallDesignData> wallDesignDatas = new List<WallDesignData>();
        //    IList<WallDesignData> resultDesignDatas = new List<WallDesignData>();

        //    foreach(Room room in rooms) {
        //        RevitRoomHandler roomHandler = new RevitRoomHandler(_revitRepository, room, _curveLoopsSimplifier);
        //        ICollection<ElementId> wallsIdsInRoom = roomHandler.GetWallsFromRoom();
        //        foreach(ElementId wallId in wallsIdsInRoom) {
        //            WallDesignData wallDesignData = GetWallDesignData(roomHandler, wallId);
        //            wallDesignDatas.Add(wallDesignData);
        //        }
        //        //wallDesignDatas = wallDesignDatas
        //        //        .OrderBy(w => _revitRepository.GetOriginFromCurve(w.BorderOfRoom))
        //        //        .ToList();
        //        //var groupedWallDesignDatas = wallDesignDatas
        //        //    .GroupBy(w => _revitRepository.GetOriginFromCurve(w.BorderOfRoom));
        //        var groupedWallDesignDatas = wallDesignDatas
        //            .GroupBy(w => w.BorderOfRoom.Evaluate(0.5, true)) // Берем среднюю точку
        //            .OrderBy(g => g.Key.X)
        //            .ThenBy(g => g.Key.Y)
        //            .ThenBy(g => g.Key.Z);

        //        foreach(var group in groupedWallDesignDatas) {
        //            XYZ origin = group.Key; // Уникальная координата группы

        //            // Сортируем wallData по расстоянию
        //            IList<WallDesignData> sortedWallData = group
        //                .OrderBy(x => x.DistanceFromBorder) // Сортировка по возрастанию дистанции
        //                .ToList();

        //            // Назначаем LayerNumber на основе позиции в отсортированном списке
        //            for(int i = 0; i < sortedWallData.Count; i++) {
        //                sortedWallData[i].LayerNumber = i;
        //                resultDesignDatas.Add(sortedWallData[i]);
        //            }
        //        }
        //    }
        //    return resultDesignDatas;
        //}

        public IList<WallDesignData> GetWallDesignDatas() {
            IList<Room> rooms = _revitRepository.GetTestRooms();
            IList<WallDesignData> wallDesignDatas = new List<WallDesignData>();

            // Сбор всех данных по стенам из всех комнат
            foreach(Room room in rooms) {
                RevitRoomHandler roomHandler = new RevitRoomHandler(_revitRepository, room, _curveLoopsSimplifier);
                ICollection<ElementId> wallsIdsInRoom = roomHandler.GetWallsFromRoom();

                foreach(ElementId wallId in wallsIdsInRoom) {
                    WallDesignData wallDesignData = GetWallDesignData(roomHandler, wallId);
                    wallDesignDatas.Add(wallDesignData);
                }
            }

            // Теперь группируем по центру точки линий и сортируем 
            var groupedWallDesignDatas = wallDesignDatas
                .GroupBy(w => w.RoomBorder.Guid);

            IList<WallDesignData> resultDesignDatas = new List<WallDesignData>();

            foreach(var group in groupedWallDesignDatas) {
                // Сортируем внутри группы по расстоянию от границы
                IList<WallDesignData> sortedWallData = group
                    .OrderBy(x => x.DistanceFromBorder) // Сортировка внутри группы
                    .ToList();

                // Назначаем LayerNumber внутри группы
                for(int i = 0; i < sortedWallData.Count; i++) {
                    sortedWallData[i].LayerNumber = i;
                    resultDesignDatas.Add(sortedWallData[i]);
                }
            }

            return resultDesignDatas;
        }

        public WallDesignData GetWallDesignData(RevitRoomHandler roomHandler, ElementId wallId) {
            IList<CurveLoop> curveLoops = roomHandler.GetSimplifiedCurveLoops();
            Wall wall = _revitRepository.Document.GetElement(wallId) as Wall;
            RevitWallHandler wallHandler = new RevitWallHandler(_revitRepository, roomHandler.RevitRoom, wall);
            Curve wallLine = wallHandler.GetWallLine();
            RoomBorder closestBorderOfRoom = _revitRepository.GetClosestCurveFromCurveList(wallLine, curveLoops);
            IList<Line> wallLinesForDraw = wallHandler.GetWallLinesForDraw();
            double distanceFromBorder = GetDistanceFromRoomBorder(closestBorderOfRoom.Curve, wallLine);
            XYZ directionToRoom = wallHandler.GetDirectionToRoom();
            WallDesignData wallDesignData = new WallDesignData() {
                WallId = wallId,
                RoomBorder = closestBorderOfRoom,
                WallLine = wallLine,
                LinesForDraw = wallLinesForDraw,
                DistanceFromBorder = distanceFromBorder,
                DirectionToRoom = directionToRoom
            };

            return wallDesignData;
        }

        public double GetDistanceFromRoomBorder(Curve roomBorder, Curve wallLine) {
            XYZ wallLineCenter = wallLine.Evaluate(0.5, true);
            XYZ closestPoint = roomBorder.Project(wallLineCenter).XYZPoint;
            double distance = closestPoint.DistanceTo(wallLineCenter);
            return distance;
        }
    }
}
