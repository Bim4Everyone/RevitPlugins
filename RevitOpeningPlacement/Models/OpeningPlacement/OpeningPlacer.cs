﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Exceptions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement {
    internal class OpeningPlacer {
        private readonly RevitRepository _revitRepository;

        public OpeningPlacer(RevitRepository revitRepository) {
            _revitRepository = revitRepository;
        }
        public ClashModel Clash { get; set; }
        public IPointFinder PointFinder { get; set; }
        public IAngleFinder AngleFinder { get; set; }
        public IParametersGetter ParameterGetter { get; set; }
        public FamilySymbol Type { get; set; }

        public void Place() {
            var point = PointFinder.GetPoint();
            var level = _revitRepository.GetLevel(Clash.MainElement.Level);
            var opening = _revitRepository.CreateInstance(Type, point, level);

            var angle = AngleFinder.GetAngle();
            _revitRepository.RotateElement(opening, point, angle);

            SetParamValues(opening);
        }

        private void SetParamValues(FamilyInstance opening) {
            foreach(var paramValue in ParameterGetter.GetParamValues()) {
                paramValue.Value.SetParamValue(opening, paramValue.ParamName);
            }
        }
    }
}