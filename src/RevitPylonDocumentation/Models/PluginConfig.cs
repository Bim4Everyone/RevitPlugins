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

            projectSettings.ProjectSection = settings.ProjectSection;
            projectSettings.ProjectSectionTemp = settings.ProjectSection;
            projectSettings.Mark = settings.Mark;
            projectSettings.MarkTemp = settings.Mark;
            projectSettings.TitleBlockName = settings.TitleBlockName;
            projectSettings.TitleBlockNameTemp = settings.TitleBlockName;
            projectSettings.DispatcherGroupingFirst = settings.DispatcherGroupingFirst;
            projectSettings.DispatcherGroupingFirstTemp = settings.DispatcherGroupingFirst;
            projectSettings.DispatcherGroupingSecond = settings.DispatcherGroupingSecond;
            projectSettings.DispatcherGroupingSecondTemp = settings.DispatcherGroupingSecond;
            projectSettings.PylonLengthParamName = settings.PylonLengthParamName;
            projectSettings.PylonLengthParamNameTemp = settings.PylonLengthParamName;
            projectSettings.PylonWidthParamName = settings.PylonWidthParamName;
            projectSettings.PylonWidthParamNameTemp = settings.PylonWidthParamName;

            projectSettings.SheetSize = settings.SheetSize;
            projectSettings.SheetSizeTemp = settings.SheetSize;
            projectSettings.SheetCoefficient = settings.SheetCoefficient;
            projectSettings.SheetCoefficientTemp = settings.SheetCoefficient;
            projectSettings.SheetPrefix = settings.SheetPrefix;
            projectSettings.SheetPrefixTemp = settings.SheetPrefix;
            projectSettings.SheetSuffix = settings.SheetSuffix;
            projectSettings.SheetSuffixTemp = settings.SheetSuffix;

            projectSettings.TypicalPylonFilterParameter = settings.TypicalPylonFilterParameter;
            projectSettings.TypicalPylonFilterParameterTemp = settings.TypicalPylonFilterParameter;
            projectSettings.TypicalPylonFilterValue = settings.TypicalPylonFilterValue;
            projectSettings.TypicalPylonFilterValueTemp = settings.TypicalPylonFilterValue;

            projectSettings.LegendName = settings.LegendName;
            projectSettings.LegendNameTemp = settings.LegendName;
            projectSettings.LegendXOffset = settings.LegendXOffset;
            projectSettings.LegendXOffsetTemp = settings.LegendXOffset;
            projectSettings.LegendYOffset = settings.LegendYOffset;
            projectSettings.LegendYOffsetTemp = settings.LegendYOffset;

            viewSectionSettings.GeneralViewPrefix = settings.GeneralViewPrefix;
            viewSectionSettings.GeneralViewPrefixTemp = settings.GeneralViewPrefix;
            viewSectionSettings.GeneralViewSuffix = settings.GeneralViewSuffix;
            viewSectionSettings.GeneralViewSuffixTemp = settings.GeneralViewSuffix;
            viewSectionSettings.GeneralViewPerpendicularPrefix = settings.GeneralViewPerpendicularPrefix;
            viewSectionSettings.GeneralViewPerpendicularPrefixTemp = settings.GeneralViewPerpendicularPrefix;
            viewSectionSettings.GeneralViewPerpendicularSuffix = settings.GeneralViewPerpendicularSuffix;
            viewSectionSettings.GeneralViewPerpendicularSuffixTemp = settings.GeneralViewPerpendicularSuffix;
            viewSectionSettings.GeneralViewTemplateName = settings.GeneralViewTemplateName;
            viewSectionSettings.GeneralViewTemplateNameTemp = settings.GeneralViewTemplateName;



            viewSectionSettings.GeneralViewXOffset = settings.GeneralViewXOffset;
            viewSectionSettings.GeneralViewXOffsetTemp = settings.GeneralViewXOffset;
            viewSectionSettings.GeneralViewYTopOffset = settings.GeneralViewYTopOffset;
            viewSectionSettings.GeneralViewYTopOffsetTemp = settings.GeneralViewYTopOffset;
            viewSectionSettings.GeneralViewYBottomOffset = settings.GeneralViewYBottomOffset;
            viewSectionSettings.GeneralViewYBottomOffsetTemp = settings.GeneralViewYBottomOffset;

            viewSectionSettings.TransverseViewFirstPrefix = settings.TransverseViewFirstPrefix;
            viewSectionSettings.TransverseViewFirstPrefixTemp = settings.TransverseViewFirstPrefix;
            viewSectionSettings.TransverseViewFirstSuffix = settings.TransverseViewFirstSuffix;
            viewSectionSettings.TransverseViewFirstSuffixTemp = settings.TransverseViewFirstSuffix;
            viewSectionSettings.TransverseViewFirstElevation = settings.TransverseViewFirstElevation;
            viewSectionSettings.TransverseViewFirstElevationTemp = settings.TransverseViewFirstElevation;
            viewSectionSettings.TransverseViewSecondPrefix = settings.TransverseViewSecondPrefix;
            viewSectionSettings.TransverseViewSecondPrefixTemp = settings.TransverseViewSecondPrefix;
            viewSectionSettings.TransverseViewSecondSuffix = settings.TransverseViewSecondSuffix;
            viewSectionSettings.TransverseViewSecondSuffixTemp = settings.TransverseViewSecondSuffix;
            viewSectionSettings.TransverseViewSecondElevation = settings.TransverseViewSecondElevation;
            viewSectionSettings.TransverseViewSecondElevationTemp = settings.TransverseViewSecondElevation;
            viewSectionSettings.TransverseViewThirdPrefix = settings.TransverseViewThirdPrefix;
            viewSectionSettings.TransverseViewThirdPrefixTemp = settings.TransverseViewThirdPrefix;
            viewSectionSettings.TransverseViewThirdSuffix = settings.TransverseViewThirdSuffix;
            viewSectionSettings.TransverseViewThirdSuffixTemp = settings.TransverseViewThirdSuffix;
            viewSectionSettings.TransverseViewThirdElevation = settings.TransverseViewThirdElevation;
            viewSectionSettings.TransverseViewThirdElevationTemp = settings.TransverseViewThirdElevation;
            viewSectionSettings.TransverseViewTemplateName = settings.TransverseViewTemplateName;
            viewSectionSettings.TransverseViewTemplateNameTemp = settings.TransverseViewTemplateName;

            viewSectionSettings.TransverseViewXOffset = settings.TransverseViewXOffset;
            viewSectionSettings.TransverseViewXOffsetTemp = settings.TransverseViewXOffset;
            viewSectionSettings.TransverseViewYOffset = settings.TransverseViewYOffset;
            viewSectionSettings.TransverseViewYOffsetTemp = settings.TransverseViewYOffset;

            viewSectionSettings.ViewFamilyTypeName = settings.ViewFamilyTypeName;
            viewSectionSettings.ViewFamilyTypeNameTemp = settings.ViewFamilyTypeName;

            schedulesSettings.RebarSchedulePrefix = settings.RebarSchedulePrefix;
            schedulesSettings.RebarSchedulePrefixTemp = settings.RebarSchedulePrefix;
            schedulesSettings.RebarScheduleSuffix = settings.RebarScheduleSuffix;
            schedulesSettings.RebarScheduleSuffixTemp = settings.RebarScheduleSuffix;

            schedulesSettings.MaterialSchedulePrefix = settings.MaterialSchedulePrefix;
            schedulesSettings.MaterialSchedulePrefixTemp = settings.MaterialSchedulePrefix;
            schedulesSettings.MaterialScheduleSuffix = settings.MaterialScheduleSuffix;
            schedulesSettings.MaterialScheduleSuffixTemp = settings.MaterialScheduleSuffix;

            schedulesSettings.SystemPartsSchedulePrefix = settings.SystemPartsSchedulePrefix;
            schedulesSettings.SystemPartsSchedulePrefixTemp = settings.SystemPartsSchedulePrefix;
            schedulesSettings.SystemPartsScheduleSuffix = settings.SystemPartsScheduleSuffix;
            schedulesSettings.SystemPartsScheduleSuffixTemp = settings.SystemPartsScheduleSuffix;

            schedulesSettings.IFCPartsSchedulePrefix = settings.IFCPartsSchedulePrefix;
            schedulesSettings.IFCPartsSchedulePrefixTemp = settings.IFCPartsSchedulePrefix;
            schedulesSettings.IFCPartsScheduleSuffix = settings.IFCPartsScheduleSuffix;
            schedulesSettings.IFCPartsScheduleSuffixTemp = settings.IFCPartsScheduleSuffix;

            schedulesSettings.RebarScheduleName = settings.RebarScheduleName;
            schedulesSettings.RebarScheduleNameTemp = settings.RebarScheduleName;
            schedulesSettings.MaterialScheduleName = settings.MaterialScheduleName;
            schedulesSettings.MaterialScheduleNameTemp = settings.MaterialScheduleName;
            schedulesSettings.SytemPartsScheduleName = settings.SystemPartsScheduleName;
            schedulesSettings.SytemPartsScheduleNameTemp = settings.SystemPartsScheduleName;
            schedulesSettings.IFCPartsScheduleName = settings.IFCPartsScheduleName;
            schedulesSettings.IFCPartsScheduleNameTemp = settings.IFCPartsScheduleName;

            schedulesSettings.RebarScheduleDisp1 = settings.RebarScheduleDisp1;
            schedulesSettings.RebarScheduleDisp1Temp = settings.RebarScheduleDisp1;
            schedulesSettings.MaterialScheduleDisp1 = settings.MaterialScheduleDisp1;
            schedulesSettings.MaterialScheduleDisp1Temp = settings.MaterialScheduleDisp1;
            schedulesSettings.SystemPartsScheduleDisp1 = settings.SystemPartsScheduleDisp1;
            schedulesSettings.SystemPartsScheduleDisp1Temp = settings.SystemPartsScheduleDisp1;
            schedulesSettings.IFCPartsScheduleDisp1 = settings.IFCPartsScheduleDisp1;
            schedulesSettings.IFCPartsScheduleDisp1Temp = settings.IFCPartsScheduleDisp1;
            schedulesSettings.RebarScheduleDisp2 = settings.RebarScheduleDisp2;
            schedulesSettings.RebarScheduleDisp2Temp = settings.RebarScheduleDisp2;
            schedulesSettings.MaterialScheduleDisp2 = settings.MaterialScheduleDisp2;
            schedulesSettings.MaterialScheduleDisp2Temp = settings.MaterialScheduleDisp2;
            schedulesSettings.SystemPartsScheduleDisp2 = settings.SystemPartsScheduleDisp2;
            schedulesSettings.SystemPartsScheduleDisp2Temp = settings.SystemPartsScheduleDisp2;
            schedulesSettings.IFCPartsScheduleDisp2 = settings.IFCPartsScheduleDisp2;
            schedulesSettings.IFCPartsScheduleDisp2Temp = settings.IFCPartsScheduleDisp2;

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


            settings.ProjectSection = projectSettings.ProjectSection;
            settings.Mark = projectSettings.Mark;
            settings.TitleBlockName = projectSettings.TitleBlockName;
            settings.DispatcherGroupingFirst = projectSettings.DispatcherGroupingFirst;
            settings.DispatcherGroupingSecond = projectSettings.DispatcherGroupingSecond;

            settings.SheetSize = projectSettings.SheetSize;
            settings.SheetCoefficient = projectSettings.SheetCoefficient;
            settings.SheetPrefix = projectSettings.SheetPrefix;
            settings.SheetSuffix = projectSettings.SheetSuffix;

            settings.TypicalPylonFilterParameter = projectSettings.TypicalPylonFilterParameter;
            settings.TypicalPylonFilterValue = projectSettings.TypicalPylonFilterValue;

            settings.LegendName = projectSettings.LegendName;
            settings.LegendXOffset = projectSettings.LegendXOffset;
            settings.LegendYOffset = projectSettings.LegendYOffset;

            settings.PylonLengthParamName = projectSettings.PylonLengthParamName;
            settings.PylonWidthParamName = projectSettings.PylonWidthParamName;

            settings.GeneralViewPrefix = viewSectionSettings.GeneralViewPrefix;
            settings.GeneralViewSuffix = viewSectionSettings.GeneralViewSuffix;
            settings.GeneralViewPerpendicularPrefix = viewSectionSettings.GeneralViewPerpendicularPrefix;
            settings.GeneralViewPerpendicularSuffix = viewSectionSettings.GeneralViewPerpendicularSuffix;
            settings.GeneralViewTemplateName = viewSectionSettings.GeneralViewTemplateName;
            settings.GeneralViewXOffset = viewSectionSettings.GeneralViewXOffset;
            settings.GeneralViewYTopOffset = viewSectionSettings.GeneralViewYTopOffset;
            settings.GeneralViewYBottomOffset = viewSectionSettings.GeneralViewYBottomOffset;

            settings.TransverseViewFirstPrefix = viewSectionSettings.TransverseViewFirstPrefix;
            settings.TransverseViewFirstSuffix = viewSectionSettings.TransverseViewFirstSuffix;
            settings.TransverseViewFirstElevation = viewSectionSettings.TransverseViewFirstElevation;
            settings.TransverseViewSecondPrefix = viewSectionSettings.TransverseViewSecondPrefix;
            settings.TransverseViewSecondSuffix = viewSectionSettings.TransverseViewSecondSuffix;
            settings.TransverseViewSecondElevation = viewSectionSettings.TransverseViewSecondElevation;
            settings.TransverseViewThirdPrefix = viewSectionSettings.TransverseViewThirdPrefix;
            settings.TransverseViewThirdSuffix = viewSectionSettings.TransverseViewThirdSuffix;
            settings.TransverseViewThirdElevation = viewSectionSettings.TransverseViewThirdElevation;
            settings.TransverseViewTemplateName = viewSectionSettings.TransverseViewTemplateName;

            settings.TransverseViewXOffset = viewSectionSettings.TransverseViewXOffset;
            settings.TransverseViewYOffset = viewSectionSettings.TransverseViewYOffset;

            settings.ViewFamilyTypeName = viewSectionSettings.ViewFamilyTypeName;


            settings.RebarSchedulePrefix = schedulesSettings.RebarSchedulePrefix;
            settings.RebarScheduleSuffix = schedulesSettings.RebarScheduleSuffix;

            settings.MaterialSchedulePrefix = schedulesSettings.MaterialSchedulePrefix;
            settings.MaterialScheduleSuffix = schedulesSettings.MaterialScheduleSuffix;

            settings.SystemPartsSchedulePrefix = schedulesSettings.SystemPartsSchedulePrefix;
            settings.SystemPartsScheduleSuffix = schedulesSettings.SystemPartsScheduleSuffix;

            settings.IFCPartsSchedulePrefix = schedulesSettings.IFCPartsSchedulePrefix;
            settings.IFCPartsScheduleSuffix = schedulesSettings.IFCPartsScheduleSuffix;

            settings.RebarScheduleName = schedulesSettings.RebarScheduleName;
            settings.MaterialScheduleName = schedulesSettings.MaterialScheduleName;
            settings.SystemPartsScheduleName = schedulesSettings.SytemPartsScheduleName;
            settings.IFCPartsScheduleName = schedulesSettings.IFCPartsScheduleName;

            settings.RebarScheduleDisp1 = schedulesSettings.RebarScheduleDisp1;
            settings.MaterialScheduleDisp1 = schedulesSettings.MaterialScheduleDisp1;
            settings.SystemPartsScheduleDisp1 = schedulesSettings.SystemPartsScheduleDisp1;
            settings.IFCPartsScheduleDisp1 = schedulesSettings.IFCPartsScheduleDisp1;
            settings.RebarScheduleDisp2 = schedulesSettings.RebarScheduleDisp2;
            settings.MaterialScheduleDisp2 = schedulesSettings.MaterialScheduleDisp2;
            settings.SystemPartsScheduleDisp2 = schedulesSettings.SystemPartsScheduleDisp2;
            settings.IFCPartsScheduleDisp2 = schedulesSettings.IFCPartsScheduleDisp2;

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


        public string ProjectSection { get; set; }
        public string Mark { get; set; }
        public string TitleBlockName { get; set; }
        public string DispatcherGroupingFirst { get; set; }
        public string DispatcherGroupingSecond { get; set; }
        public string SheetSize { get; set; }
        public string SheetCoefficient { get; set; }
        public string SheetPrefix { get; set; }
        public string SheetSuffix { get; set; }
        public string TypicalPylonFilterParameter { get; set; }
        public string TypicalPylonFilterValue { get; set; }
        public string LegendName { get; set; }
        public string LegendXOffset { get; set; }
        public string LegendYOffset { get; set; }
        public string PylonLengthParamName { get; set; }
        public string PylonWidthParamName { get; set; }
        public string GeneralViewPrefix { get; set; }
        public string GeneralViewSuffix { get; set; }
        public string GeneralViewPerpendicularPrefix { get; set; }
        public string GeneralViewPerpendicularSuffix { get; set; }
        public string GeneralViewTemplateName { get; set; }
        public string GeneralViewXOffset { get; set; }
        public string GeneralViewYTopOffset { get; set; }
        public string GeneralViewYBottomOffset { get; set; }
        public string TransverseViewFirstPrefix { get; set; }
        public string TransverseViewFirstSuffix { get; set; }
        public string TransverseViewFirstElevation { get; set; }
        public string TransverseViewSecondPrefix { get; set; }
        public string TransverseViewSecondSuffix { get; set; }
        public string TransverseViewSecondElevation { get; set; }
        public string TransverseViewThirdPrefix { get; set; }
        public string TransverseViewThirdSuffix { get; set; }
        public string TransverseViewThirdElevation { get; set; }
        public string TransverseViewTemplateName { get; set; }
        public string TransverseViewXOffset { get; set; }
        public string TransverseViewYOffset { get; set; }
        public string ViewFamilyTypeName { get; set; }
        public string RebarSchedulePrefix { get; set; }
        public string RebarScheduleSuffix { get; set; }
        public string MaterialSchedulePrefix { get; set; }
        public string MaterialScheduleSuffix { get; set; }
        public string SystemPartsSchedulePrefix { get; set; }
        public string SystemPartsScheduleSuffix { get; set; }
        public string IFCPartsSchedulePrefix { get; set; }
        public string IFCPartsScheduleSuffix { get; set; }
        public string RebarScheduleName { get; set; }
        public string MaterialScheduleName { get; set; }
        public string SystemPartsScheduleName { get; set; }
        public string IFCPartsScheduleName { get; set; }
        public string RebarScheduleDisp1 { get; set; }
        public string MaterialScheduleDisp1 { get; set; }
        public string SystemPartsScheduleDisp1 { get; set; }
        public string IFCPartsScheduleDisp1 { get; set; }
        public string RebarScheduleDisp2 { get; set; }
        public string MaterialScheduleDisp2 { get; set; }
        public string SystemPartsScheduleDisp2 { get; set; }
        public string IFCPartsScheduleDisp2 { get; set; }
        public ObservableCollection<ScheduleFilterParamHelper> ParamsForScheduleFilters { get; set; }

    }
}