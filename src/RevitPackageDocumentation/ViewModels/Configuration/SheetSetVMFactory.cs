using System;
using System.Data;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.Models.ConfigSerializer;
using RevitPackageDocumentation.ViewModels.Configuration.CustomParameters;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
using RevitPackageDocumentation.ViewModels.Parameters;
using RevitPackageDocumentation.ViewModels.ScheduleFilters;

namespace RevitPackageDocumentation.ViewModels.Configuration;

internal interface ISheetSetVMFactory {
    SheetSetVM CreateSheetSetVM(SheetSetData data);
    SheetVM CreateSheetVM(SheetSetVM sheetSetVM, SheetData data);
    SheetComponentVM CreateComponentVM(SheetVM sheetVM, SheetComponentData data);
    PluginParamVM CreateParamVM(SheetSetVM sheetSetVM, PluginParamData data);
}

internal class SheetSetVMFactory : ISheetSetVMFactory {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly IMessageBoxService _messageBoxService;
    private readonly ISheetSetDataFactory _sheetSetDataFactory;
    private readonly StringParamSetService _stringParamSetService;

    public SheetSetVMFactory(
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        IMessageBoxService messageBoxService,
        ISheetSetDataFactory sheetSetDataFactory,
        StringParamSetService stringParamSetService) {

        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _messageBoxService = messageBoxService;
        _sheetSetDataFactory = sheetSetDataFactory;
        _stringParamSetService = stringParamSetService;
    }

