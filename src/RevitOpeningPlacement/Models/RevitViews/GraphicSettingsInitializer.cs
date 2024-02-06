using System.Linq;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.RevitViews.RevitViewSettings {
    internal class GraphicSettingsInitializer {
        /// <summary>
        /// Настройки графики для элементов заданий на отверстия
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static OverrideGraphicSettings GetOpeningGraphicSettings(Document doc) {
            // красный
            return GetGraphicSettings(doc, new Color(255, 0, 0));
        }

        /// <summary>
        /// Настройки графики для элементов инженерных систем
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public static OverrideGraphicSettings GetMepGraphicSettings(Document doc) {
            // зеленый
            return GetGraphicSettings(doc, new Color(0, 128, 0)).SetSurfaceTransparency(10);
        }

        /// <summary>
        /// Настройки графики для элементов конструкций - стен и перекрытий
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        internal static OverrideGraphicSettings GetConstructureGraphicSettings(Document doc) {
            // серый
            return GetGraphicSettings(doc, new Color(128, 128, 128));
        }

        /// <summary>
        /// Настройки графики для элементов конструкций - стен и перекрытий, которые не интересны в данный момент
        /// </summary>
        /// <returns></returns>
        internal static OverrideGraphicSettings GetNotInterestingConstructionsGraphicSettings() {
            return new OverrideGraphicSettings().SetHalftone(true).SetSurfaceTransparency(70);
        }

        /// <summary>
        /// Настройки графики для неважных элементов для работы с заданиями на отверстиями: потолки, двери, изоляция трубопроводов и т.п.
        /// </summary>
        /// <returns></returns>
        internal static OverrideGraphicSettings GetSecondaryElementsGraphicSettings() {
            return new OverrideGraphicSettings().SetHalftone(true).SetSurfaceTransparency(70);
        }

        private static OverrideGraphicSettings GetGraphicSettings(Document doc, Color color) {
            var settings = new OverrideGraphicSettings();

            settings
                .SetCutBackgroundPatternColor(color)
                .SetCutForegroundPatternColor(color)
                .SetSurfaceBackgroundPatternColor(color)
                .SetSurfaceForegroundPatternColor(color)
               //.SetCutLineColor(color)
               //.SetCutLineWeight(16)
               //.SetCutLinePatternId(LinePatternElement.GetSolidPatternId())
               ;

            var solidFillPattern = new FilteredElementCollector(doc)
                .OfClass(typeof(FillPatternElement))
                .OfType<FillPatternElement>()
                .FirstOrDefault(item => item.GetFillPattern().IsSolidFill);
            if(solidFillPattern != null) {
                settings
                    //.SetCutBackgroundPatternId(solidFillPattern.Id)
                    //.SetCutBackgroundPatternVisible(true)
                    //.SetCutForegroundPatternId(solidFillPattern.Id)
                    //.SetCutForegroundPatternVisible(true)
                    .SetSurfaceBackgroundPatternId(solidFillPattern.Id)
                    .SetSurfaceForegroundPatternId(solidFillPattern.Id)
                    ;
            }

            return settings;
        }
    }
}
