using System;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.RevitViewSettings {
    internal class FilterSetting : IView3DSetting {
        private readonly ParameterFilterElement _filter;
        private readonly OverrideGraphicSettings _graphicSettings;

        public FilterSetting(ParameterFilterElement filter, OverrideGraphicSettings graphicSettings) {
            _filter = filter ?? throw new ArgumentNullException(nameof(filter));
            _graphicSettings = graphicSettings ?? throw new ArgumentNullException(nameof(graphicSettings));
        }

        public void Apply(View3D view3D) {
            view3D.AddFilter(_filter.Id);
            view3D.SetFilterOverrides(_filter.Id, _graphicSettings);
            view3D.SetFilterVisibility(_filter.Id, true);
        }
    }
}
