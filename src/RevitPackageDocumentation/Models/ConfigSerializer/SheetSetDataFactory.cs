using System;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPackageDocumentation.ViewModels.Configuration;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;
using RevitPackageDocumentation.ViewModels.FiltrationComboBoxVMs;

namespace RevitPackageDocumentation.Models.ConfigSerializer;

internal interface ISheetSetDataFactory {
    SheetSetData CreateSheetSetData(SheetSetVM vm);
    SheetData CreateSheetData(SheetVM vm);
    SheetComponentData CreateComponentData(SheetComponentVM vm);
    SheetComponentData CreateComponentData(Type componentType);
    PluginParamData CreatePluginParamData(PluginParamVM vm);
    PluginParamData CreatePluginParamData(Type paramType);
}

internal class SheetSetDataFactory : ISheetSetDataFactory {
    public SheetSetData CreateSheetSetData(SheetSetVM vm) {
        if(vm == null)
            return new SheetSetData();

        return new SheetSetData {
            Name = vm.Name,
            Sheets = vm.SheetList?.Select(CreateSheetData).ToList(),
            Params = vm.SheetSetParams.Params?.Select(CreatePluginParamData).ToList()
        };
    }

    public SheetData CreateSheetData(SheetVM vm) {
        if(vm == null)
            return new SheetData();

        return new SheetData {
            IsModuleCheck = vm.IsModuleCheck,
            ModuleName = vm.ModuleName,
            ModuleComment = vm.ModuleComment,
            CustomParamsList = GetCustomParametersList(vm),

            SheetNameFormula = vm.SheetNameFormula,
            SheetSize = vm.SheetSize,
            SheetCoefficient = vm.SheetCoefficient,
            TitleBlockFamilyName = vm.TitleBlockFamily?.Name,
            TitleBlockTypeName = vm.TitleBlockType?.Name,
            Views = vm.SheetComponents?.Select(CreateComponentData).ToList(),
        };
    }

    public SheetComponentData CreateComponentData(SheetComponentVM sheetComponentVM) {
        return sheetComponentVM switch {
            StructuralPlanViewVM vm => new StructuralPlanViewData {
                IsModuleCheck = vm.IsModuleCheck,
                ModuleName = vm.ModuleName,
                ModuleComment = vm.ModuleComment,
                CustomParamsList = GetCustomParametersList(vm),

                ViewNameFormula = vm.ViewNameFormula,
                ViewFamilyTypeName = vm.ViewFamilyType?.Name,
                ViewFamilyTypeFilterValues = GetFiltrationComboBoxFilterList(vm.ViewFamilyTypeFilter),
                ViewTemplateName = vm.ViewTemplate?.Name,
                ViewTemplateFilterValues = GetFiltrationComboBoxFilterList(vm.ViewTemplateFilter),
                ViewportTypeName = vm.ViewportType?.Name,
                ViewportTypeFilterValues = GetFiltrationComboBoxFilterList(vm.ViewportTypeFilter),
                ViewCount = vm.ViewCount,
                SelectedSelectElemParamName = vm.SelectedSelectElemParam?.ParamName,
            },

            StructuralCalloutViewVM vm => new StructuralCalloutViewData {
                IsModuleCheck = vm.IsModuleCheck,
                ModuleName = vm.ModuleName,
                ModuleComment = vm.ModuleComment,
                CustomParamsList = GetCustomParametersList(vm),

                ViewNameFormula = vm.ViewNameFormula,
                ViewFamilyTypeName = vm.ViewFamilyType?.Name,
                ViewFamilyTypeFilterValues = GetFiltrationComboBoxFilterList(vm.ViewFamilyTypeFilter),
                ViewTemplateName = vm.ViewTemplate?.Name,
                ViewTemplateFilterValues = GetFiltrationComboBoxFilterList(vm.ViewTemplateFilter),
                ViewportTypeName = vm.ViewportType?.Name,
                ViewportTypeFilterValues = GetFiltrationComboBoxFilterList(vm.ViewportTypeFilter),
                ViewCount = vm.ViewCount,
                SelectedSelectElemParamName = vm.SelectedSelectElemParam.ParamName,
            },

            SectionViewVM vm => new SectionViewData {
                IsModuleCheck = vm.IsModuleCheck,
                ModuleName = vm.ModuleName,
                ModuleComment = vm.ModuleComment,
                CustomParamsList = GetCustomParametersList(vm),

                ViewNameFormula = vm.ViewNameFormula,
                ViewFamilyTypeName = vm.ViewFamilyType?.Name,
                ViewFamilyTypeFilterValues = GetFiltrationComboBoxFilterList(vm.ViewFamilyTypeFilter),
                ViewTemplateName = vm.ViewTemplate?.Name,
                ViewTemplateFilterValues = GetFiltrationComboBoxFilterList(vm.ViewTemplateFilter),
                ViewportTypeName = vm.ViewportType?.Name,
                ViewportTypeFilterValues = GetFiltrationComboBoxFilterList(vm.ViewportTypeFilter),
                ViewCount = vm.ViewCount,
                SelectedSelectElemParamName = vm.SelectedSelectElemParam.ParamName,
            },

            ScheduleViewVM vm => new ScheduleViewData {
                IsModuleCheck = vm.IsModuleCheck,
                ModuleName = vm.ModuleName,
                ModuleComment = vm.ModuleComment,
                CustomParamsList = GetCustomParametersList(vm),

                ReferenceViewName = vm.ReferenceSpec.Name,
                ReferenceViewFilterValues = GetFiltrationComboBoxFilterList(vm.ReferenceSpecFilter),
                ViewNameFormula = vm.ViewNameFormula,
                ViewColumn = vm.ViewColumn,
                ViewCount = vm.ViewCount,
                ScheduleFilterList = GetScheduleFilterList(vm),
            },

            TextNoteVM vm => new TextNoteData {
                IsModuleCheck = vm.IsModuleCheck,
                ModuleName = vm.ModuleName,
                ModuleComment = vm.ModuleComment,
                CustomParamsList = GetCustomParametersList(vm),

                TextFormula = vm.TextFormula,
                TextNoteTypeName = vm.TextNoteType?.Name,
                TextNoteTypeFilterValues = GetFiltrationComboBoxFilterList(vm.TextNoteTypeFilter),
            },

            TypicalAnnotationVM vm => new TypicalAnnotationData {
                IsModuleCheck = vm.IsModuleCheck,
                ModuleName = vm.ModuleName,
                ModuleComment = vm.ModuleComment,
                CustomParamsList = GetCustomParametersList(vm),

                AnnotationFamilyName = vm.AnnotationFamily?.Name,
                AnnotationFamilyFilterValues = GetFiltrationComboBoxFilterList(vm.AnnotationFamilyFilter),
                AnnotationTypeName = vm.AnnotationType?.Name,
                AnnotationTypeFilterValues = GetFiltrationComboBoxFilterList(vm.AnnotationTypeFilter),
            },

            LegendViewVM vm => new LegendViewData {
                IsModuleCheck = vm.IsModuleCheck,
                ModuleName = vm.ModuleName,
                ModuleComment = vm.ModuleComment,
                CustomParamsList = GetCustomParametersList(vm),

                ViewName = vm.LegendView?.Name,
                ViewFilterValues = GetFiltrationComboBoxFilterList(vm.LegendViewFilter),
                ViewportTypeName = vm.ViewportType.Name,
                ViewportTypeFilterValues = GetFiltrationComboBoxFilterList(vm.ViewportTypeFilter),
            },

            _ => throw new NotSupportedException($"Тип '{sheetComponentVM?.GetType().Name}' не поддерживается")
        };
    }

