using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitPackageDocumentation.ViewModels.Configuration;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;
using RevitPackageDocumentation.ViewModels.FiltrationComboBoxVMs;

namespace RevitPackageDocumentation.Models.ConfigSerializer;

/// <summary>
/// Фабрика DTO конфигурации: из моделей представления (для сохранения) и пустых DTO по типу (для добавления модуля
/// или параметра в окне)
/// </summary>
internal interface ISheetSetDataFactory {
    SheetSetData CreateSheetSetData();
    SheetSetData CreateSheetSetData(SheetSetVM vm);
    SheetData CreateSheetData(SheetVM vm);
    SheetComponentData CreateComponentData(SheetComponentVM vm);
    SheetComponentData CreateComponentData(Type componentType);
    PluginParamData CreatePluginParamData(PluginParamVM vm);
    PluginParamData CreatePluginParamData(Type paramType);
}

internal class SheetSetDataFactory : ISheetSetDataFactory {
    private readonly ILocalizationService _localizationService;

    public SheetSetDataFactory(ILocalizationService localizationService) {
        _localizationService = localizationService;
    }

    public SheetSetData CreateSheetSetData() {
        return new SheetSetData();
    }

    public SheetSetData CreateSheetSetData(SheetSetVM vm) {
        if(vm == null)
            return CreateSheetSetData();

        return new SheetSetData {
            Name = vm.Name,
            Sheets = vm.SheetList?.Select(CreateSheetData).ToList(),
            Params = vm.SheetSetParams.Params?.Select(CreatePluginParamData).ToList(),
            ParamsComment = vm.SheetSetParams.ModuleComment
        };
    }

    public SheetData CreateSheetData(SheetVM vm) {
        if(vm == null)
            return new SheetData();

        return WithModuleData(new SheetData {
            SheetNameFormula = vm.SheetNameFormula,
            SheetNumberFormula = vm.SheetNumberFormula,
            SheetSize = vm.SheetSize,
            SheetCoefficient = vm.SheetCoefficient,
            TitleBlockFamilyName = vm.TitleBlockFamily?.Name,
            TitleBlockFamilyFilterValues = GetFiltrationComboBoxFilterList(vm.TitleBlockFamilyFilter),
            TitleBlockTypeName = vm.TitleBlockType?.Name,
            TitleBlockTypeFilterValues = GetFiltrationComboBoxFilterList(vm.TitleBlockTypeFilter),
            Views = vm.SheetComponents?.Select(CreateComponentData).ToList(),
        }, vm);
    }

