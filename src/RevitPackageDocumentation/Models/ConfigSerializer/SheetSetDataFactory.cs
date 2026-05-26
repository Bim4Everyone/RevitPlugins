using System;
using System.Linq;

using RevitPackageDocumentation.ViewModels.Configuration;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
using RevitPackageDocumentation.ViewModels.Parameters;

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
            Params = vm.Params?.Select(CreatePluginParamData).ToList()
        };
    }

    public SheetData CreateSheetData(SheetVM vm) {
        if(vm == null)
            return new SheetData();

        return new SheetData {
            IsModuleCheck = vm.IsModuleCheck,
            ModuleName = vm.ModuleName,
            ModuleComment = vm.ModuleComment,

            SheetName = vm.SheetName,
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

                ViewName = vm.ViewName,
                ViewFamilyTypeName = vm.ViewFamilyType?.Name,
                ViewTemplateName = vm.ViewTemplate?.Name,
                ViewportTypeName = vm.ViewportType?.Name,
                ViewCount = vm.ViewCount,
            },

            StructuralCalloutViewVM vm => new StructuralCalloutViewData {
                IsModuleCheck = vm.IsModuleCheck,
                ModuleName = vm.ModuleName,
                ModuleComment = vm.ModuleComment,

                ViewName = vm.ViewName,
                ViewFamilyTypeName = vm.ViewFamilyType?.Name,
                ViewTemplateName = vm.ViewTemplate?.Name,
                ViewportTypeName = vm.ViewportType?.Name,
                ViewCount = vm.ViewCount,
            },

            SectionViewVM vm => new SectionViewData {
                IsModuleCheck = vm.IsModuleCheck,
                ModuleName = vm.ModuleName,
                ModuleComment = vm.ModuleComment,

                ViewName = vm.ViewName,
                ViewFamilyTypeName = vm.ViewFamilyType?.Name,
                ViewTemplateName = vm.ViewTemplate?.Name,
                ViewportTypeName = vm.ViewportType?.Name,
                ViewCount = vm.ViewCount,
            },

            ScheduleViewVM vm => new ScheduleViewData {
                IsModuleCheck = vm.IsModuleCheck,
                ModuleName = vm.ModuleName,
                ModuleComment = vm.ModuleComment,

                ReferenceViewName = vm.ReferenceSpec.Name,
                ViewName = vm.ViewName,
                ViewColumn = vm.ViewColumn,
                ViewCount = vm.ViewCount,
                ScheduleFilterList = new ScheduleFilterListData() {
                    ScheduleFilterRules = vm
                        .ScheduleFilterList
                        .ScheduleFilterRules
                        .Select(r => new ScheduleFilterRuleData() {
                            FieldName = r.SelectedSpecFieldName,
                            FilterType = r.SelectedFilterType,
                            FilterValue = r.FilterValue
                        })
                        .ToList()
                }
            },

            TextNoteVM vm => new TextNoteData {
                IsModuleCheck = vm.IsModuleCheck,
                ModuleName = vm.ModuleName,
                ModuleComment = vm.ModuleComment,

                Text = vm.Text,
                TextNoteTypeName = vm.TextNoteType?.Name,
            },

            TypicalAnnotationVM vm => new TypicalAnnotationData {
                IsModuleCheck = vm.IsModuleCheck,
                ModuleName = vm.ModuleName,
                ModuleComment = vm.ModuleComment,

                AnnotationFamilyName = vm.AnnotationFamily?.Name,
                AnnotationTypeName = vm.AnnotationType?.Name,
            },

            LegendViewVM vm => new LegendViewData {
                IsModuleCheck = vm.IsModuleCheck,
                ModuleName = vm.ModuleName,
                ModuleComment = vm.ModuleComment,

                ViewName = vm.LegendView?.Name,
            },

            _ => throw new NotSupportedException($"Тип '{sheetComponentVM?.GetType().Name}' не поддерживается")
        };
    }

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
