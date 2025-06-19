
using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitRoughFinishingDesign.Services;

namespace RevitRoughFinishingDesign.Models;
internal class WallDesignDataGetter {
    private readonly RevitRepository _revitRepository;
    private readonly ICurveLoopsSimplifier _curveLoopsSimplifier;

    public WallDesignDataGetter(RevitRepository revitRepository, ICurveLoopsSimplifier curveLoopsSimplifier) {
        _revitRepository = revitRepository;
        _curveLoopsSimplifier = curveLoopsSimplifier;
    }

    public IList<WallDesignData> GetWallDesignDatas(RevitSettings settings) {
        var rooms = _revitRepository.GetSelectedRooms();
        IList<WallDesignData> wallDesignDatas = [];

        // Сбор всех данных по стенам из всех комнат
        foreach(var room in rooms) {
            try {

                var roomHandler = new RevitRoomHandler(_revitRepository, room, _curveLoopsSimplifier);
                var wallsIdsInRoom = roomHandler.GetWallsFromRoom();

                foreach(var wallId in wallsIdsInRoom) {
                    var wallDesignData = GetWallDesignData(roomHandler, settings, wallId);
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

        IList<WallDesignData> resultDesignDatas = [];

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
        var pairModels = settings?.PairModels;
        var lineStyle = ElementId.InvalidElementId;
        foreach(var pairModel in pairModels) {
            if(pairModel.WallTypeId == wallTypeId) {
                lineStyle = pairModel.LineStyleId;
                break;
            }
        }
        return lineStyle;
    }

    public ElementId GetLineStyleIdAutomated(string lineName) {
        var allLineStyles = _revitRepository.GetAllLineStyles();
        foreach(var lineStyle in allLineStyles) {
            if(lineStyle.Name == lineName) {
                return lineStyle.Id;
            }
        }
        return ElementId.InvalidElementId;
    }

    public WallDesignData GetWallDesignData(
        RevitRoomHandler roomHandler,
        RevitSettings settings,
        ElementId wallId) {
        _ = roomHandler.GetSimplifiedCurveLoops();
        var roomBorders = roomHandler.GetRoomBorders();
        var wall = _revitRepository.Document.GetElement(wallId) as Wall;
        var wallHandler = new RevitWallHandler(_revitRepository, roomHandler.RevitRoom, wall);
        _ = ElementId.InvalidElementId;
        var lineStyleId = settings.IsAutomated
            ? GetLineStyleIdAutomated(wallHandler.GetLineName())
            : GetLineStyleId(settings, wallHandler.GetWallTypeId());
        Curve wallLine = wallHandler.GetWallLineOnActiveView();
        var closestBorderOfRoom = _revitRepository.GetClosestCurveFromCurveList(wallLine, roomBorders);
        var wallLinesForDraw = wallHandler.GetWallLinesForDraw();
        double distanceFromBorder = GetDistanceFromRoomBorder(closestBorderOfRoom.Curve, wallLine);
        var directionToRoom = wallHandler.GetDirectionToRoom();

        var wallDesignData = new WallDesignData() {
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
        var wallLineCenter = wallLine.Evaluate(0.5, true);
        var closestPoint = roomBorder.Project(wallLineCenter).XYZPoint;
        double distance = closestPoint.DistanceTo(wallLineCenter);
        return distance;
    }
}
