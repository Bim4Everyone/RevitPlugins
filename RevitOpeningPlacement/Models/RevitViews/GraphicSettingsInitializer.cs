using System.Linq;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.RevitViews.RevitViewSettings {
    internal class GraphicSettingsInitializer {
        public static OverrideGraphicSettings GetOpeningGraphicSettings(Document doc) {
            // красный
            return GetGraphicSettings(doc, new Color(255, 0, 0));
        }

        public static OverrideGraphicSettings GetMepGraphicSettings(Document doc) {
            // зеленый
            return GetGraphicSettings(doc, new Color(0, 128, 0));
        }

        internal static OverrideGraphicSettings GetConstructureGraphicSettings(Document doc) {
            // серый
            return GetGraphicSettings(doc, new Color(128, 128, 128));
        }

        private static OverrideGraphicSettings GetGraphicSettings(Document doc, Color color) {
            var settings = new OverrideGraphicSettings();

            settings.SetCutBackgroundPatternColor(color)
               .SetCutForegroundPatternColor(color)
               .SetSurfaceBackgroundPatternColor(color)
               .SetSurfaceForegroundPatternColor(color)
               .SetCutLineColor(color)
               .SetCutLineWeight(16)
               .SetCutLinePatternId(LinePatternElement.GetSolidPatternId());

            var solidFillPattern = new FilteredElementCollector(doc)
                .OfClass(typeof(FillPatternElement))
                .OfType<FillPatternElement>()
                .FirstOrDefault(item => item.GetFillPattern().IsSolidFill);
            if(solidFillPattern != null) {
                settings.SetCutBackgroundPatternId(solidFillPattern.Id)
                    .SetCutBackgroundPatternVisible(true)
                    .SetCutForegroundPatternId(solidFillPattern.Id)
                    .SetCutForegroundPatternVisible(true)
                    .SetSurfaceBackgroundPatternId(solidFillPattern.Id)
                    .SetSurfaceForegroundPatternId(solidFillPattern.Id);
            }

            return settings;
        }
    }
}
