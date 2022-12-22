﻿using System;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RevitViews.RevitViewSettings {
    internal class ParamSetting : IView3DSetting {
        private readonly string _paramName;
        private readonly ParamValue _paramValue;

        public ParamSetting(string paramName, ParamValue paramValue) {
            if(string.IsNullOrEmpty(paramName)) {
                throw new ArgumentException($"'{nameof(paramName)}' cannot be null or empty.", nameof(paramName));
            }

            _paramName = paramName;
            _paramValue = paramValue ?? throw new ArgumentNullException(nameof(paramValue));
        }

        public void Apply(View3D view3D) {
            _paramValue.SetParamValue(view3D, _paramName);
        }
    }
}
