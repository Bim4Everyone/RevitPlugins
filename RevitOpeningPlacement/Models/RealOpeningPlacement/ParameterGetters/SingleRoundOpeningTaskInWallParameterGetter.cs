﻿using System;
using System.Collections.Generic;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement;
using RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters;
using RevitOpeningPlacement.Models.RealOpeningPlacement.ValueGetters;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Models.RealOpeningPlacement.ParameterGetters {
    /// <summary>
    /// Класс, предоставляющий параметры для чистового круглого отверстия из параметров входящего задания на круглое отверстие в стене
    /// </summary>
    internal class SingleRoundOpeningTaskInWallParameterGetter : IParametersGetter {
        private readonly OpeningMepTaskIncoming _openingMepTaskIncoming;
        private readonly IPointFinder _pointFinder;


        /// <summary>
        /// Класс, предоставляющий параметры для чистового круглого отверстия из параметров входящего задания на круглое отверстие в стене
        /// </summary>
        /// <param name="incomingTask">Входящее задание на отверстие</param>
        /// <param name="pointFinder">Провайдер точки вставки чистового отверстия</param>
        public SingleRoundOpeningTaskInWallParameterGetter(OpeningMepTaskIncoming incomingTask, IPointFinder pointFinder) {
            if(incomingTask is null) { throw new ArgumentNullException(nameof(incomingTask)); }
            if(pointFinder is null) { throw new ArgumentNullException(nameof(pointFinder)); }

            _openingMepTaskIncoming = incomingTask;
            _pointFinder = pointFinder;
        }


        public IEnumerable<ParameterValuePair> GetParamValues() {
            // габариты отверстия
            yield return new DoubleParameterGetter(RealOpeningPlacer.RealOpeningDiameter, new RoundOpeningTaskInWallDiameterValueGetter(_openingMepTaskIncoming, _pointFinder)).GetParamValue();
        }
    }
}