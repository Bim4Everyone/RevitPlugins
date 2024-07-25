using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using RevitFinishingWalls.Exceptions;
using RevitFinishingWalls.Models;
using RevitFinishingWalls.Models.Enums;

namespace RevitFinishingWalls.Services.Creation.Implements {
    internal class WallCreationDataProvider : IWallCreationDataProvider {
        private readonly RevitRepository _revitRepository;

        public WallCreationDataProvider(RevitRepository revitRepository) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        }


        /// <summary>
        /// Возвращает список объектов с данными для создания отделочных стен
        /// </summary>
        /// <param name="room">Помещение для обработки</param>
        /// <param name="settings">Настройки создания отделочных стен</param>
        /// <exception cref="ArgumentNullException">Исключение, если один из обязательных параметров null</exception>
        /// <exception cref="CannotCreateWallException">Исключение, 
        /// если не удалось получить данные для построения отделочных стен в помещении</exception>
        public IList<WallCreationData> GetWallCreationData(Room room, RevitSettings settings) {
            if(room is null) { throw new ArgumentNullException(nameof(room)); }
            if(settings is null) { throw new ArgumentNullException(nameof(settings)); }

            List<WallCreationData> wallCreationData = new List<WallCreationData>();
            WallCreationData lastWallCreationData = null;
            double wallHeight = CalculateFinishingWallHeight(room, settings);
            double wallBaseOffset = _revitRepository.ConvertMmToFeet(settings.WallBaseOffsetMm);
            double wallSideOffset = _revitRepository.ConvertMmToFeet(settings.WallSideOffsetMm);

            foreach(IList<BoundarySegment> loop in _revitRepository.GetBoundarySegments(room)) {
                try {
                    IList<CurveSegmentElement> segments
                        = _revitRepository.GetCurveSegmentsElements(loop, settings.WallTypeId, -wallSideOffset);
                    IList<CurveSegmentElement> curveSegmentsElements = ReorderFromCorner(segments);
                    for(int i = 0; i < curveSegmentsElements.Count; i++) {
                        CurveSegmentElement curveSegmentElement = curveSegmentsElements[i];
                        if((lastWallCreationData != null)
                            && _revitRepository.IsContinuation(lastWallCreationData.Curve, curveSegmentElement.Curve)) {

                            lastWallCreationData.Curve
                                = _revitRepository.CombineCurves(lastWallCreationData.Curve, curveSegmentElement.Curve);
                            lastWallCreationData.AddRangeElementsForJoin(curveSegmentElement.Elements);
                        } else {
                            lastWallCreationData = new WallCreationData(_revitRepository.Document) {
                                Curve = curveSegmentElement.Curve,
                                LevelId = room.LevelId,
                                Height = wallHeight,
                                WallTypeId = settings.WallTypeId,
                                BaseOffset = wallBaseOffset
                            };
                            lastWallCreationData.AddRangeElementsForJoin(curveSegmentElement.Elements);
                            wallCreationData.Add(lastWallCreationData);
                        }
                    }
                } catch(InvalidOperationException e) {
                    throw new CannotCreateWallException(e.Message);
                }
            }
            return wallCreationData;
        }


        /// <summary>
        /// Возвращает список объектов линий замкнутого контура границы помещения. 
        /// Список линий начинается с угла (или сопряжения непрямых линий, например - окружностей). 
        /// Порядок линий соответствует порядку исходного замкнутого контура.
        /// </summary>
        /// <param name="roomBorderLoop">Замкнутый контур границы помещения</param>
        private IList<CurveSegmentElement> ReorderFromCorner(IList<CurveSegmentElement> roomBorderLoop) {
            int startIndex = 0;
            for(int previous = 0, current = 1; current < roomBorderLoop.Count; current++, previous++) {
                // находим первый индекс линии, которая образует угол
                if(!_revitRepository.IsContinuation(roomBorderLoop[previous].Curve, roomBorderLoop[current].Curve)) {
                    startIndex = current;
                    break;
                }
            }
            var result = new List<CurveSegmentElement>();
            for(int i = startIndex; i < roomBorderLoop.Count; i++) {
                result.Add(roomBorderLoop[i]);
            }
            for(int i = 0; i < startIndex; i++) {
                result.Add(roomBorderLoop[i]);
            }
            return result;
        }

        /// <summary>
        /// Вычисляет высоту стены, чтобы ее верхняя отметка от уровня была в соответствии с настройками
        /// </summary>
        /// <param name="room">Помещение</param>
        /// <param name="settings">Настройки расстановки отделочных стен</param>
        /// <exception cref="ArgumentNullException"></exception>
        private double CalculateFinishingWallHeight(Room room, RevitSettings settings) {
            if(room is null) { throw new ArgumentNullException(nameof(room)); }
            if(settings is null) { throw new ArgumentNullException(nameof(settings)); }

            if(settings.WallElevationMode == WallElevationMode.ManualHeight) {
                return _revitRepository.ConvertMmToFeet(settings.WallElevationMm - settings.WallBaseOffsetMm);
            } else {
                double roomTopElevation = _revitRepository.GetRoomTopElevation(room);
                double roomBaseOffset = _revitRepository.ConvertMmToFeet(settings.WallBaseOffsetMm);
                return roomTopElevation - roomBaseOffset;
            }
        }
    }
}