    public SheetSetVM CreateSheetSetVM(SheetSetData data) {
        if(data == null)
            throw new ArgumentNullException(nameof(data));

        var sheetSetVM = new SheetSetVM(
            _revitRepository,
            _localizationService,
            _messageBoxService,
            this,
            _sheetSetDataFactory,
            _stringParamSetService) {
            Name = data.Name
        };

        foreach(var paramData in data.Params) {
            sheetSetVM.AddSheetSetParam(paramData);
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

        var sheetVM = new SheetVM(
            sheetSetVM,
            _revitRepository,
            _localizationService,
            _messageBoxService,
            this,
            _sheetSetDataFactory,
            _stringParamSetService) {

            IsModuleCheck = data.IsModuleCheck ?? false,
            ModuleName = data.ModuleName ?? string.Empty,
            ModuleComment = data.ModuleComment ?? string.Empty,
            ModuleCode = "123",

            SheetNameFormula = data.SheetNameFormula ?? string.Empty,
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
            StructuralPlanViewData data => CreateStructuralPlanViewVM(sheetVM, data),
            StructuralCalloutViewData data => CreateStructuralCalloutViewVM(sheetVM, data),
            SectionViewData data => CreateSectionViewVM(sheetVM, data),
            ScheduleViewData data => CreateScheduleViewVM(sheetVM, data),
            TextNoteData data => CreateTextNoteVM(sheetVM, data),
            TypicalAnnotationData data => CreateTypicalAnnotationVM(sheetVM, data),
            LegendViewData data => CreateLegendViewVM(sheetVM, data),
            _ => throw new NotSupportedException($"Тип '{sheetComponentData?.GetType().Name}' не поддерживается")
        };
    }

    private StructuralPlanViewVM CreateStructuralPlanViewVM(SheetVM sheetVM, StructuralPlanViewData data) {
        var sheetComponentVM = new StructuralPlanViewVM(
            sheetVM, _revitRepository, _localizationService, _stringParamSetService) {
            IsModuleCheck = data.IsModuleCheck ?? false,
            ModuleName = data.ModuleName ?? string.Empty,
            ModuleComment = data.ModuleComment ?? string.Empty,
            ModuleCode = "123",

            ViewNameFormula = data.ViewNameFormula ?? string.Empty,
            ViewFamilyType = _revitRepository.StructuralPlanViewTypes.FirstOrDefault(v => v.Name.Equals(data.ViewFamilyTypeName)),
            ViewTemplate = _revitRepository.PlanViewTemplates.FirstOrDefault(v => v.Name.Equals(data.ViewTemplateName)),
            ViewportType = _revitRepository.ViewportTypes.FirstOrDefault(v => v.Name.Equals(data.ViewportTypeName)),
            ViewCount = data.ViewCount ?? "1",
            SelectedSelectElemParam = sheetVM.SheetSet.SelectElemParams
                .FirstOrDefault(p => p.ParamName == data.SelectedSelectElemParamName && p is SelectElemParamVM)
        };

        // Добавляем список дополнительных параметров
        SetCustomParametersList(sheetComponentVM, data);
        return sheetComponentVM;
    }

    private StructuralCalloutViewVM CreateStructuralCalloutViewVM(SheetVM sheetVM, StructuralCalloutViewData data) {
        var sheetComponentVM = new StructuralCalloutViewVM(
            sheetVM, _revitRepository, _localizationService, _stringParamSetService) {
            IsModuleCheck = data.IsModuleCheck ?? false,
            ModuleName = data.ModuleName ?? string.Empty,
            ModuleComment = data.ModuleComment ?? string.Empty,
            ModuleCode = "05",

            ViewNameFormula = data.ViewNameFormula ?? string.Empty,
            ViewFamilyType = _revitRepository.StructuralPlanViewTypes.FirstOrDefault(v => v.Name.Equals(data.ViewFamilyTypeName)),
            ViewTemplate = _revitRepository.PlanViewTemplates.FirstOrDefault(v => v.Name.Equals(data.ViewTemplateName)),
            ViewportType = _revitRepository.ViewportTypes.FirstOrDefault(v => v.Name.Equals(data.ViewportTypeName)),
            ViewCount = data.ViewCount ?? "1",
            SelectedSelectElemParam = sheetVM.SheetSet.SelectElemParams
                .FirstOrDefault(p => p.ParamName == data.SelectedSelectElemParamName && p is SelectElemParamVM)
        };

        // Добавляем список дополнительных параметров
        SetCustomParametersList(sheetComponentVM, data);
        return sheetComponentVM;
    }

    private SectionViewVM CreateSectionViewVM(SheetVM sheetVM, SectionViewData data) {
        var sheetComponentVM = new SectionViewVM(
            sheetVM, _revitRepository, _localizationService, _stringParamSetService) {
            IsModuleCheck = data.IsModuleCheck ?? false,
            ModuleName = data.ModuleName ?? string.Empty,
            ModuleComment = data.ModuleComment ?? string.Empty,
            ModuleCode = "789",

            ViewNameFormula = data.ViewNameFormula ?? string.Empty,
            ViewFamilyType = _revitRepository.SectionViewTypes.FirstOrDefault(v => v.Name.Equals(data.ViewFamilyTypeName)),
            ViewTemplate = _revitRepository.SectionViewTemplates.FirstOrDefault(v => v.Name.Equals(data.ViewTemplateName)),
            ViewportType = _revitRepository.ViewportTypes.FirstOrDefault(v => v.Name.Equals(data.ViewportTypeName)),
            ViewCount = data.ViewCount ?? "1",
            SelectedSelectElemParam = sheetVM.SheetSet.SelectElemParams
                .FirstOrDefault(p => p.ParamName == data.SelectedSelectElemParamName && p is SelectElemParamVM)
        };

        // Добавляем список дополнительных параметров
        SetCustomParametersList(sheetComponentVM, data);
        return sheetComponentVM;
    }

    private ScheduleViewVM CreateScheduleViewVM(SheetVM sheetVM, ScheduleViewData data) {
        var referenceSpec = _revitRepository.GetSpecByName(data.ReferenceViewName);

        var scheduleViewVM = new ScheduleViewVM(
            sheetVM, _revitRepository, _localizationService, _stringParamSetService) {
            IsModuleCheck = data.IsModuleCheck ?? false,
            ModuleName = data.ModuleName ?? string.Empty,
            ModuleComment = data.ModuleComment ?? string.Empty,
            ModuleCode = "456",

            ReferenceSpec = referenceSpec,
            ViewNameFormula = data.ViewNameFormula ?? string.Empty,
            ViewColumn = data.ViewColumn ?? "1",
            ViewCount = data.ViewCount ?? "1",
        };

        // Добавляем список фильтров
        SetScheduleFilterList(scheduleViewVM, data, referenceSpec);

        // Добавляем список дополнительных параметров
        SetCustomParametersList(scheduleViewVM, data);
        return scheduleViewVM;
    }

    private TextNoteVM CreateTextNoteVM(SheetVM sheetVM, TextNoteData data) {
        var sheetComponentVM = new TextNoteVM(
            sheetVM, _revitRepository, _localizationService, _stringParamSetService) {
            IsModuleCheck = data.IsModuleCheck ?? false,
            ModuleName = data.ModuleName ?? string.Empty,
            ModuleComment = data.ModuleComment ?? string.Empty,
            ModuleCode = "01",

            TextFormula = data.TextFormula ?? string.Empty,
            TextNoteType = _revitRepository.TextNoteTypes.FirstOrDefault(v => v.Name.Equals(data.TextNoteTypeName)),
        };

        // Добавляем список дополнительных параметров
        SetCustomParametersList(sheetComponentVM, data);
        return sheetComponentVM;
    }

    private TypicalAnnotationVM CreateTypicalAnnotationVM(SheetVM sheetVM, TypicalAnnotationData data) {
        var annotationFamily = _revitRepository.GenericAnnotationFamilies?.FirstOrDefault(v => v.Name.Equals(data.AnnotationFamilyName));

        var annotationTypes = annotationFamily
                    ?.GetFamilySymbolIds()
                    ?.Select(id => _revitRepository.Document.GetElement(id) as AnnotationSymbolType)
                    ?.ToList();

        var annotationType = annotationTypes
                    ?.FirstOrDefault(v => v.Name.Equals(data.AnnotationTypeName));

        var sheetComponentVM = new TypicalAnnotationVM(
            sheetVM, _revitRepository, _localizationService, _stringParamSetService) {
            IsModuleCheck = data.IsModuleCheck ?? false,
            ModuleName = data.ModuleName ?? string.Empty,
            ModuleComment = data.ModuleComment ?? string.Empty,
            ModuleCode = "02",

            AnnotationTypes = annotationTypes,
            AnnotationFamily = annotationFamily,
            AnnotationType = annotationType
        };

        // Добавляем список дополнительных параметров
        SetCustomParametersList(sheetComponentVM, data);
        return sheetComponentVM;
    }

    private LegendViewVM CreateLegendViewVM(SheetVM sheetVM, LegendViewData data) {
        var sheetComponentVM = new LegendViewVM(
            sheetVM, _revitRepository, _localizationService, _stringParamSetService) {
            IsModuleCheck = data.IsModuleCheck ?? false,
            ModuleName = data.ModuleName ?? string.Empty,
            ModuleComment = data.ModuleComment ?? string.Empty,
            ModuleCode = "03",

            LegendView = _revitRepository.LegendsInProject.FirstOrDefault(v => v.Name.Equals(data.ViewName)),
            ViewportType = _revitRepository.ViewportTypes.FirstOrDefault(v => v.Name.Equals(data.ViewportTypeName)),
        };

        // Добавляем список дополнительных параметров
        SetCustomParametersList(sheetComponentVM, data);
        return sheetComponentVM;
    }

    /// <summary>
    /// Добавляет список дополнительных параметров
    /// </summary>
    private void SetCustomParametersList(SheetComponentVM sheetComponentVM, SheetComponentData data) {
        // Добавляем список дополнительных параметров
        var customParamsList = new CustomParametersListVM(sheetComponentVM, _stringParamSetService);
        foreach(var paramData in data.CustomParamsList?.Params ?? []) {
            var paramVM = new CustomParameterVM(customParamsList, _stringParamSetService) {
                ParamName = paramData.ParamName ?? string.Empty,
                ParamValueFormula = paramData.ParamValueFormula ?? string.Empty,
            };
            customParamsList.Params.Add(paramVM);
        }
        sheetComponentVM.CustomParamsList = customParamsList;
    }

    /// <summary>
    /// Добавляем список фильтров
    /// </summary>
    private void SetScheduleFilterList(SheetComponentVM sheetComponentVM, SheetComponentData data, ViewSchedule referenceSpec) {
        // Метод предназначен только для модуля спецификаций
        if(sheetComponentVM is not ScheduleViewVM scheduleViewVM || data is not ScheduleViewData scheduleViewData) { return; }

        var scheduleFilterList = new ScheduleFilterListVM(scheduleViewVM);
        foreach(var ruleData in scheduleViewData.ScheduleFilterList?.ScheduleFilterRules ?? []) {
            var ruleVM = new ScheduleFilterRuleVM(scheduleFilterList) {
                SelectedSpecFieldName = ruleData.FieldName,
                FilterValue = ruleData.FilterValue ?? string.Empty,
                SelectedFilterType = _revitRepository.FilterTypes.FirstOrDefault(t => t.FilterType == ruleData.FilterType)
            };

            ruleVM.SetSchedule(referenceSpec);
            scheduleFilterList.ScheduleFilterRules.Add(ruleVM);
        }
        scheduleViewVM.ScheduleFilterList = scheduleFilterList;
    }

    public PluginParamVM CreateParamVM(SheetSetVM sheetSetVM, PluginParamData paramData) {
        return paramData switch {
            StringParamData data => new StringParamVM(sheetSetVM, data.ParamName, data.ParamComment, data.StringValue),
            SelectElemParamData data => new SelectElemParamVM(sheetSetVM, data.ParamName, data.ParamComment),
            _ => throw new NotSupportedException($"Тип '{paramData?.GetType().Name}' не поддерживается")
        };
    }
}
