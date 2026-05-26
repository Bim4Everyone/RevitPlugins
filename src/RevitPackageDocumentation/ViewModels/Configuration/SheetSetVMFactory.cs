using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.Models.ConfigSerializer;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
using RevitPackageDocumentation.ViewModels.Parameters;
using RevitPackageDocumentation.ViewModels.ScheduleFilters;

namespace RevitPackageDocumentation.ViewModels.Configuration;

internal interface ISheetSetVMFactory {
    SheetSetVM CreateSheetSetVM(SheetSetData data);
    SheetVM CreateSheetVM(SheetSetVM sheetSetVM, SheetData data);
    SheetComponentVM CreateComponentVM(SheetVM sheetVM, SheetComponentData data);
    PluginParamVM CreateParamVM(PluginParamData data);
}

internal class SheetSetVMFactory : ISheetSetVMFactory {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly IMessageBoxService _messageBoxService;
    private readonly ISheetSetDataFactory _sheetSetDataFactory;

    public SheetSetVMFactory(
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        IMessageBoxService messageBoxService,
        ISheetSetDataFactory sheetSetDataFactory) {

        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _messageBoxService = messageBoxService;
        _sheetSetDataFactory = sheetSetDataFactory;
    }

    public SheetSetVM CreateSheetSetVM(SheetSetData data) {
        if(data == null)
            throw new ArgumentNullException(nameof(data));

        var sheetSetVM = new SheetSetVM(_revitRepository, _localizationService, _messageBoxService, this, _sheetSetDataFactory) {
            Name = data.Name
        };

        foreach(var paramData in data.Params) {
            var paramVM = CreateParamVM(paramData);
            sheetSetVM.Params.Add(paramVM);
        }

        foreach(var sheetData in data.Sheets) {
            var sheetVM = CreateSheetVM(sheetSetVM, sheetData);
            sheetSetVM.SheetList.Add(sheetVM);
        }

        return sheetSetVM;
    }

    public SheetVM CreateSheetVM(SheetSetVM sheetSetVM, SheetData data) {
        if(data == null)
            throw new ArgumentNullException(nameof(data));

        var titleBlockFamily = _revitRepository.TitleBlockFamilies?.FirstOrDefault(v => v.Name.Equals(data.TitleBlockFamilyName));

        var titleBlockTypes = titleBlockFamily
            ?.GetFamilySymbolIds()
            ?.Select(id => _revitRepository.Document.GetElement(id) as FamilySymbol)
            ?.ToList();

        var titleBlockType = titleBlockTypes?.FirstOrDefault(v => v.Name.Equals(data.TitleBlockTypeName));

        var sheetVM = new SheetVM(sheetSetVM, _revitRepository, _localizationService, _messageBoxService, this, _sheetSetDataFactory) {
            IsModuleCheck = data.IsModuleCheck ?? false,
            ModuleName = data.ModuleName ?? string.Empty,
            ModuleComment = data.ModuleComment ?? string.Empty,
            ModuleCode = "123",

            SheetName = data.SheetName ?? string.Empty,
            SheetSize = data.SheetSize ?? string.Empty,
            SheetCoefficient = data.SheetCoefficient ?? string.Empty,

            TitleBlockFamily = titleBlockFamily,
            TitleBlockTypes = titleBlockTypes,
            TitleBlockType = titleBlockType,
        };

        foreach(var componentData in data.Views) {
            var componentVM = CreateComponentVM(sheetVM, componentData);
            sheetVM.SheetComponents.Add(componentVM);
        }

        return sheetVM;
    }

