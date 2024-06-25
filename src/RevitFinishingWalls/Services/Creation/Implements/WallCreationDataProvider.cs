using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using RevitFinishingWalls.Models;
using RevitFinishingWalls.Models.Enums;

namespace RevitFinishingWalls.Services.Creation.Implements {
    internal class WallCreationDataProvider : IWallCreationDataProvider {
        private readonly RevitRepository _revitRepository;

        public WallCreationDataProvider(RevitRepository revitRepository) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        }


        public IList<WallCreationData> GetWallCreationData(Room room, PluginConfig config) {
            if(room is null) { throw new ArgumentNullException(nameof(room)); }
            if(config is null) { throw new ArgumentNullException(nameof(config)); }

            List<WallCreationData> wallCreationData = new List<WallCreationData>();
            WallCreationData lastWallCreationData = null;
            double wallHeight = CalculateFinishingWallHeight(room, config);
            double wallBaseOffset = _revitRepository.ConvertMmToFeet(config.WallBaseOffsetMm);

            foreach(IList<BoundarySegment> loop in _revitRepository.GetBoundarySegments(room)) {
                IList<CurveSegmentElement> curveSegmentsElements
                    = _revitRepository.GetCurveSegmentsElements(loop, config.WallTypeId);
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
                            WallTypeId = config.WallTypeId,
                            BaseOffset = wallBaseOffset
                        };
                        lastWallCreationData.AddRangeElementsForJoin(curveSegmentElement.Elements);
                        wallCreationData.Add(lastWallCreationData);
                    }
                }
            }
            return wallCreationData;
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
                return _revitRepository.ConvertMmToFeet(config.WallElevationMm - config.WallBaseOffsetMm);
            } else {
                double roomTopElevation = _revitRepository.GetRoomTopElevation(room);
                double roomBaseOffset = _revitRepository.ConvertMmToFeet(config.WallBaseOffsetMm);
                return roomTopElevation - roomBaseOffset;
            }
        }
    }
}
