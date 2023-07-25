using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitPylonDocumentation.Models {
    internal class PluginConfig : ProjectConfig<PluginSettings> {
        [JsonIgnore] public override string ProjectConfigPath { get; set; }

        [JsonIgnore] public override IConfigSerializer Serializer { get; set; }

        public static PluginConfig GetPluginConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new ConfigSerializer())
                .SetPluginName(nameof(RevitPylonDocumentation))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(PluginConfig) + ".json")
                .Build<PluginConfig>();
        }
    }

    internal class PluginSettings : ProjectSettings {
        public override string ProjectName { get; set; }

        public bool NeedWorkWithGeneralView { get; set; }
        public bool NeedWorkWithGeneralPerpendicularView { get; set; }
        public bool NeedWorkWithTransverseViewFirst { get; set; }
        public bool NeedWorkWithTransverseViewSecond { get; set; }
        public bool NeedWorkWithTransverseViewThird { get; set; }
        public bool NeedWorkWithRebarSchedule { get; set; }
        public bool NeedWorkWithMaterialSchedule { get; set; }
        public bool NeedWorkWithSystemPartsSchedule { get; set; }
        public bool NeedWorkWithIFCPartsSchedule { get; set; }
        public bool NeedWorkWithLegend { get; set; }


        public string PROJECT_SECTION { get; set; }
        public string MARK { get; set; }
        public string DISPATCHER_GROUPING_FIRST { get; set; }
        public string DISPATCHER_GROUPING_SECOND { get; set; }
        public string SHEET_SIZE { get; set; }
        public string SHEET_COEFFICIENT { get; set; }
        public string SHEET_PREFIX { get; set; }
        public string SHEET_SUFFIX { get; set; }
        public string GENERAL_VIEW_PREFIX { get; set; }
        public string GENERAL_VIEW_SUFFIX { get; set; }
        public string GENERAL_VIEW_PERPENDICULAR_PREFIX { get; set; }
        public string GENERAL_VIEW_PERPENDICULAR_SUFFIX { get; set; }
        public string GENERAL_VIEW_TEMPLATE_NAME { get; set; }
        public string GENERAL_VIEW_X_OFFSET { get; set; }
        public string GENERAL_VIEW_Y_TOP_OFFSET { get; set; }
        public string GENERAL_VIEW_Y_BOTTOM_OFFSET { get; set; }
        public string TRANSVERSE_VIEW_FIRST_PREFIX { get; set; }
        public string TRANSVERSE_VIEW_FIRST_SUFFIX { get; set; }
        public string TRANSVERSE_VIEW_SECOND_PREFIX { get; set; }
        public string TRANSVERSE_VIEW_SECOND_SUFFIX { get; set; }
        public string TRANSVERSE_VIEW_THIRD_PREFIX { get; set; }
        public string TRANSVERSE_VIEW_THIRD_SUFFIX { get; set; }
        public string TRANSVERSE_VIEW_TEMPLATE_NAME { get; set; }
        public string TRANSVERSE_VIEW_X_OFFSET { get; set; }
        public string TRANSVERSE_VIEW_Y_OFFSET { get; set; }
        public string REBAR_SCHEDULE_PREFIX { get; set; }
        public string REBAR_SCHEDULE_SUFFIX { get; set; }
        public string MATERIAL_SCHEDULE_PREFIX { get; set; }
        public string MATERIAL_SCHEDULE_SUFFIX { get; set; }
        public string SYSTEM_PARTS_SCHEDULE_PREFIX { get; set; }
        public string SYSTEM_PARTS_SCHEDULE_SUFFIX { get; set; }
        public string IFC_PARTS_SCHEDULE_PREFIX { get; set; }
        public string IFC_PARTS_SCHEDULE_SUFFIX { get; set; }
        public string REBAR_SCHEDULE_NAME { get; set; }
        public string MATERIAL_SCHEDULE_NAME { get; set; }
        public string SYSTEM_PARTS_SCHEDULE_NAME { get; set; }
        public string IFC_PARTS_SCHEDULE_NAME { get; set; }
        public string REBAR_SCHEDULE_DISP1 { get; set; }
        public string MATERIAL_SCHEDULE_DISP1 { get; set; }
        public string SYSTEM_PARTS_SCHEDULE_DISP1 { get; set; }
        public string IFC_PARTS_SCHEDULE_DISP1 { get; set; }
        public string REBAR_SCHEDULE_DISP2 { get; set; }
        public string MATERIAL_SCHEDULE_DISP2 { get; set; }
        public string SYSTEM_PARTS_SCHEDULE_DISP2 { get; set; }
        public string IFC_PARTS_SCHEDULE_DISP2 { get; set; }
        public string TYPICAL_PYLON_FILTER_PARAMETER { get; set; }
        public string TYPICAL_PYLON_FILTER_VALUE { get; set; }
    }
}