    private CustomParametersListData GetCustomParametersList(BaseParamContainerVM vm) => new() {
        Params = vm.CustomParamsList.Params
            .Select(r => new CustomParameterData() {
                ParamName = r.ParamName ?? string.Empty,
                ParamValueFormula = r.ParamValueFormula ?? string.Empty,
            })
        .ToList()
    };

    private FiltrationComboBoxFilterListData GetFiltrationComboBoxFilterList(FiltrationComboBoxFilterListVM vm) => new() {
        ValueList = vm.ValueList
            .Select(r => new FiltrationComboBoxFilterData() {
                ValueFormula = r.ValueFormula ?? string.Empty,
            })
        .ToList()
    };

    private ScheduleFilterListData GetScheduleFilterList(ScheduleViewVM vm) => new() {
        ScheduleFilterRules = vm.ScheduleFilterList.ScheduleFilterRules
            .Select(r => new ScheduleFilterRuleData() {
                FieldName = r.SelectedSpecFieldName ?? string.Empty,
                FilterType = r.SelectedFilterType?.FilterType ?? ScheduleFilterType.Equal,
                FilterValueFormula = r.FilterValueFormula
            })
        .ToList()
    };

    public SheetComponentData CreateComponentData(Type componentType) {
        return componentType switch {
            Type t when t == typeof(StructuralPlanViewVM) => new StructuralPlanViewData(),
            Type t when t == typeof(StructuralCalloutViewVM) => new StructuralCalloutViewData(),
            Type t when t == typeof(SectionViewVM) => new SectionViewData(),
            Type t when t == typeof(ScheduleViewVM) => new ScheduleViewData(),
            Type t when t == typeof(TextNoteVM) => new TextNoteData(),
            Type t when t == typeof(TypicalAnnotationVM) => new TypicalAnnotationData(),
            Type t when t == typeof(LegendViewVM) => new LegendViewData(),
            _ => throw new NotSupportedException($"Тип '{componentType?.Name}' не поддерживается")
        };
    }

    public PluginParamData CreatePluginParamData(PluginParamVM vm) {
        if(vm == null)
            return null;

        return vm switch {
            StringParamVM stringVm => new StringParamData {
                ParamName = stringVm.ParamName,
                ParamComment = stringVm.ParamComment,
                StringValue = stringVm.StringValue
            },
            SelectElemParamVM selectVm => new SelectElemParamData {
                ParamName = selectVm.ParamName,
                ParamComment = selectVm.ParamComment
            },
            _ => throw new NotSupportedException($"Тип параметра '{vm?.GetType().Name}' не поддерживается")
        };
    }

    public PluginParamData CreatePluginParamData(Type paramType) {
        return paramType switch {
            Type t when t == typeof(StringParamVM) => new StringParamData(),
            Type t when t == typeof(SelectElemParamVM) => new SelectElemParamData(),
            _ => throw new NotSupportedException($"Тип параметра '{paramType?.Name}' не поддерживается")
        };
    }
}