    public SheetComponentData CreateComponentData(SheetComponentVM sheetComponentVM) {
        return sheetComponentVM switch {
            PlanViewVM vm => WithModuleData(new PlanViewData {
                ViewNameFormula = vm.ViewNameFormula,
                ViewFamilyTypeName = vm.ViewFamilyType?.Name,
                ViewFamilyTypeFilterValues = GetFiltrationComboBoxFilterList(vm.ViewFamilyTypeFilter),
                ViewTemplateName = vm.ViewTemplate?.Name,
                ViewTemplateFilterValues = GetFiltrationComboBoxFilterList(vm.ViewTemplateFilter),
                ViewTemplateRemoveAfterCreation = vm.ViewTemplateRemoveAfterCreation,
                ViewportTypeName = vm.ViewportType?.Name,
                ViewportTypeFilterValues = GetFiltrationComboBoxFilterList(vm.ViewportTypeFilter),
                ViewCount = vm.ViewCount,
                SelectedSelectElemParamName = vm.SelectedSelectElemParam?.ParamName,
                IsDuplicate = vm.IsDuplicate,
                ReferenceViewName = vm.ReferenceView?.Name,
                ReferenceViewFilterValues = GetFiltrationComboBoxFilterList(vm.ReferenceViewFilter),
            }, vm),

            CalloutViewVM vm => WithModuleData(new CalloutViewData {
                ViewNameFormula = vm.ViewNameFormula,
                ViewFamilyTypeName = vm.ViewFamilyType?.Name,
                ViewFamilyTypeFilterValues = GetFiltrationComboBoxFilterList(vm.ViewFamilyTypeFilter),
                ViewTemplateName = vm.ViewTemplate?.Name,
                ViewTemplateFilterValues = GetFiltrationComboBoxFilterList(vm.ViewTemplateFilter),
                ViewTemplateRemoveAfterCreation = vm.ViewTemplateRemoveAfterCreation,
                ViewportTypeName = vm.ViewportType?.Name,
                ViewportTypeFilterValues = GetFiltrationComboBoxFilterList(vm.ViewportTypeFilter),
                ViewCount = vm.ViewCount,
                SelectedSelectElemParamName = vm.SelectedSelectElemParam?.ParamName,
            }, vm),

            SectionViewVM vm => WithModuleData(new SectionViewData {
                ViewNameFormula = vm.ViewNameFormula,
                ViewFamilyTypeName = vm.ViewFamilyType?.Name,
                ViewFamilyTypeFilterValues = GetFiltrationComboBoxFilterList(vm.ViewFamilyTypeFilter),
                ViewTemplateName = vm.ViewTemplate?.Name,
                ViewTemplateFilterValues = GetFiltrationComboBoxFilterList(vm.ViewTemplateFilter),
                ViewTemplateRemoveAfterCreation = vm.ViewTemplateRemoveAfterCreation,
                ViewportTypeName = vm.ViewportType?.Name,
                ViewportTypeFilterValues = GetFiltrationComboBoxFilterList(vm.ViewportTypeFilter),
                ViewCount = vm.ViewCount,
                SelectedSelectElemParamName = vm.SelectedSelectElemParam?.ParamName,
            }, vm),

            ScheduleViewVM vm => WithModuleData(new ScheduleViewData {
                ReferenceViewName = vm.ReferenceSpec?.Name,
                ReferenceViewFilterValues = GetFiltrationComboBoxFilterList(vm.ReferenceSpecFilter),
                ViewNameFormula = vm.ViewNameFormula,
                ViewColumn = vm.ViewColumn,
                ViewCount = vm.ViewCount,
                ScheduleFilterList = GetScheduleFilterList(vm),
            }, vm),

            TextNoteVM vm => WithModuleData(new TextNoteData {
                TextFormula = vm.TextFormula,
                TextNoteTypeName = vm.TextNoteType?.Name,
                TextNoteTypeFilterValues = GetFiltrationComboBoxFilterList(vm.TextNoteTypeFilter),
                TextWidth = vm.TextWidth,
            }, vm),

            TypicalAnnotationVM vm => WithModuleData(new TypicalAnnotationData {
                IsFromFolder = vm.IsFromFolder,
                FamilyFolderPath = vm.FamilyFolderPath ?? string.Empty,
                FamilyName = vm.FamilyNameForConfig,
                TypeName = vm.TypeNameForConfig,
                FamilyNameFilterValues = GetFiltrationComboBoxFilterList(vm.FamilyNameFilter),
                TypeNameFilterValues = GetFiltrationComboBoxFilterList(vm.TypeNameFilter),
            }, vm),

            LegendViewVM vm => WithModuleData(new LegendViewData {
                ViewName = vm.LegendView?.Name,
                ViewFilterValues = GetFiltrationComboBoxFilterList(vm.LegendViewFilter),
                ViewportTypeName = vm.ViewportType?.Name,
                ViewportTypeFilterValues = GetFiltrationComboBoxFilterList(vm.ViewportTypeFilter),
            }, vm),

            _ => throw new NotSupportedException(
                $"{_localizationService.GetLocalizedString("MainViewModel.Type")} '{sheetComponentVM?.GetType().Name}' " +
                    $"{_localizationService.GetLocalizedString("MainViewModel.NotSupported")}")
        };
    }

    /// <summary>
    /// Заполняет общие данные модуля: галочку генерации, имя, комментарий и дополнительные параметры
    /// </summary>
    private static T WithModuleData<T>(T data, ModuleVM vm) where T : ParamContainerModuleData {
        data.IsModuleCheck = vm.IsModuleCheck;
        data.ModuleName = vm.ModuleName;
        data.ModuleComment = vm.ModuleComment;
        data.CustomParamsList = GetCustomParametersList(vm);
        return data;
    }

    private static CustomParametersListData GetCustomParametersList(BaseParamContainerVM vm) => new() {
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
                IsExcluding = r.IsExcluding,
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
            Type t when t == typeof(PlanViewVM) => new PlanViewData(),
            Type t when t == typeof(CalloutViewVM) => new CalloutViewData(),
            Type t when t == typeof(SectionViewVM) => new SectionViewData(),
            Type t when t == typeof(ScheduleViewVM) => new ScheduleViewData(),
            Type t when t == typeof(TextNoteVM) => new TextNoteData(),
            Type t when t == typeof(TypicalAnnotationVM) => new TypicalAnnotationData(),
            Type t when t == typeof(LegendViewVM) => new LegendViewData(),
            _ => throw new NotSupportedException($"{_localizationService.GetLocalizedString("MainViewModel.Type")} " +
                $"'{componentType?.Name}' {_localizationService.GetLocalizedString("MainViewModel.NotSupported")}")
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
            _ => throw new NotSupportedException(
                $"{_localizationService.GetLocalizedString("MainViewModel.ParameterType")} '{vm?.GetType().Name}' " +
                $"{_localizationService.GetLocalizedString("MainViewModel.NotSupported")}")
        };
    }

    public PluginParamData CreatePluginParamData(Type paramType) {
        return paramType switch {
            Type t when t == typeof(StringParamVM) => new StringParamData(),
            Type t when t == typeof(SelectElemParamVM) => new SelectElemParamData(),
            _ => throw new NotSupportedException(
                $"{_localizationService.GetLocalizedString("MainViewModel.ParameterType")} '{paramType?.Name}' " +
                $"{_localizationService.GetLocalizedString("MainViewModel.NotSupported")}")
        };
    }
}