    public SheetComponentVM CreateComponentVM(SheetVM sheetVM, SheetComponentData sheetComponentData) {
        return sheetComponentData switch {
            StructuralPlanViewData data => new StructuralPlanViewVM(sheetVM, _revitRepository, _localizationService) {
                IsModuleCheck = data.IsModuleCheck ?? false,
                ModuleName = data.ModuleName ?? string.Empty,
                ModuleComment = data.ModuleComment ?? string.Empty,
                ModuleCode = "123",

                ViewName = data.ViewName ?? string.Empty,
                ViewFamilyType = _revitRepository.StructuralPlanViewTypes.FirstOrDefault(v => v.Name.Equals(data.ViewFamilyTypeName)),
                ViewTemplate = _revitRepository.PlanViewTemplates.FirstOrDefault(v => v.Name.Equals(data.ViewTemplateName)),
                ViewportType = _revitRepository.ViewportTypes.FirstOrDefault(v => v.Name.Equals(data.ViewportTypeName)),
                ViewCount = data.ViewCount ?? "1",
            },

            StructuralCalloutViewData data => new StructuralCalloutViewVM(sheetVM, _localizationService) {
                IsModuleCheck = data.IsModuleCheck ?? false,
                ModuleName = data.ModuleName ?? string.Empty,
                ModuleComment = data.ModuleComment ?? string.Empty,
                ModuleCode = "05",

                ViewName = data.ViewName ?? string.Empty,
                ViewFamilyType = _revitRepository.StructuralPlanViewTypes.FirstOrDefault(v => v.Name.Equals(data.ViewFamilyTypeName)),
                ViewTemplate = _revitRepository.PlanViewTemplates.FirstOrDefault(v => v.Name.Equals(data.ViewTemplateName)),
                ViewportType = _revitRepository.ViewportTypes.FirstOrDefault(v => v.Name.Equals(data.ViewportTypeName)),
                ViewCount = data.ViewCount ?? "1",
            },

            SectionViewData data => new SectionViewVM(sheetVM, _revitRepository, _localizationService) {
                IsModuleCheck = data.IsModuleCheck ?? false,
                ModuleName = data.ModuleName ?? string.Empty,
                ModuleComment = data.ModuleComment ?? string.Empty,
                ModuleCode = "789",

                ViewName = data.ViewName ?? string.Empty,
                ViewFamilyType = _revitRepository.SectionViewTypes.FirstOrDefault(v => v.Name.Equals(data.ViewFamilyTypeName)),
                ViewTemplate = _revitRepository.SectionViewTemplates.FirstOrDefault(v => v.Name.Equals(data.ViewTemplateName)),
                ViewportType = _revitRepository.ViewportTypes.FirstOrDefault(v => v.Name.Equals(data.ViewportTypeName)),
                ViewCount = data.ViewCount ?? "1",
            },

            ScheduleViewData data => CreateScheduleViewVM(sheetVM, data),

            TextNoteData data => new TextNoteVM(sheetVM, _localizationService) {
                IsModuleCheck = data.IsModuleCheck ?? false,
                ModuleName = data.ModuleName ?? string.Empty,
                ModuleComment = data.ModuleComment ?? string.Empty,
                ModuleCode = "01",

                Text = data.Text ?? string.Empty,
                TextNoteType = _revitRepository.TextNoteTypes.FirstOrDefault(v => v.Name.Equals(data.TextNoteTypeName)),
            },

            TypicalAnnotationData data => CreateTypicalAnnotationVM(sheetVM, data),

            LegendViewData data => new LegendViewVM(sheetVM, _localizationService) {
                IsModuleCheck = data.IsModuleCheck ?? false,
                ModuleName = data.ModuleName ?? string.Empty,
                ModuleComment = data.ModuleComment ?? string.Empty,
                ModuleCode = "03",

                LegendView = _revitRepository.LegendsInProject.FirstOrDefault(v => v.Name.Equals(data.ViewName)),
            },

            _ => throw new NotSupportedException($"Тип '{sheetComponentData?.GetType().Name}' не поддерживается")
        };
    }


    private ScheduleViewVM CreateScheduleViewVM(SheetVM sheetVM, ScheduleViewData data) {
        var referenceSpec = _revitRepository.GetSpecByName(data.ReferenceViewName);

        var scheduleViewVM = new ScheduleViewVM(sheetVM, _revitRepository, _localizationService) {
            IsModuleCheck = data.IsModuleCheck ?? false,
            ModuleName = data.ModuleName ?? string.Empty,
            ModuleComment = data.ModuleComment ?? string.Empty,
            ModuleCode = "456",

            ReferenceSpec = referenceSpec,
            ViewName = data.ViewName ?? string.Empty,
            ViewColumn = data.ViewColumn ?? "1",
            ViewCount = data.ViewCount ?? "1",
        };

        var scheduleFilterList = new ScheduleFilterListVM(scheduleViewVM);

        foreach(var ruleData in data.ScheduleFilterList.ScheduleFilterRules) {
            var ruleVM = new ScheduleFilterRuleVM(scheduleFilterList) {
                SelectedSpecFieldName = ruleData.FieldName,
                FilterValue = ruleData.FilterValue,
                SelectedFilterType = _revitRepository.FilterTypes.FirstOrDefault(t => t.FilterType == ruleData.FilterType)
            };

            ruleVM.SetSchedule(referenceSpec);
            scheduleFilterList.ScheduleFilterRules.Add(ruleVM);
        }
        scheduleViewVM.ScheduleFilterList = scheduleFilterList;
        return scheduleViewVM;
    }


    private TypicalAnnotationVM CreateTypicalAnnotationVM(SheetVM sheetVM, TypicalAnnotationData data) {

        var annotationFamily = _revitRepository.GenericAnnotationFamilies?.FirstOrDefault(v => v.Name.Equals(data.AnnotationFamilyName));

        var annotationTypes = annotationFamily
                    ?.GetFamilySymbolIds()
                    ?.Select(id => _revitRepository.Document.GetElement(id) as AnnotationSymbolType)
                    ?.ToList();

        var annotationType = annotationTypes
                    ?.FirstOrDefault(v => v.Name.Equals(data.AnnotationTypeName));

        return new TypicalAnnotationVM(sheetVM, _revitRepository, _localizationService) {
            IsModuleCheck = data.IsModuleCheck ?? false,
            ModuleName = data.ModuleName ?? string.Empty,
            ModuleComment = data.ModuleComment ?? string.Empty,
            ModuleCode = "02",

            AnnotationTypes = annotationTypes,
            AnnotationFamily = annotationFamily,
            AnnotationType = annotationType
        };
    }

    public PluginParamVM CreateParamVM(PluginParamData paramData) {
        return paramData switch {
            StringParamData data => new StringParamVM() {
                ParamName = data.ParamName ?? string.Empty,
                ParamComment = data.ParamComment ?? string.Empty,
                StringValue = data.StringValue ?? string.Empty,
            },

            SelectElemParamData data => new SelectElemParamVM() {
                ParamName = data.ParamName ?? string.Empty,
                ParamComment = data.ParamComment ?? string.Empty,
            },

            _ => throw new NotSupportedException($"Тип '{paramData?.GetType().Name}' не поддерживается")
        };
    }
}
