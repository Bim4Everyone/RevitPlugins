using System.Collections.ObjectModel;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

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
        var verticalViewSectionSettings = mainViewModel.VerticalViewSettings;
        var horizontalViewSectionSettings = mainViewModel.HorizontalViewSettings;
        var schedulesSettings = mainViewModel.SchedulesSettings;
        var legendsAndAnnotationsSettings = mainViewModel.LegendsAndAnnotationsSettings;
        var projectSettings = mainViewModel.ProjectSettings;

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

        verticalViewSectionSettings.GeneralViewPrefix = settings.GeneralViewPrefix;
        verticalViewSectionSettings.GeneralViewPrefixTemp = settings.GeneralViewPrefix;
        verticalViewSectionSettings.GeneralViewSuffix = settings.GeneralViewSuffix;
        verticalViewSectionSettings.GeneralViewSuffixTemp = settings.GeneralViewSuffix;
        verticalViewSectionSettings.GeneralViewPerpendicularPrefix = settings.GeneralViewPerpendicularPrefix;
        verticalViewSectionSettings.GeneralViewPerpendicularPrefixTemp = settings.GeneralViewPerpendicularPrefix;
        verticalViewSectionSettings.GeneralViewPerpendicularSuffix = settings.GeneralViewPerpendicularSuffix;
        verticalViewSectionSettings.GeneralViewPerpendicularSuffixTemp = settings.GeneralViewPerpendicularSuffix;

        verticalViewSectionSettings.GeneralRebarViewPrefix = settings.GeneralRebarViewPrefix;
        verticalViewSectionSettings.GeneralRebarViewPrefixTemp = settings.GeneralRebarViewPrefix;
        verticalViewSectionSettings.GeneralRebarViewSuffix = settings.GeneralRebarViewSuffix;
        verticalViewSectionSettings.GeneralRebarViewSuffixTemp = settings.GeneralRebarViewSuffix;
        verticalViewSectionSettings.GeneralRebarViewPerpendicularPrefix = settings.GeneralRebarViewPerpendicularPrefix;
        verticalViewSectionSettings.GeneralRebarViewPerpendicularPrefixTemp = settings.GeneralRebarViewPerpendicularPrefix;
        verticalViewSectionSettings.GeneralRebarViewPerpendicularSuffix = settings.GeneralRebarViewPerpendicularSuffix;
        verticalViewSectionSettings.GeneralRebarViewPerpendicularSuffixTemp = settings.GeneralRebarViewPerpendicularSuffix;

        verticalViewSectionSettings.GeneralViewTemplateName = settings.GeneralViewTemplateName;
        verticalViewSectionSettings.GeneralViewTemplateNameTemp = settings.GeneralViewTemplateName;
        verticalViewSectionSettings.GeneralRebarViewTemplateName = settings.GeneralRebarViewTemplateName;
        verticalViewSectionSettings.GeneralRebarViewTemplateNameTemp = settings.GeneralRebarViewTemplateName;

        verticalViewSectionSettings.GeneralViewXOffset = settings.GeneralViewXOffset;
        verticalViewSectionSettings.GeneralViewXOffsetTemp = settings.GeneralViewXOffset;
        verticalViewSectionSettings.GeneralViewYTopOffset = settings.GeneralViewYTopOffset;
        verticalViewSectionSettings.GeneralViewYTopOffsetTemp = settings.GeneralViewYTopOffset;
        verticalViewSectionSettings.GeneralViewYBottomOffset = settings.GeneralViewYBottomOffset;
        verticalViewSectionSettings.GeneralViewYBottomOffsetTemp = settings.GeneralViewYBottomOffset;

        verticalViewSectionSettings.GeneralViewPerpXOffset = settings.GeneralViewPerpXOffset;
        verticalViewSectionSettings.GeneralViewPerpXOffsetTemp = settings.GeneralViewPerpXOffset;
        verticalViewSectionSettings.GeneralViewPerpYTopOffset = settings.GeneralViewPerpYTopOffset;
        verticalViewSectionSettings.GeneralViewPerpYTopOffsetTemp = settings.GeneralViewPerpYTopOffset;
        verticalViewSectionSettings.GeneralViewPerpYBottomOffset = settings.GeneralViewPerpYBottomOffset;
        verticalViewSectionSettings.GeneralViewPerpYBottomOffsetTemp = settings.GeneralViewPerpYBottomOffset;

        horizontalViewSectionSettings.TransverseViewDepth = settings.TransverseViewDepth;
        horizontalViewSectionSettings.TransverseViewDepthTemp = settings.TransverseViewDepth;
        horizontalViewSectionSettings.TransverseViewFirstPrefix = settings.TransverseViewFirstPrefix;
        horizontalViewSectionSettings.TransverseViewFirstPrefixTemp = settings.TransverseViewFirstPrefix;
        horizontalViewSectionSettings.TransverseViewFirstSuffix = settings.TransverseViewFirstSuffix;
        horizontalViewSectionSettings.TransverseViewFirstSuffixTemp = settings.TransverseViewFirstSuffix;
        horizontalViewSectionSettings.TransverseViewFirstElevation = settings.TransverseViewFirstElevation;
        horizontalViewSectionSettings.TransverseViewFirstElevationTemp = settings.TransverseViewFirstElevation;
        horizontalViewSectionSettings.TransverseViewSecondPrefix = settings.TransverseViewSecondPrefix;
        horizontalViewSectionSettings.TransverseViewSecondPrefixTemp = settings.TransverseViewSecondPrefix;
        horizontalViewSectionSettings.TransverseViewSecondSuffix = settings.TransverseViewSecondSuffix;
        horizontalViewSectionSettings.TransverseViewSecondSuffixTemp = settings.TransverseViewSecondSuffix;
        horizontalViewSectionSettings.TransverseViewSecondElevation = settings.TransverseViewSecondElevation;
        horizontalViewSectionSettings.TransverseViewSecondElevationTemp = settings.TransverseViewSecondElevation;
        horizontalViewSectionSettings.TransverseViewThirdPrefix = settings.TransverseViewThirdPrefix;
        horizontalViewSectionSettings.TransverseViewThirdPrefixTemp = settings.TransverseViewThirdPrefix;
        horizontalViewSectionSettings.TransverseViewThirdSuffix = settings.TransverseViewThirdSuffix;
        horizontalViewSectionSettings.TransverseViewThirdSuffixTemp = settings.TransverseViewThirdSuffix;
        horizontalViewSectionSettings.TransverseViewThirdElevation = settings.TransverseViewThirdElevation;
        horizontalViewSectionSettings.TransverseViewThirdElevationTemp = settings.TransverseViewThirdElevation;
        horizontalViewSectionSettings.TransverseViewTemplateName = settings.TransverseViewTemplateName;
        horizontalViewSectionSettings.TransverseViewTemplateNameTemp = settings.TransverseViewTemplateName;

        horizontalViewSectionSettings.TransverseRebarViewDepth = settings.TransverseRebarViewDepth;
        horizontalViewSectionSettings.TransverseRebarViewDepthTemp = settings.TransverseRebarViewDepth;
        horizontalViewSectionSettings.TransverseRebarViewFirstPrefix = settings.TransverseRebarViewFirstPrefix;
        horizontalViewSectionSettings.TransverseRebarViewFirstPrefixTemp = settings.TransverseRebarViewFirstPrefix;
        horizontalViewSectionSettings.TransverseRebarViewFirstSuffix = settings.TransverseRebarViewFirstSuffix;
        horizontalViewSectionSettings.TransverseRebarViewFirstSuffixTemp = settings.TransverseRebarViewFirstSuffix;
        horizontalViewSectionSettings.TransverseRebarViewSecondPrefix = settings.TransverseRebarViewSecondPrefix;
        horizontalViewSectionSettings.TransverseRebarViewSecondPrefixTemp = settings.TransverseRebarViewSecondPrefix;
        horizontalViewSectionSettings.TransverseRebarViewSecondSuffix = settings.TransverseRebarViewSecondSuffix;
        horizontalViewSectionSettings.TransverseRebarViewSecondSuffixTemp = settings.TransverseRebarViewSecondSuffix;
        horizontalViewSectionSettings.TransverseRebarViewThirdPrefix = settings.TransverseRebarViewThirdPrefix;
        horizontalViewSectionSettings.TransverseRebarViewThirdPrefixTemp = settings.TransverseRebarViewThirdPrefix;
        horizontalViewSectionSettings.TransverseRebarViewThirdSuffix = settings.TransverseRebarViewThirdSuffix;
        horizontalViewSectionSettings.TransverseRebarViewThirdSuffixTemp = settings.TransverseRebarViewThirdSuffix;

        horizontalViewSectionSettings.TransverseRebarViewTemplateName = settings.TransverseRebarViewTemplateName;
        horizontalViewSectionSettings.TransverseRebarViewTemplateNameTemp = settings.TransverseRebarViewTemplateName;

        horizontalViewSectionSettings.TransverseViewXOffset = settings.TransverseViewXOffset;
        horizontalViewSectionSettings.TransverseViewXOffsetTemp = settings.TransverseViewXOffset;
        horizontalViewSectionSettings.TransverseViewYOffset = settings.TransverseViewYOffset;
        horizontalViewSectionSettings.TransverseViewYOffsetTemp = settings.TransverseViewYOffset;

        verticalViewSectionSettings.GeneralViewFamilyTypeName = settings.GeneralViewFamilyTypeName;
        verticalViewSectionSettings.GeneralViewFamilyTypeNameTemp = settings.GeneralViewFamilyTypeName;

        horizontalViewSectionSettings.TransverseViewFamilyTypeName = settings.TransverseViewFamilyTypeName;
        horizontalViewSectionSettings.TransverseViewFamilyTypeNameTemp = settings.TransverseViewFamilyTypeName;

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

        legendsAndAnnotationsSettings.LegendName = settings.LegendName;
        legendsAndAnnotationsSettings.LegendNameTemp = settings.LegendName;
        legendsAndAnnotationsSettings.LegendXOffset = settings.LegendXOffset;
        legendsAndAnnotationsSettings.LegendXOffsetTemp = settings.LegendXOffset;
        legendsAndAnnotationsSettings.LegendYOffset = settings.LegendYOffset;
        legendsAndAnnotationsSettings.LegendYOffsetTemp = settings.LegendYOffset;

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
    }

    internal void SetConfigProps(PluginSettings settings, MainViewModel mainViewModel) {
        var selectionSettings = mainViewModel.SelectionSettings;
        var verticalViewSectionSettings = mainViewModel.VerticalViewSettings;
        var horizontalViewSectionSettings = mainViewModel.HorizontalViewSettings;
        var schedulesSettings = mainViewModel.SchedulesSettings;
        var legendsAndAnnotationsSettings = mainViewModel.LegendsAndAnnotationsSettings;
        var projectSettings = mainViewModel.ProjectSettings;

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

        settings.GeneralViewPrefix = verticalViewSectionSettings.GeneralViewPrefix;
        settings.GeneralViewSuffix = verticalViewSectionSettings.GeneralViewSuffix;
        settings.GeneralViewPerpendicularPrefix = verticalViewSectionSettings.GeneralViewPerpendicularPrefix;
        settings.GeneralViewPerpendicularSuffix = verticalViewSectionSettings.GeneralViewPerpendicularSuffix;
        settings.GeneralViewTemplateName = verticalViewSectionSettings.GeneralViewTemplateName;

        settings.GeneralRebarViewPrefix = verticalViewSectionSettings.GeneralRebarViewPrefix;
        settings.GeneralRebarViewSuffix = verticalViewSectionSettings.GeneralRebarViewSuffix;
        settings.GeneralRebarViewPerpendicularPrefix = verticalViewSectionSettings.GeneralRebarViewPerpendicularPrefix;
        settings.GeneralRebarViewPerpendicularSuffix = verticalViewSectionSettings.GeneralRebarViewPerpendicularSuffix;
        settings.GeneralRebarViewTemplateName = verticalViewSectionSettings.GeneralRebarViewTemplateName;

        settings.GeneralViewXOffset = verticalViewSectionSettings.GeneralViewXOffset;
        settings.GeneralViewYTopOffset = verticalViewSectionSettings.GeneralViewYTopOffset;
        settings.GeneralViewYBottomOffset = verticalViewSectionSettings.GeneralViewYBottomOffset;
        settings.GeneralViewPerpXOffset = verticalViewSectionSettings.GeneralViewPerpXOffset;
        settings.GeneralViewPerpYTopOffset = verticalViewSectionSettings.GeneralViewPerpYTopOffset;
        settings.GeneralViewPerpYBottomOffset = verticalViewSectionSettings.GeneralViewPerpYBottomOffset;

        settings.TransverseViewDepth = horizontalViewSectionSettings.TransverseViewDepth;
        settings.TransverseViewFirstPrefix = horizontalViewSectionSettings.TransverseViewFirstPrefix;
        settings.TransverseViewFirstSuffix = horizontalViewSectionSettings.TransverseViewFirstSuffix;
        settings.TransverseViewFirstElevation = horizontalViewSectionSettings.TransverseViewFirstElevation;
        settings.TransverseViewSecondPrefix = horizontalViewSectionSettings.TransverseViewSecondPrefix;
        settings.TransverseViewSecondSuffix = horizontalViewSectionSettings.TransverseViewSecondSuffix;
        settings.TransverseViewSecondElevation = horizontalViewSectionSettings.TransverseViewSecondElevation;
        settings.TransverseViewThirdPrefix = horizontalViewSectionSettings.TransverseViewThirdPrefix;
        settings.TransverseViewThirdSuffix = horizontalViewSectionSettings.TransverseViewThirdSuffix;
        settings.TransverseViewThirdElevation = horizontalViewSectionSettings.TransverseViewThirdElevation;

        settings.TransverseRebarViewDepth = horizontalViewSectionSettings.TransverseRebarViewDepth;
        settings.TransverseRebarViewFirstPrefix = horizontalViewSectionSettings.TransverseRebarViewFirstPrefix;
        settings.TransverseRebarViewFirstSuffix = horizontalViewSectionSettings.TransverseRebarViewFirstSuffix;
        settings.TransverseRebarViewSecondPrefix = horizontalViewSectionSettings.TransverseRebarViewSecondPrefix;
        settings.TransverseRebarViewSecondSuffix = horizontalViewSectionSettings.TransverseRebarViewSecondSuffix;
        settings.TransverseRebarViewThirdPrefix = horizontalViewSectionSettings.TransverseRebarViewThirdPrefix;
        settings.TransverseRebarViewThirdSuffix = horizontalViewSectionSettings.TransverseRebarViewThirdSuffix;

        settings.TransverseViewTemplateName = horizontalViewSectionSettings.TransverseViewTemplateName;
        settings.TransverseRebarViewTemplateName = horizontalViewSectionSettings.TransverseRebarViewTemplateName;

        settings.TransverseViewXOffset = horizontalViewSectionSettings.TransverseViewXOffset;
        settings.TransverseViewYOffset = horizontalViewSectionSettings.TransverseViewYOffset;

        settings.GeneralViewFamilyTypeName = verticalViewSectionSettings.GeneralViewFamilyTypeName;
        settings.TransverseViewFamilyTypeName = horizontalViewSectionSettings.TransverseViewFamilyTypeName;

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

        settings.LegendName = legendsAndAnnotationsSettings.LegendName;
        settings.LegendXOffset = legendsAndAnnotationsSettings.LegendXOffset;
        settings.LegendYOffset = legendsAndAnnotationsSettings.LegendYOffset;

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

        settings.PylonLengthParamName = projectSettings.PylonLengthParamName;
        settings.PylonWidthParamName = projectSettings.PylonWidthParamName;
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
    public string GeneralViewFamilyTypeName { get; set; }
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
    public string TransverseViewFamilyTypeName { get; set; }

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
