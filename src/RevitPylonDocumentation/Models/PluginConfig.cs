using System.Collections.ObjectModel;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models;
internal class PluginConfig : ProjectConfig<PluginSettings> {
    [JsonIgnore] public override string ProjectConfigPath { get; set; }

    [JsonIgnore] public override IConfigSerializer Serializer { get; set; }

    public static PluginConfig GetPluginConfig(IConfigSerializer configSerializer) {
        return new ProjectConfigBuilder()
            .SetSerializer(configSerializer)
            .SetPluginName(nameof(RevitPylonDocumentation))
            .SetRevitVersion(ModuleEnvironment.RevitVersion)
            .SetProjectConfigName(nameof(PluginConfig) + ".json")
            .Build<PluginConfig>();
    }


    internal void GetConfigProps(PluginSettings settings, MainViewModel mainViewModel) {
        var selectionSettings = mainViewModel.SelectionSettings;
        var projectSettings = mainViewModel.ProjectSettings;
        var viewSectionSettings = mainViewModel.ViewSectionSettings;
        var schedulesSettings = mainViewModel.SchedulesSettings;

        selectionSettings.NeedWorkWithGeneralView = settings.NeedWorkWithGeneralView;
        selectionSettings.NeedWorkWithGeneralPerpendicularView = settings.NeedWorkWithGeneralPerpendicularView;
        selectionSettings.NeedWorkWithTransverseViewFirst = settings.NeedWorkWithTransverseViewFirst;
        selectionSettings.NeedWorkWithTransverseViewSecond = settings.NeedWorkWithTransverseViewSecond;
        selectionSettings.NeedWorkWithTransverseViewThird = settings.NeedWorkWithTransverseViewThird;

        selectionSettings.NeedWorkWithGeneralRebarView = settings.NeedWorkWithGeneralRebarView;
        selectionSettings.NeedWorkWithGeneralPerpendicularRebarView = settings.NeedWorkWithGeneralPerpendicularRebarView;
        selectionSettings.NeedWorkWithTransverseRebarViewFirst = settings.NeedWorkWithTransverseRebarViewFirst;
        selectionSettings.NeedWorkWithTransverseRebarViewSecond = settings.NeedWorkWithTransverseRebarViewSecond;
        selectionSettings.NeedWorkWithTransverseRebarViewThird = settings.NeedWorkWithTransverseRebarViewThird;

        selectionSettings.NeedWorkWithSkeletonSchedule = settings.NeedWorkWithSkeletonSchedule;
        selectionSettings.NeedWorkWithSkeletonByElemsSchedule = settings.NeedWorkWithSkeletonByElemsSchedule;
        selectionSettings.NeedWorkWithMaterialSchedule = settings.NeedWorkWithMaterialSchedule;
        selectionSettings.NeedWorkWithSystemPartsSchedule = settings.NeedWorkWithSystemPartsSchedule;
        selectionSettings.NeedWorkWithIfcPartsSchedule = settings.NeedWorkWithIfcPartsSchedule;
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

        projectSettings.DimensionTypeName = settings.DimensionTypeName;
        projectSettings.DimensionTypeNameTemp = settings.DimensionTypeName;
        projectSettings.SpotDimensionTypeName = settings.SpotDimensionTypeName;
        projectSettings.SpotDimensionTypeNameTemp = settings.SpotDimensionTypeName;

        projectSettings.SkeletonTagTypeName = settings.SkeletonTagTypeName;
        projectSettings.SkeletonTagTypeNameTemp = settings.SkeletonTagTypeName;
        projectSettings.RebarTagTypeWithSerifName = settings.RebarTagTypeWithSerifName;
        projectSettings.RebarTagTypeWithSerifNameTemp = settings.RebarTagTypeWithSerifName;
        projectSettings.RebarTagTypeWithStepName = settings.RebarTagTypeWithStepName;
        projectSettings.RebarTagTypeWithStepNameTemp = settings.RebarTagTypeWithStepName;
        projectSettings.RebarTagTypeWithCommentName = settings.RebarTagTypeWithCommentName;
        projectSettings.RebarTagTypeWithCommentNameTemp = settings.RebarTagTypeWithCommentName;
        projectSettings.UniversalTagTypeName = settings.UniversalTagTypeName;
        projectSettings.UniversalTagTypeNameTemp = settings.UniversalTagTypeName;

        projectSettings.BreakLineTypeName = settings.BreakLineTypeName;
        projectSettings.BreakLineTypeNameTemp = settings.BreakLineTypeName;
        projectSettings.ConcretingJointTypeName = settings.ConcretingJointTypeName;
        projectSettings.ConcretingJointTypeNameTemp = settings.ConcretingJointTypeName;

        viewSectionSettings.GeneralViewPrefix = settings.GeneralViewPrefix;
        viewSectionSettings.GeneralViewPrefixTemp = settings.GeneralViewPrefix;
        viewSectionSettings.GeneralViewSuffix = settings.GeneralViewSuffix;
        viewSectionSettings.GeneralViewSuffixTemp = settings.GeneralViewSuffix;
        viewSectionSettings.GeneralViewPerpendicularPrefix = settings.GeneralViewPerpendicularPrefix;
        viewSectionSettings.GeneralViewPerpendicularPrefixTemp = settings.GeneralViewPerpendicularPrefix;
        viewSectionSettings.GeneralViewPerpendicularSuffix = settings.GeneralViewPerpendicularSuffix;
        viewSectionSettings.GeneralViewPerpendicularSuffixTemp = settings.GeneralViewPerpendicularSuffix;

        viewSectionSettings.GeneralRebarViewPrefix = settings.GeneralRebarViewPrefix;
        viewSectionSettings.GeneralRebarViewPrefixTemp = settings.GeneralRebarViewPrefix;
        viewSectionSettings.GeneralRebarViewSuffix = settings.GeneralRebarViewSuffix;
        viewSectionSettings.GeneralRebarViewSuffixTemp = settings.GeneralRebarViewSuffix;
        viewSectionSettings.GeneralRebarViewPerpendicularPrefix = settings.GeneralRebarViewPerpendicularPrefix;
        viewSectionSettings.GeneralRebarViewPerpendicularPrefixTemp = settings.GeneralRebarViewPerpendicularPrefix;
        viewSectionSettings.GeneralRebarViewPerpendicularSuffix = settings.GeneralRebarViewPerpendicularSuffix;
        viewSectionSettings.GeneralRebarViewPerpendicularSuffixTemp = settings.GeneralRebarViewPerpendicularSuffix;

        viewSectionSettings.GeneralViewTemplateName = settings.GeneralViewTemplateName;
        viewSectionSettings.GeneralViewTemplateNameTemp = settings.GeneralViewTemplateName;
        viewSectionSettings.GeneralRebarViewTemplateName = settings.GeneralRebarViewTemplateName;
        viewSectionSettings.GeneralRebarViewTemplateNameTemp = settings.GeneralRebarViewTemplateName;

        viewSectionSettings.GeneralViewXOffset = settings.GeneralViewXOffset;
        viewSectionSettings.GeneralViewXOffsetTemp = settings.GeneralViewXOffset;
        viewSectionSettings.GeneralViewYTopOffset = settings.GeneralViewYTopOffset;
        viewSectionSettings.GeneralViewYTopOffsetTemp = settings.GeneralViewYTopOffset;
        viewSectionSettings.GeneralViewYBottomOffset = settings.GeneralViewYBottomOffset;
        viewSectionSettings.GeneralViewYBottomOffsetTemp = settings.GeneralViewYBottomOffset;

        viewSectionSettings.GeneralViewPerpXOffset = settings.GeneralViewPerpXOffset;
        viewSectionSettings.GeneralViewPerpXOffsetTemp = settings.GeneralViewPerpXOffset;
        viewSectionSettings.GeneralViewPerpYTopOffset = settings.GeneralViewPerpYTopOffset;
        viewSectionSettings.GeneralViewPerpYTopOffsetTemp = settings.GeneralViewPerpYTopOffset;
        viewSectionSettings.GeneralViewPerpYBottomOffset = settings.GeneralViewPerpYBottomOffset;
        viewSectionSettings.GeneralViewPerpYBottomOffsetTemp = settings.GeneralViewPerpYBottomOffset;

        viewSectionSettings.TransverseViewDepth = settings.TransverseViewDepth;
        viewSectionSettings.TransverseViewDepthTemp = settings.TransverseViewDepth;
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

        viewSectionSettings.TransverseRebarViewDepth = settings.TransverseRebarViewDepth;
        viewSectionSettings.TransverseRebarViewDepthTemp = settings.TransverseRebarViewDepth;
        viewSectionSettings.TransverseRebarViewFirstPrefix = settings.TransverseRebarViewFirstPrefix;
        viewSectionSettings.TransverseRebarViewFirstPrefixTemp = settings.TransverseRebarViewFirstPrefix;
        viewSectionSettings.TransverseRebarViewFirstSuffix = settings.TransverseRebarViewFirstSuffix;
        viewSectionSettings.TransverseRebarViewFirstSuffixTemp = settings.TransverseRebarViewFirstSuffix;
        viewSectionSettings.TransverseRebarViewSecondPrefix = settings.TransverseRebarViewSecondPrefix;
        viewSectionSettings.TransverseRebarViewSecondPrefixTemp = settings.TransverseRebarViewSecondPrefix;
        viewSectionSettings.TransverseRebarViewSecondSuffix = settings.TransverseRebarViewSecondSuffix;
        viewSectionSettings.TransverseRebarViewSecondSuffixTemp = settings.TransverseRebarViewSecondSuffix;
        viewSectionSettings.TransverseRebarViewThirdPrefix = settings.TransverseRebarViewThirdPrefix;
        viewSectionSettings.TransverseRebarViewThirdPrefixTemp = settings.TransverseRebarViewThirdPrefix;
        viewSectionSettings.TransverseRebarViewThirdSuffix = settings.TransverseRebarViewThirdSuffix;
        viewSectionSettings.TransverseRebarViewThirdSuffixTemp = settings.TransverseRebarViewThirdSuffix;

        viewSectionSettings.TransverseRebarViewTemplateName = settings.TransverseRebarViewTemplateName;
        viewSectionSettings.TransverseRebarViewTemplateNameTemp = settings.TransverseRebarViewTemplateName;

        viewSectionSettings.TransverseViewXOffset = settings.TransverseViewXOffset;
        viewSectionSettings.TransverseViewXOffsetTemp = settings.TransverseViewXOffset;
        viewSectionSettings.TransverseViewYOffset = settings.TransverseViewYOffset;
        viewSectionSettings.TransverseViewYOffsetTemp = settings.TransverseViewYOffset;

        viewSectionSettings.ViewFamilyTypeName = settings.ViewFamilyTypeName;
        viewSectionSettings.ViewFamilyTypeNameTemp = settings.ViewFamilyTypeName;

        schedulesSettings.SkeletonSchedulePrefix = settings.SkeletonSchedulePrefix;
        schedulesSettings.SkeletonSchedulePrefixTemp = settings.SkeletonSchedulePrefix;
        schedulesSettings.SkeletonScheduleSuffix = settings.SkeletonScheduleSuffix;
        schedulesSettings.SkeletonScheduleSuffixTemp = settings.SkeletonScheduleSuffix;
        schedulesSettings.SkeletonByElemsSchedulePrefix = settings.SkeletonByElemsSchedulePrefix;
        schedulesSettings.SkeletonByElemsSchedulePrefixTemp = settings.SkeletonByElemsSchedulePrefix;
        schedulesSettings.SkeletonByElemsScheduleSuffix = settings.SkeletonByElemsScheduleSuffix;
        schedulesSettings.SkeletonByElemsScheduleSuffixTemp = settings.SkeletonByElemsScheduleSuffix;

        schedulesSettings.MaterialSchedulePrefix = settings.MaterialSchedulePrefix;
        schedulesSettings.MaterialSchedulePrefixTemp = settings.MaterialSchedulePrefix;
        schedulesSettings.MaterialScheduleSuffix = settings.MaterialScheduleSuffix;
        schedulesSettings.MaterialScheduleSuffixTemp = settings.MaterialScheduleSuffix;

        schedulesSettings.SystemPartsSchedulePrefix = settings.SystemPartsSchedulePrefix;
        schedulesSettings.SystemPartsSchedulePrefixTemp = settings.SystemPartsSchedulePrefix;
        schedulesSettings.SystemPartsScheduleSuffix = settings.SystemPartsScheduleSuffix;
        schedulesSettings.SystemPartsScheduleSuffixTemp = settings.SystemPartsScheduleSuffix;

        schedulesSettings.IfcPartsSchedulePrefix = settings.IfcPartsSchedulePrefix;
        schedulesSettings.IfcPartsSchedulePrefixTemp = settings.IfcPartsSchedulePrefix;
        schedulesSettings.IfcPartsScheduleSuffix = settings.IfcPartsScheduleSuffix;
        schedulesSettings.IfcPartsScheduleSuffixTemp = settings.IfcPartsScheduleSuffix;

        schedulesSettings.SkeletonScheduleName = settings.SkeletonScheduleName;
        schedulesSettings.SkeletonScheduleNameTemp = settings.SkeletonScheduleName;
        schedulesSettings.SkeletonByElemsScheduleName = settings.SkeletonByElemsScheduleName;
        schedulesSettings.SkeletonByElemsScheduleNameTemp = settings.SkeletonByElemsScheduleName;

        schedulesSettings.MaterialScheduleName = settings.MaterialScheduleName;
        schedulesSettings.MaterialScheduleNameTemp = settings.MaterialScheduleName;
        schedulesSettings.SystemPartsScheduleName = settings.SystemPartsScheduleName;
        schedulesSettings.SystemPartsScheduleNameTemp = settings.SystemPartsScheduleName;
        schedulesSettings.IfcPartsScheduleName = settings.IfcPartsScheduleName;
        schedulesSettings.IfcPartsScheduleNameTemp = settings.IfcPartsScheduleName;

        schedulesSettings.SkeletonScheduleDisp1 = settings.SkeletonScheduleDisp1;
        schedulesSettings.SkeletonScheduleDisp1Temp = settings.SkeletonScheduleDisp1;
        schedulesSettings.SkeletonByElemsScheduleDisp1 = settings.SkeletonByElemsScheduleDisp1;
        schedulesSettings.SkeletonByElemsScheduleDisp1Temp = settings.SkeletonByElemsScheduleDisp1;

        schedulesSettings.MaterialScheduleDisp1 = settings.MaterialScheduleDisp1;
        schedulesSettings.MaterialScheduleDisp1Temp = settings.MaterialScheduleDisp1;
        schedulesSettings.SystemPartsScheduleDisp1 = settings.SystemPartsScheduleDisp1;
        schedulesSettings.SystemPartsScheduleDisp1Temp = settings.SystemPartsScheduleDisp1;
        schedulesSettings.IfcPartsScheduleDisp1 = settings.IfcPartsScheduleDisp1;
        schedulesSettings.IfcPartsScheduleDisp1Temp = settings.IfcPartsScheduleDisp1;

        schedulesSettings.SkeletonScheduleDisp2 = settings.SkeletonScheduleDisp2;
        schedulesSettings.SkeletonScheduleDisp2Temp = settings.SkeletonScheduleDisp2;
        schedulesSettings.SkeletonByElemsScheduleDisp2 = settings.SkeletonByElemsScheduleDisp2;
        schedulesSettings.SkeletonByElemsScheduleDisp2Temp = settings.SkeletonByElemsScheduleDisp2;

        schedulesSettings.MaterialScheduleDisp2 = settings.MaterialScheduleDisp2;
        schedulesSettings.MaterialScheduleDisp2Temp = settings.MaterialScheduleDisp2;
        schedulesSettings.SystemPartsScheduleDisp2 = settings.SystemPartsScheduleDisp2;
        schedulesSettings.SystemPartsScheduleDisp2Temp = settings.SystemPartsScheduleDisp2;
        schedulesSettings.IfcPartsScheduleDisp2 = settings.IfcPartsScheduleDisp2;
        schedulesSettings.IfcPartsScheduleDisp2Temp = settings.IfcPartsScheduleDisp2;

        schedulesSettings.ParamsForScheduleFilters = settings.ParamsForScheduleFilters;
        schedulesSettings.ParamsForScheduleFiltersTemp = settings.ParamsForScheduleFilters;
    }

