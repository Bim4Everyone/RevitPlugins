using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectParams;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.RevitViews.RevitViewSettings;

namespace RevitOpeningPlacement.Models.RevitViews {
    internal class ViewSettingsInitializer {
        public static List<IView3DSetting> GetView3DSettings(Document doc) {
            return new List<IView3DSetting>() {
                GetDetailLevelSetting(),
                GetDisplayStyleSetting(),
                GetHiddenCategoriesSetting(),
                GetOpeningFilterSetting(doc),
                GetMepFilterSetting(doc),
                GetViewDisplayModelSetting(),
                GetParamSetting(doc)
            }.Where(item => item != null)
            .ToList();
        }

        private static DetailLevelSetting GetDetailLevelSetting() {
            return new DetailLevelSetting(ViewDetailLevel.Fine);
        }

        private static DisplayStyleSetting GetDisplayStyleSetting() {
            return new DisplayStyleSetting(DisplayStyle.HLR);
        }

        private static HiddenCategoriesSetting GetHiddenCategoriesSetting() {
            return new HiddenCategoriesSetting(new BuiltInCategory[] {
                BuiltInCategory.OST_Levels,
                BuiltInCategory.OST_WallRefPlanes,
                BuiltInCategory.OST_Grids,
                BuiltInCategory.OST_VolumeOfInterest
            });
        }

        private static FilterSetting GetOpeningFilterSetting(Document doc) {
            var filter = ParameterFilterInitializer.GetOpeningFilter(doc);
            var graphicSettings = GraphicSettingsInitializer.GetOpeningGraphicSettings(doc);
            return new FilterSetting(filter, graphicSettings);
        }

        private static FilterSetting GetMepFilterSetting(Document doc) {
            var filter = ParameterFilterInitializer.GetMepFilter(doc);
            var graphicSettings = GraphicSettingsInitializer.GetMepGraphicSettings(doc);
            return new FilterSetting(filter, graphicSettings);
        }

        private static TransparencySetting GetViewDisplayModelSetting() {
            return new TransparencySetting(20);
        }

        private static ParamSetting GetParamSetting(Document doc) {
            var bimGroup = new FilteredElementCollector(doc)
                        .OfClass(typeof(View3D))
                        .Cast<View3D>()
                        .Where(item => !item.IsTemplate)
                        .Select(item => item.GetParamValueOrDefault<string>(ProjectParamsConfig.Instance.ViewGroup))
                        .FirstOrDefault(item => item != null && item.Contains("BIM"));
            if(bimGroup != null) {
                return new ParamSetting(ProjectParamsConfig.Instance.ViewGroup.Name, new StringParamValue(bimGroup));
            } else {
                return null;
            }
        }
    }
}
