
using System;
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

        public IList<WallDesignData> GetWallDesignDatas(RevitSettings settings) {
            IList<Room> rooms = _revitRepository.GetSelectedRooms();
            IList<WallDesignData> wallDesignDatas = new List<WallDesignData>();

            // Сбор всех данных по стенам из всех комнат
            foreach(Room room in rooms) {
                try {

                    RevitRoomHandler roomHandler = new RevitRoomHandler(_revitRepository, room, _curveLoopsSimplifier);
                    ICollection<ElementId> wallsIdsInRoom = roomHandler.GetWallsFromRoom();

                    foreach(ElementId wallId in wallsIdsInRoom) {
                        WallDesignData wallDesignData = GetWallDesignData(roomHandler, settings, wallId);
                        if(wallDesignData != null) {
                            wallDesignDatas.Add(wallDesignData);
                        }
                    }
                } catch(Exception ex) {
                    string message = ex.Message;
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

        public ElementId GetLineStyleId(RevitSettings settings, ElementId wallTypeId) {
            List<PairModel> pairModels = settings?.PairModels;
            ElementId lineStyle = ElementId.InvalidElementId;
            foreach(PairModel pairModel in pairModels) {
                if(pairModel.WallTypeId == wallTypeId) {
                    lineStyle = pairModel.LineStyleId;
                    break;
                }
            }
            return lineStyle;
        }

        public ElementId GetLineStyleIdAutomated(string lineName) {
            IList<GraphicsStyle> allLineStyles = _revitRepository.GetAllLineStyles();
            foreach(GraphicsStyle lineStyle in allLineStyles) {
                if(lineStyle.Name == lineName)
                    return lineStyle.Id;
            }
            return ElementId.InvalidElementId;
        }

        public WallDesignData GetWallDesignData(
            RevitRoomHandler roomHandler,
            RevitSettings settings,
            ElementId wallId) {

            IList<CurveLoop> curveLoops = roomHandler.GetSimplifiedCurveLoops();
            IList<RoomBorder> roomBorders = roomHandler.GetRoomBorders();
            Wall wall = _revitRepository.Document.GetElement(wallId) as Wall;
            RevitWallHandler wallHandler = new RevitWallHandler(_revitRepository, roomHandler.RevitRoom, wall);
            ElementId lineStyleId = ElementId.InvalidElementId;
            if(settings.IsAutomated) {
                lineStyleId = GetLineStyleIdAutomated(wallHandler.GetLineName());
            } else {
                lineStyleId = GetLineStyleId(settings, wallHandler.GetWallTypeId());
            }
            Curve wallLine = wallHandler.GetWallLineOnActiveView();
            RoomBorder closestBorderOfRoom = _revitRepository.GetClosestCurveFromCurveList(wallLine, roomBorders);
            IList<Line> wallLinesForDraw = wallHandler.GetWallLinesForDraw();
            double distanceFromBorder = GetDistanceFromRoomBorder(closestBorderOfRoom.Curve, wallLine);
            XYZ directionToRoom = wallHandler.GetDirectionToRoom();

            WallDesignData wallDesignData = new WallDesignData() {
                WallId = wallId,
                RoomBorder = closestBorderOfRoom,
                WallLine = wallLine,
                LinesForDraw = wallLinesForDraw,
                DistanceFromBorder = distanceFromBorder,
                DirectionToRoom = directionToRoom,
                LineStyleId = lineStyleId
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