    internal void SetConfigProps(PluginSettings settings, MainViewModel mainViewModel) {
        var selectionSettings = mainViewModel.SelectionSettings;
        var projectSettings = mainViewModel.ProjectSettings;
        var viewSectionSettings = mainViewModel.ViewSectionSettings;
        var schedulesSettings = mainViewModel.SchedulesSettings;

        settings.NeedWorkWithGeneralView = selectionSettings.NeedWorkWithGeneralView;
        settings.NeedWorkWithGeneralPerpendicularView = selectionSettings.NeedWorkWithGeneralPerpendicularView;
        settings.NeedWorkWithTransverseViewFirst = selectionSettings.NeedWorkWithTransverseViewFirst;
        settings.NeedWorkWithTransverseViewSecond = selectionSettings.NeedWorkWithTransverseViewSecond;
        settings.NeedWorkWithTransverseViewThird = selectionSettings.NeedWorkWithTransverseViewThird;

        settings.NeedWorkWithGeneralRebarView = selectionSettings.NeedWorkWithGeneralRebarView;
        settings.NeedWorkWithGeneralPerpendicularRebarView = selectionSettings.NeedWorkWithGeneralPerpendicularRebarView;
        settings.NeedWorkWithTransverseRebarViewFirst = selectionSettings.NeedWorkWithTransverseRebarViewFirst;
        settings.NeedWorkWithTransverseRebarViewSecond = selectionSettings.NeedWorkWithTransverseRebarViewSecond;
        settings.NeedWorkWithTransverseRebarViewThird = selectionSettings.NeedWorkWithTransverseRebarViewThird;

        settings.NeedWorkWithSkeletonSchedule = selectionSettings.NeedWorkWithSkeletonSchedule;
        settings.NeedWorkWithSkeletonByElemsSchedule = selectionSettings.NeedWorkWithSkeletonByElemsSchedule;
        settings.NeedWorkWithMaterialSchedule = selectionSettings.NeedWorkWithMaterialSchedule;
        settings.NeedWorkWithSystemPartsSchedule = selectionSettings.NeedWorkWithSystemPartsSchedule;
        settings.NeedWorkWithIfcPartsSchedule = selectionSettings.NeedWorkWithIfcPartsSchedule;
        settings.NeedWorkWithLegend = selectionSettings.NeedWorkWithLegend;

        settings.ProjectSection = projectSettings.ProjectSection;
        settings.Mark = projectSettings.Mark;
        settings.TitleBlockName = projectSettings.TitleBlockName;
        settings.DispatcherGroupingFirst = projectSettings.DispatcherGroupingFirst;
        settings.DispatcherGroupingSecond = projectSettings.DispatcherGroupingSecond;
        settings.DimensionTypeName = projectSettings.DimensionTypeName;
        settings.SpotDimensionTypeName = projectSettings.SpotDimensionTypeName;
        settings.SkeletonTagTypeName = projectSettings.SkeletonTagTypeName;
        settings.RebarTagTypeWithSerifName = projectSettings.RebarTagTypeWithSerifName;
        settings.RebarTagTypeWithStepName = projectSettings.RebarTagTypeWithStepName;
        settings.RebarTagTypeWithCommentName = projectSettings.RebarTagTypeWithCommentName;
        settings.UniversalTagTypeName = projectSettings.UniversalTagTypeName;

        settings.BreakLineTypeName = projectSettings.BreakLineTypeName;
        settings.ConcretingJointTypeName = projectSettings.ConcretingJointTypeName;

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

        settings.GeneralRebarViewPrefix = viewSectionSettings.GeneralRebarViewPrefix;
        settings.GeneralRebarViewSuffix = viewSectionSettings.GeneralRebarViewSuffix;
        settings.GeneralRebarViewPerpendicularPrefix = viewSectionSettings.GeneralRebarViewPerpendicularPrefix;
        settings.GeneralRebarViewPerpendicularSuffix = viewSectionSettings.GeneralRebarViewPerpendicularSuffix;
        settings.GeneralRebarViewTemplateName = viewSectionSettings.GeneralRebarViewTemplateName;

        settings.GeneralViewXOffset = viewSectionSettings.GeneralViewXOffset;
        settings.GeneralViewYTopOffset = viewSectionSettings.GeneralViewYTopOffset;
        settings.GeneralViewYBottomOffset = viewSectionSettings.GeneralViewYBottomOffset;
        settings.GeneralViewPerpXOffset = viewSectionSettings.GeneralViewPerpXOffset;
        settings.GeneralViewPerpYTopOffset = viewSectionSettings.GeneralViewPerpYTopOffset;
        settings.GeneralViewPerpYBottomOffset = viewSectionSettings.GeneralViewPerpYBottomOffset;

        settings.TransverseViewDepth = viewSectionSettings.TransverseViewDepth;
        settings.TransverseViewFirstPrefix = viewSectionSettings.TransverseViewFirstPrefix;
        settings.TransverseViewFirstSuffix = viewSectionSettings.TransverseViewFirstSuffix;
        settings.TransverseViewFirstElevation = viewSectionSettings.TransverseViewFirstElevation;
        settings.TransverseViewSecondPrefix = viewSectionSettings.TransverseViewSecondPrefix;
        settings.TransverseViewSecondSuffix = viewSectionSettings.TransverseViewSecondSuffix;
        settings.TransverseViewSecondElevation = viewSectionSettings.TransverseViewSecondElevation;
        settings.TransverseViewThirdPrefix = viewSectionSettings.TransverseViewThirdPrefix;
        settings.TransverseViewThirdSuffix = viewSectionSettings.TransverseViewThirdSuffix;
        settings.TransverseViewThirdElevation = viewSectionSettings.TransverseViewThirdElevation;

        settings.TransverseRebarViewDepth = viewSectionSettings.TransverseRebarViewDepth;
        settings.TransverseRebarViewFirstPrefix = viewSectionSettings.TransverseRebarViewFirstPrefix;
        settings.TransverseRebarViewFirstSuffix = viewSectionSettings.TransverseRebarViewFirstSuffix;
        settings.TransverseRebarViewSecondPrefix = viewSectionSettings.TransverseRebarViewSecondPrefix;
        settings.TransverseRebarViewSecondSuffix = viewSectionSettings.TransverseRebarViewSecondSuffix;
        settings.TransverseRebarViewThirdPrefix = viewSectionSettings.TransverseRebarViewThirdPrefix;
        settings.TransverseRebarViewThirdSuffix = viewSectionSettings.TransverseRebarViewThirdSuffix;

        settings.TransverseViewTemplateName = viewSectionSettings.TransverseViewTemplateName;
        settings.TransverseRebarViewTemplateName = viewSectionSettings.TransverseRebarViewTemplateName;

        settings.TransverseViewXOffset = viewSectionSettings.TransverseViewXOffset;
        settings.TransverseViewYOffset = viewSectionSettings.TransverseViewYOffset;

        settings.ViewFamilyTypeName = viewSectionSettings.ViewFamilyTypeName;

        settings.SkeletonSchedulePrefix = schedulesSettings.SkeletonSchedulePrefix;
        settings.SkeletonScheduleSuffix = schedulesSettings.SkeletonScheduleSuffix;
        settings.SkeletonByElemsSchedulePrefix = schedulesSettings.SkeletonByElemsSchedulePrefix;
        settings.SkeletonByElemsScheduleSuffix = schedulesSettings.SkeletonByElemsScheduleSuffix;

        settings.MaterialSchedulePrefix = schedulesSettings.MaterialSchedulePrefix;
        settings.MaterialScheduleSuffix = schedulesSettings.MaterialScheduleSuffix;

        settings.SystemPartsSchedulePrefix = schedulesSettings.SystemPartsSchedulePrefix;
        settings.SystemPartsScheduleSuffix = schedulesSettings.SystemPartsScheduleSuffix;

        settings.IfcPartsSchedulePrefix = schedulesSettings.IfcPartsSchedulePrefix;
        settings.IfcPartsScheduleSuffix = schedulesSettings.IfcPartsScheduleSuffix;

        settings.SkeletonScheduleName = schedulesSettings.SkeletonScheduleName;
        settings.SkeletonByElemsScheduleName = schedulesSettings.SkeletonByElemsScheduleName;

        settings.MaterialScheduleName = schedulesSettings.MaterialScheduleName;
        settings.SystemPartsScheduleName = schedulesSettings.SystemPartsScheduleName;
        settings.IfcPartsScheduleName = schedulesSettings.IfcPartsScheduleName;

        settings.SkeletonScheduleDisp1 = schedulesSettings.SkeletonScheduleDisp1;
        settings.SkeletonByElemsScheduleDisp1 = schedulesSettings.SkeletonByElemsScheduleDisp1;

        settings.MaterialScheduleDisp1 = schedulesSettings.MaterialScheduleDisp1;
        settings.SystemPartsScheduleDisp1 = schedulesSettings.SystemPartsScheduleDisp1;
        settings.IfcPartsScheduleDisp1 = schedulesSettings.IfcPartsScheduleDisp1;

        settings.SkeletonScheduleDisp2 = schedulesSettings.SkeletonScheduleDisp2;
        settings.SkeletonByElemsScheduleDisp2 = schedulesSettings.SkeletonByElemsScheduleDisp2;

        settings.MaterialScheduleDisp2 = schedulesSettings.MaterialScheduleDisp2;
        settings.SystemPartsScheduleDisp2 = schedulesSettings.SystemPartsScheduleDisp2;
        settings.IfcPartsScheduleDisp2 = schedulesSettings.IfcPartsScheduleDisp2;

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
    public bool NeedWorkWithGeneralRebarView { get; set; }
    public bool NeedWorkWithGeneralPerpendicularRebarView { get; set; }
    public bool NeedWorkWithTransverseRebarViewFirst { get; set; }
    public bool NeedWorkWithTransverseRebarViewSecond { get; set; }
    public bool NeedWorkWithTransverseRebarViewThird { get; set; }

    public bool NeedWorkWithMaterialSchedule { get; set; }
    public bool NeedWorkWithSystemPartsSchedule { get; set; }
    public bool NeedWorkWithIfcPartsSchedule { get; set; }
    public bool NeedWorkWithLegend { get; set; }
    public bool NeedWorkWithSkeletonSchedule { get; set; }
    public bool NeedWorkWithSkeletonByElemsSchedule { get; set; }

    public string ProjectSection { get; set; }
    public string Mark { get; set; }
    public string TitleBlockName { get; set; }
    public string DispatcherGroupingFirst { get; set; }
    public string DispatcherGroupingSecond { get; set; }
    public string DimensionTypeName { get; set; }
    public string SpotDimensionTypeName { get; set; }
    public string SkeletonTagTypeName { get; set; }
    public string RebarTagTypeWithSerifName { get; set; }
    public string RebarTagTypeWithStepName { get; set; }
    public string RebarTagTypeWithCommentName { get; set; }
    public string UniversalTagTypeName { get; set; }
    public string BreakLineTypeName { get; set; }
    public string ConcretingJointTypeName { get; set; }
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
    public string GeneralRebarViewPrefix { get; set; }
    public string GeneralRebarViewSuffix { get; set; }
    public string GeneralRebarViewPerpendicularPrefix { get; set; }
    public string GeneralRebarViewPerpendicularSuffix { get; set; }
    public string GeneralRebarViewTemplateName { get; set; }
    public string GeneralViewXOffset { get; set; }
    public string GeneralViewYTopOffset { get; set; }
    public string GeneralViewYBottomOffset { get; set; }
    public string GeneralViewPerpXOffset { get; set; }
    public string GeneralViewPerpYTopOffset { get; set; }
    public string GeneralViewPerpYBottomOffset { get; set; }
    public string TransverseViewDepth { get; set; }
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
    public string TransverseRebarViewDepth { get; set; }
    public string TransverseRebarViewFirstPrefix { get; set; }
    public string TransverseRebarViewFirstSuffix { get; set; }
    public string TransverseRebarViewSecondPrefix { get; set; }
    public string TransverseRebarViewSecondSuffix { get; set; }
    public string TransverseRebarViewThirdPrefix { get; set; }
    public string TransverseRebarViewThirdSuffix { get; set; }
    public string TransverseRebarViewTemplateName { get; set; }
    public string TransverseViewXOffset { get; set; }
    public string TransverseViewYOffset { get; set; }
    public string ViewFamilyTypeName { get; set; }

    public string SkeletonSchedulePrefix { get; set; }
    public string SkeletonScheduleSuffix { get; set; }
    public string SkeletonByElemsSchedulePrefix { get; set; }
    public string SkeletonByElemsScheduleSuffix { get; set; }

    public string MaterialSchedulePrefix { get; set; }
    public string MaterialScheduleSuffix { get; set; }
    public string SystemPartsSchedulePrefix { get; set; }
    public string SystemPartsScheduleSuffix { get; set; }
    public string IfcPartsSchedulePrefix { get; set; }
    public string IfcPartsScheduleSuffix { get; set; }

    public string SkeletonScheduleName { get; set; }
    public string SkeletonByElemsScheduleName { get; set; }

    public string MaterialScheduleName { get; set; }
    public string SystemPartsScheduleName { get; set; }
    public string IfcPartsScheduleName { get; set; }

    public string SkeletonScheduleDisp1 { get; set; }
    public string SkeletonByElemsScheduleDisp1 { get; set; }

    public string MaterialScheduleDisp1 { get; set; }
    public string SystemPartsScheduleDisp1 { get; set; }
    public string IfcPartsScheduleDisp1 { get; set; }

    public string SkeletonScheduleDisp2 { get; set; }
    public string SkeletonByElemsScheduleDisp2 { get; set; }

    public string MaterialScheduleDisp2 { get; set; }
    public string SystemPartsScheduleDisp2 { get; set; }
    public string IfcPartsScheduleDisp2 { get; set; }
    public ObservableCollection<ScheduleFilterParamHelper> ParamsForScheduleFilters { get; set; }
}
