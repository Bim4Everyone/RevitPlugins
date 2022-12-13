﻿using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class PerpendicularRectangleCurveFloorParamGetter : IParametersGetter {
        private readonly MepCurveClash<CeilingAndFloor> _clash;
        private readonly MepCategory _mepCategory;

        public PerpendicularRectangleCurveFloorParamGetter(MepCurveClash<CeilingAndFloor> clash, MepCategory mepCategory) {
            _clash = clash;
            _mepCategory = mepCategory;
        }

        public IEnumerable<ParameterValuePair> GetParamValues() {
            yield return new DoubleParameterGetter(RevitRepository.OpeningHeight, new HeightValueGetter(_clash.Element1, _mepCategory)).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningWidth, new WidthValueGetter(_clash.Element1, _mepCategory)).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningThickness, new FloorThicknessValueGetter(_clash)).GetParamValue();
        }
    }
}
