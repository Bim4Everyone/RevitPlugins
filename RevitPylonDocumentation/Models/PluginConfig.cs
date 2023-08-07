using System.Collections.ObjectModel;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

using RevitPylonDocumentation.Models.UserSettings;
using RevitPylonDocumentation.ViewModels;

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



        internal void GetConfigProps(PluginSettings settings, MainViewModel mainViewModel) {

            UserSelectionSettings selectionSettings = mainViewModel.SelectionSettings;
            UserProjectSettings projectSettings = mainViewModel.ProjectSettings;
            UserViewSectionSettings viewSectionSettings = mainViewModel.ViewSectionSettings;
            UserSchedulesSettings schedulesSettings = mainViewModel.SchedulesSettings;


            selectionSettings.NeedWorkWithGeneralView = settings.NeedWorkWithGeneralView;
            selectionSettings.NeedWorkWithGeneralPerpendicularView = settings.NeedWorkWithGeneralPerpendicularView;
            selectionSettings.NeedWorkWithTransverseViewFirst = settings.NeedWorkWithTransverseViewFirst;
            selectionSettings.NeedWorkWithTransverseViewSecond = settings.NeedWorkWithTransverseViewSecond;
            selectionSettings.NeedWorkWithTransverseViewThird = settings.NeedWorkWithTransverseViewThird;
            selectionSettings.NeedWorkWithRebarSchedule = settings.NeedWorkWithRebarSchedule;
            selectionSettings.NeedWorkWithMaterialSchedule = settings.NeedWorkWithMaterialSchedule;
            selectionSettings.NeedWorkWithSystemPartsSchedule = settings.NeedWorkWithSystemPartsSchedule;
            selectionSettings.NeedWorkWithIFCPartsSchedule = settings.NeedWorkWithIFCPartsSchedule;
            selectionSettings.NeedWorkWithLegend = settings.NeedWorkWithLegend;

            projectSettings.PROJECT_SECTION = settings.PROJECT_SECTION;
            projectSettings.PROJECT_SECTION_TEMP = settings.PROJECT_SECTION;
            projectSettings.MARK = settings.MARK;
            projectSettings.MARK_TEMP = settings.MARK;
            projectSettings.TITLEBLOCK_NAME = settings.TITLEBLOCK_NAME;
            projectSettings.TITLEBLOCK_NAME_TEMP = settings.TITLEBLOCK_NAME;
            projectSettings.DISPATCHER_GROUPING_FIRST = settings.DISPATCHER_GROUPING_FIRST;
            projectSettings.DISPATCHER_GROUPING_FIRST_TEMP = settings.DISPATCHER_GROUPING_FIRST;
            projectSettings.DISPATCHER_GROUPING_SECOND = settings.DISPATCHER_GROUPING_SECOND;
            projectSettings.DISPATCHER_GROUPING_SECOND_TEMP = settings.DISPATCHER_GROUPING_SECOND;

            projectSettings.SHEET_SIZE = settings.SHEET_SIZE;
            projectSettings.SHEET_SIZE_TEMP = settings.SHEET_SIZE;
            projectSettings.SHEET_COEFFICIENT = settings.SHEET_COEFFICIENT;
            projectSettings.SHEET_COEFFICIENT_TEMP = settings.SHEET_COEFFICIENT;
            projectSettings.SHEET_PREFIX = settings.SHEET_PREFIX;
            projectSettings.SHEET_PREFIX_TEMP = settings.SHEET_PREFIX;
            projectSettings.SHEET_SUFFIX = settings.SHEET_SUFFIX;
            projectSettings.SHEET_SUFFIX_TEMP = settings.SHEET_SUFFIX;

            projectSettings.TYPICAL_PYLON_FILTER_PARAMETER = settings.TYPICAL_PYLON_FILTER_PARAMETER;
            projectSettings.TYPICAL_PYLON_FILTER_PARAMETER_TEMP = settings.TYPICAL_PYLON_FILTER_PARAMETER;
            projectSettings.TYPICAL_PYLON_FILTER_VALUE = settings.TYPICAL_PYLON_FILTER_VALUE;
            projectSettings.TYPICAL_PYLON_FILTER_VALUE_TEMP = settings.TYPICAL_PYLON_FILTER_VALUE;

            projectSettings.LEGEND_NAME = settings.LEGEND_NAME;
            projectSettings.LEGEND_NAME_TEMP = settings.LEGEND_NAME;

            viewSectionSettings.GENERAL_VIEW_PREFIX = settings.GENERAL_VIEW_PREFIX;
            viewSectionSettings.GENERAL_VIEW_PREFIX_TEMP = settings.GENERAL_VIEW_PREFIX;
            viewSectionSettings.GENERAL_VIEW_SUFFIX = settings.GENERAL_VIEW_SUFFIX;
            viewSectionSettings.GENERAL_VIEW_SUFFIX_TEMP = settings.GENERAL_VIEW_SUFFIX;
            viewSectionSettings.GENERAL_VIEW_PERPENDICULAR_PREFIX = settings.GENERAL_VIEW_PERPENDICULAR_PREFIX;
            viewSectionSettings.GENERAL_VIEW_PERPENDICULAR_PREFIX_TEMP = settings.GENERAL_VIEW_PERPENDICULAR_PREFIX;
            viewSectionSettings.GENERAL_VIEW_PERPENDICULAR_SUFFIX = settings.GENERAL_VIEW_PERPENDICULAR_SUFFIX;
            viewSectionSettings.GENERAL_VIEW_PERPENDICULAR_SUFFIX_TEMP = settings.GENERAL_VIEW_PERPENDICULAR_SUFFIX;
            viewSectionSettings.GENERAL_VIEW_TEMPLATE_NAME = settings.GENERAL_VIEW_TEMPLATE_NAME;
            viewSectionSettings.GENERAL_VIEW_TEMPLATE_NAME_TEMP = settings.GENERAL_VIEW_TEMPLATE_NAME;



            viewSectionSettings.GENERAL_VIEW_X_OFFSET = settings.GENERAL_VIEW_X_OFFSET;
            viewSectionSettings.GENERAL_VIEW_X_OFFSET_TEMP = settings.GENERAL_VIEW_X_OFFSET;
            viewSectionSettings.GENERAL_VIEW_Y_TOP_OFFSET = settings.GENERAL_VIEW_Y_TOP_OFFSET;
            viewSectionSettings.GENERAL_VIEW_Y_TOP_OFFSET_TEMP = settings.GENERAL_VIEW_Y_TOP_OFFSET;
            viewSectionSettings.GENERAL_VIEW_Y_BOTTOM_OFFSET = settings.GENERAL_VIEW_Y_BOTTOM_OFFSET;
            viewSectionSettings.GENERAL_VIEW_Y_BOTTOM_OFFSET_TEMP = settings.GENERAL_VIEW_Y_BOTTOM_OFFSET;

            viewSectionSettings.TRANSVERSE_VIEW_FIRST_PREFIX = settings.TRANSVERSE_VIEW_FIRST_PREFIX;
            viewSectionSettings.TRANSVERSE_VIEW_FIRST_PREFIX_TEMP = settings.TRANSVERSE_VIEW_FIRST_PREFIX;
            viewSectionSettings.TRANSVERSE_VIEW_FIRST_SUFFIX = settings.TRANSVERSE_VIEW_FIRST_SUFFIX;
            viewSectionSettings.TRANSVERSE_VIEW_FIRST_SUFFIX_TEMP = settings.TRANSVERSE_VIEW_FIRST_SUFFIX;
            viewSectionSettings.TRANSVERSE_VIEW_SECOND_PREFIX = settings.TRANSVERSE_VIEW_SECOND_PREFIX;
            viewSectionSettings.TRANSVERSE_VIEW_SECOND_PREFIX_TEMP = settings.TRANSVERSE_VIEW_SECOND_PREFIX;
            viewSectionSettings.TRANSVERSE_VIEW_SECOND_SUFFIX = settings.TRANSVERSE_VIEW_SECOND_SUFFIX;
            viewSectionSettings.TRANSVERSE_VIEW_SECOND_SUFFIX_TEMP = settings.TRANSVERSE_VIEW_SECOND_SUFFIX;
            viewSectionSettings.TRANSVERSE_VIEW_THIRD_PREFIX = settings.TRANSVERSE_VIEW_THIRD_PREFIX;
            viewSectionSettings.TRANSVERSE_VIEW_THIRD_PREFIX_TEMP = settings.TRANSVERSE_VIEW_THIRD_PREFIX;
            viewSectionSettings.TRANSVERSE_VIEW_THIRD_SUFFIX = settings.TRANSVERSE_VIEW_THIRD_SUFFIX;
            viewSectionSettings.TRANSVERSE_VIEW_THIRD_SUFFIX_TEMP = settings.TRANSVERSE_VIEW_THIRD_SUFFIX;
            viewSectionSettings.TRANSVERSE_VIEW_TEMPLATE_NAME = settings.TRANSVERSE_VIEW_TEMPLATE_NAME;
            viewSectionSettings.TRANSVERSE_VIEW_TEMPLATE_NAME_TEMP = settings.TRANSVERSE_VIEW_TEMPLATE_NAME;

            viewSectionSettings.TRANSVERSE_VIEW_X_OFFSET = settings.TRANSVERSE_VIEW_X_OFFSET;
            viewSectionSettings.TRANSVERSE_VIEW_X_OFFSET_TEMP = settings.TRANSVERSE_VIEW_X_OFFSET;
            viewSectionSettings.TRANSVERSE_VIEW_Y_OFFSET = settings.TRANSVERSE_VIEW_Y_OFFSET;
            viewSectionSettings.TRANSVERSE_VIEW_Y_OFFSET_TEMP = settings.TRANSVERSE_VIEW_Y_OFFSET;

            viewSectionSettings.VIEW_FAMILY_TYPE_NAME = settings.VIEW_FAMILY_TYPE_NAME;
            viewSectionSettings.VIEW_FAMILY_TYPE_NAME_TEMP = settings.VIEW_FAMILY_TYPE_NAME;

            schedulesSettings.REBAR_SCHEDULE_PREFIX = settings.REBAR_SCHEDULE_PREFIX;
            schedulesSettings.REBAR_SCHEDULE_PREFIX_TEMP = settings.REBAR_SCHEDULE_PREFIX;
            schedulesSettings.REBAR_SCHEDULE_SUFFIX = settings.REBAR_SCHEDULE_SUFFIX;
            schedulesSettings.REBAR_SCHEDULE_SUFFIX_TEMP = settings.REBAR_SCHEDULE_SUFFIX;

            schedulesSettings.MATERIAL_SCHEDULE_PREFIX = settings.MATERIAL_SCHEDULE_PREFIX;
            schedulesSettings.MATERIAL_SCHEDULE_PREFIX_TEMP = settings.MATERIAL_SCHEDULE_PREFIX;
            schedulesSettings.MATERIAL_SCHEDULE_SUFFIX = settings.MATERIAL_SCHEDULE_SUFFIX;
            schedulesSettings.MATERIAL_SCHEDULE_SUFFIX_TEMP = settings.MATERIAL_SCHEDULE_SUFFIX;

            schedulesSettings.SYSTEM_PARTS_SCHEDULE_PREFIX = settings.SYSTEM_PARTS_SCHEDULE_PREFIX;
            schedulesSettings.SYSTEM_PARTS_SCHEDULE_PREFIX_TEMP = settings.SYSTEM_PARTS_SCHEDULE_PREFIX;
            schedulesSettings.SYSTEM_PARTS_SCHEDULE_SUFFIX = settings.SYSTEM_PARTS_SCHEDULE_SUFFIX;
            schedulesSettings.SYSTEM_PARTS_SCHEDULE_SUFFIX_TEMP = settings.SYSTEM_PARTS_SCHEDULE_SUFFIX;

            schedulesSettings.IFC_PARTS_SCHEDULE_PREFIX = settings.IFC_PARTS_SCHEDULE_PREFIX;
            schedulesSettings.IFC_PARTS_SCHEDULE_PREFIX_TEMP = settings.IFC_PARTS_SCHEDULE_PREFIX;
            schedulesSettings.IFC_PARTS_SCHEDULE_SUFFIX = settings.IFC_PARTS_SCHEDULE_SUFFIX;
            schedulesSettings.IFC_PARTS_SCHEDULE_SUFFIX_TEMP = settings.IFC_PARTS_SCHEDULE_SUFFIX;

            schedulesSettings.REBAR_SCHEDULE_NAME = settings.REBAR_SCHEDULE_NAME;
            schedulesSettings.REBAR_SCHEDULE_NAME_TEMP = settings.REBAR_SCHEDULE_NAME;
            schedulesSettings.MATERIAL_SCHEDULE_NAME = settings.MATERIAL_SCHEDULE_NAME;
            schedulesSettings.MATERIAL_SCHEDULE_NAME_TEMP = settings.MATERIAL_SCHEDULE_NAME;
            schedulesSettings.SYSTEM_PARTS_SCHEDULE_NAME = settings.SYSTEM_PARTS_SCHEDULE_NAME;
            schedulesSettings.SYSTEM_PARTS_SCHEDULE_NAME_TEMP = settings.SYSTEM_PARTS_SCHEDULE_NAME;
            schedulesSettings.IFC_PARTS_SCHEDULE_NAME = settings.IFC_PARTS_SCHEDULE_NAME;
            schedulesSettings.IFC_PARTS_SCHEDULE_NAME_TEMP = settings.IFC_PARTS_SCHEDULE_NAME;

            schedulesSettings.REBAR_SCHEDULE_DISP1 = settings.REBAR_SCHEDULE_DISP1;
            schedulesSettings.REBAR_SCHEDULE_DISP1_TEMP = settings.REBAR_SCHEDULE_DISP1;
            schedulesSettings.MATERIAL_SCHEDULE_DISP1 = settings.MATERIAL_SCHEDULE_DISP1;
            schedulesSettings.MATERIAL_SCHEDULE_DISP1_TEMP = settings.MATERIAL_SCHEDULE_DISP1;
            schedulesSettings.SYSTEM_PARTS_SCHEDULE_DISP1 = settings.SYSTEM_PARTS_SCHEDULE_DISP1;
            schedulesSettings.SYSTEM_PARTS_SCHEDULE_DISP1_TEMP = settings.SYSTEM_PARTS_SCHEDULE_DISP1;
            schedulesSettings.IFC_PARTS_SCHEDULE_DISP1 = settings.IFC_PARTS_SCHEDULE_DISP1;
            schedulesSettings.IFC_PARTS_SCHEDULE_DISP1_TEMP = settings.IFC_PARTS_SCHEDULE_DISP1;
            schedulesSettings.REBAR_SCHEDULE_DISP2 = settings.REBAR_SCHEDULE_DISP2;
            schedulesSettings.REBAR_SCHEDULE_DISP2_TEMP = settings.REBAR_SCHEDULE_DISP2;
            schedulesSettings.MATERIAL_SCHEDULE_DISP2 = settings.MATERIAL_SCHEDULE_DISP2;
            schedulesSettings.MATERIAL_SCHEDULE_DISP2_TEMP = settings.MATERIAL_SCHEDULE_DISP2;
            schedulesSettings.SYSTEM_PARTS_SCHEDULE_DISP2 = settings.SYSTEM_PARTS_SCHEDULE_DISP2;
            schedulesSettings.SYSTEM_PARTS_SCHEDULE_DISP2_TEMP = settings.SYSTEM_PARTS_SCHEDULE_DISP2;
            schedulesSettings.IFC_PARTS_SCHEDULE_DISP2 = settings.IFC_PARTS_SCHEDULE_DISP2;
            schedulesSettings.IFC_PARTS_SCHEDULE_DISP2_TEMP = settings.IFC_PARTS_SCHEDULE_DISP2;

            schedulesSettings.ParamsForScheduleFilters = settings.ParamsForScheduleFilters;
            schedulesSettings.ParamsForScheduleFiltersTemp = settings.ParamsForScheduleFilters;
        }

        internal void SetConfigProps(PluginSettings settings, MainViewModel mainViewModel) {

            UserSelectionSettings selectionSettings = mainViewModel.SelectionSettings;
            UserProjectSettings projectSettings = mainViewModel.ProjectSettings;
            UserViewSectionSettings viewSectionSettings = mainViewModel.ViewSectionSettings;
            UserSchedulesSettings schedulesSettings = mainViewModel.SchedulesSettings;


            settings.NeedWorkWithGeneralView = selectionSettings.NeedWorkWithGeneralView;
            settings.NeedWorkWithGeneralPerpendicularView = selectionSettings.NeedWorkWithGeneralPerpendicularView;
            settings.NeedWorkWithTransverseViewFirst = selectionSettings.NeedWorkWithTransverseViewFirst;
            settings.NeedWorkWithTransverseViewSecond = selectionSettings.NeedWorkWithTransverseViewSecond;
            settings.NeedWorkWithTransverseViewThird = selectionSettings.NeedWorkWithTransverseViewThird;
            settings.NeedWorkWithRebarSchedule = selectionSettings.NeedWorkWithRebarSchedule;
            settings.NeedWorkWithMaterialSchedule = selectionSettings.NeedWorkWithMaterialSchedule;
            settings.NeedWorkWithSystemPartsSchedule = selectionSettings.NeedWorkWithSystemPartsSchedule;
            settings.NeedWorkWithIFCPartsSchedule = selectionSettings.NeedWorkWithIFCPartsSchedule;
            settings.NeedWorkWithLegend = selectionSettings.NeedWorkWithLegend;


            settings.PROJECT_SECTION = projectSettings.PROJECT_SECTION;
            settings.MARK = projectSettings.MARK;
            settings.TITLEBLOCK_NAME = projectSettings.TITLEBLOCK_NAME;
            settings.DISPATCHER_GROUPING_FIRST = projectSettings.DISPATCHER_GROUPING_FIRST;
            settings.DISPATCHER_GROUPING_SECOND = projectSettings.DISPATCHER_GROUPING_SECOND;

            settings.SHEET_SIZE = projectSettings.SHEET_SIZE;
            settings.SHEET_COEFFICIENT = projectSettings.SHEET_COEFFICIENT;
            settings.SHEET_PREFIX = projectSettings.SHEET_PREFIX;
            settings.SHEET_SUFFIX = projectSettings.SHEET_SUFFIX;

            settings.TYPICAL_PYLON_FILTER_PARAMETER = projectSettings.TYPICAL_PYLON_FILTER_PARAMETER;
            settings.TYPICAL_PYLON_FILTER_VALUE = projectSettings.TYPICAL_PYLON_FILTER_VALUE;

            settings.LEGEND_NAME = projectSettings.LEGEND_NAME;

            settings.GENERAL_VIEW_PREFIX = viewSectionSettings.GENERAL_VIEW_PREFIX;
            settings.GENERAL_VIEW_SUFFIX = viewSectionSettings.GENERAL_VIEW_SUFFIX;
            settings.GENERAL_VIEW_PERPENDICULAR_PREFIX = viewSectionSettings.GENERAL_VIEW_PERPENDICULAR_PREFIX;
            settings.GENERAL_VIEW_PERPENDICULAR_SUFFIX = viewSectionSettings.GENERAL_VIEW_PERPENDICULAR_SUFFIX;
            settings.GENERAL_VIEW_TEMPLATE_NAME = viewSectionSettings.GENERAL_VIEW_TEMPLATE_NAME;
            settings.GENERAL_VIEW_X_OFFSET = viewSectionSettings.GENERAL_VIEW_X_OFFSET;
            settings.GENERAL_VIEW_Y_TOP_OFFSET = viewSectionSettings.GENERAL_VIEW_Y_TOP_OFFSET;
            settings.GENERAL_VIEW_Y_BOTTOM_OFFSET = viewSectionSettings.GENERAL_VIEW_Y_BOTTOM_OFFSET;

            settings.TRANSVERSE_VIEW_FIRST_PREFIX = viewSectionSettings.TRANSVERSE_VIEW_FIRST_PREFIX;
            settings.TRANSVERSE_VIEW_FIRST_SUFFIX = viewSectionSettings.TRANSVERSE_VIEW_FIRST_SUFFIX;
            settings.TRANSVERSE_VIEW_SECOND_PREFIX = viewSectionSettings.TRANSVERSE_VIEW_SECOND_PREFIX;
            settings.TRANSVERSE_VIEW_SECOND_SUFFIX = viewSectionSettings.TRANSVERSE_VIEW_SECOND_SUFFIX;
            settings.TRANSVERSE_VIEW_THIRD_PREFIX = viewSectionSettings.TRANSVERSE_VIEW_THIRD_PREFIX;
            settings.TRANSVERSE_VIEW_THIRD_SUFFIX = viewSectionSettings.TRANSVERSE_VIEW_THIRD_SUFFIX;
            settings.TRANSVERSE_VIEW_TEMPLATE_NAME = viewSectionSettings.TRANSVERSE_VIEW_TEMPLATE_NAME;

            settings.TRANSVERSE_VIEW_X_OFFSET = viewSectionSettings.TRANSVERSE_VIEW_X_OFFSET;
            settings.TRANSVERSE_VIEW_Y_OFFSET = viewSectionSettings.TRANSVERSE_VIEW_Y_OFFSET;

            settings.VIEW_FAMILY_TYPE_NAME = viewSectionSettings.VIEW_FAMILY_TYPE_NAME;


            settings.REBAR_SCHEDULE_PREFIX = schedulesSettings.REBAR_SCHEDULE_PREFIX;
            settings.REBAR_SCHEDULE_SUFFIX = schedulesSettings.REBAR_SCHEDULE_SUFFIX;

            settings.MATERIAL_SCHEDULE_PREFIX = schedulesSettings.MATERIAL_SCHEDULE_PREFIX;
            settings.MATERIAL_SCHEDULE_SUFFIX = schedulesSettings.MATERIAL_SCHEDULE_SUFFIX;

            settings.SYSTEM_PARTS_SCHEDULE_PREFIX = schedulesSettings.SYSTEM_PARTS_SCHEDULE_PREFIX;
            settings.SYSTEM_PARTS_SCHEDULE_SUFFIX = schedulesSettings.SYSTEM_PARTS_SCHEDULE_SUFFIX;

            settings.IFC_PARTS_SCHEDULE_PREFIX = schedulesSettings.IFC_PARTS_SCHEDULE_PREFIX;
            settings.IFC_PARTS_SCHEDULE_SUFFIX = schedulesSettings.IFC_PARTS_SCHEDULE_SUFFIX;

            settings.REBAR_SCHEDULE_NAME = schedulesSettings.REBAR_SCHEDULE_NAME;
            settings.MATERIAL_SCHEDULE_NAME = schedulesSettings.MATERIAL_SCHEDULE_NAME;
            settings.SYSTEM_PARTS_SCHEDULE_NAME = schedulesSettings.SYSTEM_PARTS_SCHEDULE_NAME;
            settings.IFC_PARTS_SCHEDULE_NAME = schedulesSettings.IFC_PARTS_SCHEDULE_NAME;

            settings.REBAR_SCHEDULE_DISP1 = schedulesSettings.REBAR_SCHEDULE_DISP1;
            settings.MATERIAL_SCHEDULE_DISP1 = schedulesSettings.MATERIAL_SCHEDULE_DISP1;
            settings.SYSTEM_PARTS_SCHEDULE_DISP1 = schedulesSettings.SYSTEM_PARTS_SCHEDULE_DISP1;
            settings.IFC_PARTS_SCHEDULE_DISP1 = schedulesSettings.IFC_PARTS_SCHEDULE_DISP1;
            settings.REBAR_SCHEDULE_DISP2 = schedulesSettings.REBAR_SCHEDULE_DISP2;
            settings.MATERIAL_SCHEDULE_DISP2 = schedulesSettings.MATERIAL_SCHEDULE_DISP2;
            settings.SYSTEM_PARTS_SCHEDULE_DISP2 = schedulesSettings.SYSTEM_PARTS_SCHEDULE_DISP2;
            settings.IFC_PARTS_SCHEDULE_DISP2 = schedulesSettings.IFC_PARTS_SCHEDULE_DISP2;

            settings.ParamsForScheduleFilters = schedulesSettings.ParamsForScheduleFilters;
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
        public string TITLEBLOCK_NAME { get; set; }
        public string DISPATCHER_GROUPING_FIRST { get; set; }
        public string DISPATCHER_GROUPING_SECOND { get; set; }
        public string SHEET_SIZE { get; set; }
        public string SHEET_COEFFICIENT { get; set; }
        public string SHEET_PREFIX { get; set; }
        public string SHEET_SUFFIX { get; set; }
        public string TYPICAL_PYLON_FILTER_PARAMETER { get; set; }
        public string TYPICAL_PYLON_FILTER_VALUE { get; set; }
        public string LEGEND_NAME { get; set; }
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
        public string VIEW_FAMILY_TYPE_NAME { get; set; }
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
        public ObservableCollection<ScheduleFilterParamHelper> ParamsForScheduleFilters { get; set; }

    }
}