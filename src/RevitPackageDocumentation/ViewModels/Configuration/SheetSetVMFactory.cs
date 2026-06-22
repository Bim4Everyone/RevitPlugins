using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.Models.ConfigSerializer;
using RevitPackageDocumentation.ViewModels.Configuration.CustomParameters;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;
using RevitPackageDocumentation.ViewModels.ScheduleFilters;

namespace RevitPackageDocumentation.ViewModels.Configuration;

internal interface ISheetSetVMFactory {
    SheetSetVM CreateSheetSetVM(SheetSetData data);
    SheetVM CreateSheetVM(SheetSetVM sheetSetVM, SheetData data);
    SheetComponentVM CreateComponentVM(SheetSetVM sheetSetVM, SheetVM sheetVM, SheetComponentData data);
    PluginParamVM CreateParamVM(SheetSetParametersListVM sheetSetParamsList, PluginParamData data);
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

        var sheetSetParams = new SheetSetParametersListVM(
            sheetSetVM,
            _messageBoxService,
            this,
            _sheetSetDataFactory,
            _localizationService);
        foreach(var paramData in data.Params) {
            sheetSetParams.AddSheetSetParam(paramData);
            sheetSetVM.SheetSetParams = sheetSetParams;
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
            _revitRepository,
            _stringParamSetService,
            sheetSetVM.SheetSetParams.Params,
            sheetSetVM,
            _localizationService,
            _messageBoxService,
            this,
            _sheetSetDataFactory) {

            IsModuleCheck = data.IsModuleCheck ?? false,
            ModuleName = data.ModuleName ?? string.Empty,
            ModuleComment = data.ModuleComment ?? string.Empty,
            ModuleCode = "123",

            SheetNameFormula = data.SheetNameFormula ?? string.Empty,
            SheetName = data.SheetNameFormula ?? string.Empty,
            SheetSize = data.SheetSize ?? string.Empty,
            SheetCoefficient = data.SheetCoefficient ?? string.Empty,

            TitleBlockFamily = titleBlockFamily,
            TitleBlockTypes = titleBlockTypes,
            TitleBlockType = titleBlockType,
        };
        // Добавляем список дополнительных параметров
        SetCustomParametersList(sheetVM, data, sheetSetVM.SheetSetParams.Params);

        foreach(var componentData in data.Views) {
            var componentVM = CreateComponentVM(sheetSetVM, sheetVM, componentData);
            sheetVM.SheetComponents.Add(componentVM);
        }
        return sheetVM;
    }

    public SheetComponentVM CreateComponentVM(SheetSetVM sheetSetVM, SheetVM sheetVM, SheetComponentData sheetComponentData) {
        return sheetComponentData switch {
            StructuralPlanViewData data => CreateStructuralPlanViewVM(sheetSetVM, sheetVM, data),
            StructuralCalloutViewData data => CreateStructuralCalloutViewVM(sheetSetVM, sheetVM, data),
            SectionViewData data => CreateSectionViewVM(sheetSetVM, sheetVM, data),
            ScheduleViewData data => CreateScheduleViewVM(sheetSetVM, sheetVM, data),
            TextNoteData data => CreateTextNoteVM(sheetSetVM, sheetVM, data),
            TypicalAnnotationData data => CreateTypicalAnnotationVM(sheetSetVM, sheetVM, data),
            LegendViewData data => CreateLegendViewVM(sheetSetVM, sheetVM, data),
            _ => throw new NotSupportedException($"Тип '{sheetComponentData?.GetType().Name}' не поддерживается")
        };
    }

    private StructuralPlanViewVM CreateStructuralPlanViewVM(SheetSetVM sheetSetVM, SheetVM sheetVM, StructuralPlanViewData data) {
        var sheetComponentVM = new StructuralPlanViewVM(
            _revitRepository, _stringParamSetService, sheetSetVM.SheetSetParams.Params, sheetVM, _localizationService) {
            IsModuleCheck = data.IsModuleCheck ?? false,
            ModuleName = data.ModuleName ?? string.Empty,
            ModuleComment = data.ModuleComment ?? string.Empty,
            ModuleCode = "123",

            ViewNameFormula = data.ViewNameFormula ?? string.Empty,
            ViewName = data.ViewNameFormula ?? string.Empty,
            ViewFamilyType = _revitRepository.StructuralPlanViewTypes.FirstOrDefault(v => v.Name.Equals(data.ViewFamilyTypeName)),
            ViewTemplate = _revitRepository.PlanViewTemplates.FirstOrDefault(v => v.Name.Equals(data.ViewTemplateName)),
            ViewportType = _revitRepository.ViewportTypes.FirstOrDefault(v => v.Name.Equals(data.ViewportTypeName)),
            ViewCount = data.ViewCount ?? "1",
            SelectedSelectElemParam = sheetVM.SheetSet.SheetSetParams.SelectElemParams
                .FirstOrDefault(p => p.ParamName == data.SelectedSelectElemParamName && p is SelectElemParamVM)
        };

        // Добавляем список дополнительных параметров
        SetCustomParametersList(sheetComponentVM, data, sheetSetVM.SheetSetParams.Params);
        return sheetComponentVM;
    }

    private StructuralCalloutViewVM CreateStructuralCalloutViewVM(SheetSetVM sheetSetVM, SheetVM sheetVM, StructuralCalloutViewData data) {
        var sheetComponentVM = new StructuralCalloutViewVM(
            _revitRepository, _stringParamSetService, sheetSetVM.SheetSetParams.Params, sheetVM, _localizationService) {
            IsModuleCheck = data.IsModuleCheck ?? false,
            ModuleName = data.ModuleName ?? string.Empty,
            ModuleComment = data.ModuleComment ?? string.Empty,
            ModuleCode = "05",

            ViewNameFormula = data.ViewNameFormula ?? string.Empty,
            ViewName = data.ViewNameFormula ?? string.Empty,
            ViewFamilyType = _revitRepository.StructuralPlanViewTypes.FirstOrDefault(v => v.Name.Equals(data.ViewFamilyTypeName)),
            ViewTemplate = _revitRepository.PlanViewTemplates.FirstOrDefault(v => v.Name.Equals(data.ViewTemplateName)),
            ViewportType = _revitRepository.ViewportTypes.FirstOrDefault(v => v.Name.Equals(data.ViewportTypeName)),
            ViewCount = data.ViewCount ?? "1",
            SelectedSelectElemParam = sheetVM.SheetSet.SheetSetParams.SelectElemParams
                .FirstOrDefault(p => p.ParamName == data.SelectedSelectElemParamName && p is SelectElemParamVM)
        };

        // Добавляем список дополнительных параметров
        SetCustomParametersList(sheetComponentVM, data, sheetSetVM.SheetSetParams.Params);
        return sheetComponentVM;
    }

    private SectionViewVM CreateSectionViewVM(SheetSetVM sheetSetVM, SheetVM sheetVM, SectionViewData data) {
        var sheetComponentVM = new SectionViewVM(
            _revitRepository, _stringParamSetService, sheetSetVM.SheetSetParams.Params, sheetVM, _localizationService) {
            IsModuleCheck = data.IsModuleCheck ?? false,
            ModuleName = data.ModuleName ?? string.Empty,
            ModuleComment = data.ModuleComment ?? string.Empty,
            ModuleCode = "789",

            ViewNameFormula = data.ViewNameFormula ?? string.Empty,
            ViewName = data.ViewNameFormula ?? string.Empty,
            ViewFamilyType = _revitRepository.SectionViewTypes.FirstOrDefault(v => v.Name.Equals(data.ViewFamilyTypeName)),
            ViewTemplate = _revitRepository.SectionViewTemplates.FirstOrDefault(v => v.Name.Equals(data.ViewTemplateName)),
            ViewportType = _revitRepository.ViewportTypes.FirstOrDefault(v => v.Name.Equals(data.ViewportTypeName)),
            ViewCount = data.ViewCount ?? "1",
            SelectedSelectElemParam = sheetVM.SheetSet.SheetSetParams.SelectElemParams
                .FirstOrDefault(p => p.ParamName == data.SelectedSelectElemParamName && p is SelectElemParamVM)
        };

        // Добавляем список дополнительных параметров
        SetCustomParametersList(sheetComponentVM, data, sheetSetVM.SheetSetParams.Params);
        return sheetComponentVM;
    }

    private ScheduleViewVM CreateScheduleViewVM(SheetSetVM sheetSetVM, SheetVM sheetVM, ScheduleViewData data) {
        var referenceSpec = _revitRepository.GetSpecByName(data.ReferenceViewName);

        var scheduleViewVM = new ScheduleViewVM(
            _revitRepository, _stringParamSetService, sheetSetVM.SheetSetParams.Params, sheetVM, _localizationService) {
            IsModuleCheck = data.IsModuleCheck ?? false,
            ModuleName = data.ModuleName ?? string.Empty,
            ModuleComment = data.ModuleComment ?? string.Empty,
            ModuleCode = "456",

            ReferenceSpec = referenceSpec,
            ViewNameFormula = data.ViewNameFormula ?? string.Empty,
            ViewName = data.ViewNameFormula ?? string.Empty,
            ViewColumn = data.ViewColumn ?? "1",
            ViewCount = data.ViewCount ?? "1",
        };

        // Добавляем список фильтров
        SetScheduleFilterList(scheduleViewVM, data, referenceSpec);

        // Добавляем список дополнительных параметров
        SetCustomParametersList(scheduleViewVM, data, sheetSetVM.SheetSetParams.Params);
        return scheduleViewVM;
    }

    private TextNoteVM CreateTextNoteVM(SheetSetVM sheetSetVM, SheetVM sheetVM, TextNoteData data) {
        var sheetComponentVM = new TextNoteVM(
            _revitRepository, _stringParamSetService, sheetSetVM.SheetSetParams.Params, sheetVM, _localizationService) {
            IsModuleCheck = data.IsModuleCheck ?? false,
            ModuleName = data.ModuleName ?? string.Empty,
            ModuleComment = data.ModuleComment ?? string.Empty,
            ModuleCode = "01",

            TextFormula = data.TextFormula ?? string.Empty,
            Text = data.TextFormula ?? string.Empty,
            TextNoteType = _revitRepository.TextNoteTypes.FirstOrDefault(v => v.Name.Equals(data.TextNoteTypeName)),
        };

        // Добавляем список дополнительных параметров
        SetCustomParametersList(sheetComponentVM, data, sheetSetVM.SheetSetParams.Params);
        return sheetComponentVM;
    }

    private TypicalAnnotationVM CreateTypicalAnnotationVM(SheetSetVM sheetSetVM, SheetVM sheetVM, TypicalAnnotationData data) {
        var annotationFamily = _revitRepository.GenericAnnotationFamilies?.FirstOrDefault(v => v.Name.Equals(data.AnnotationFamilyName));

        var annotationTypes = annotationFamily
                    ?.GetFamilySymbolIds()
                    ?.Select(id => _revitRepository.Document.GetElement(id) as AnnotationSymbolType)
                    ?.ToList();

        var annotationType = annotationTypes
                    ?.FirstOrDefault(v => v.Name.Equals(data.AnnotationTypeName));

        var sheetComponentVM = new TypicalAnnotationVM(
            _revitRepository, _stringParamSetService, sheetSetVM.SheetSetParams.Params, sheetVM, _localizationService) {
            IsModuleCheck = data.IsModuleCheck ?? false,
            ModuleName = data.ModuleName ?? string.Empty,
            ModuleComment = data.ModuleComment ?? string.Empty,
            ModuleCode = "02",

            AnnotationTypes = annotationTypes,
            AnnotationFamily = annotationFamily,
            AnnotationType = annotationType
        };

        // Добавляем список дополнительных параметров
        SetCustomParametersList(sheetComponentVM, data, sheetSetVM.SheetSetParams.Params);
        return sheetComponentVM;
    }

    private LegendViewVM CreateLegendViewVM(SheetSetVM sheetSetVM, SheetVM sheetVM, LegendViewData data) {
        var sheetComponentVM = new LegendViewVM(
            _revitRepository, _stringParamSetService, sheetSetVM.SheetSetParams.Params, sheetVM, _localizationService) {
            IsModuleCheck = data.IsModuleCheck ?? false,
            ModuleName = data.ModuleName ?? string.Empty,
            ModuleComment = data.ModuleComment ?? string.Empty,
            ModuleCode = "03",

            LegendView = _revitRepository.LegendsInProject.FirstOrDefault(v => v.Name.Equals(data.ViewName)),
            ViewportType = _revitRepository.ViewportTypes.FirstOrDefault(v => v.Name.Equals(data.ViewportTypeName)),
        };

        // Добавляем список дополнительных параметров
        SetCustomParametersList(sheetComponentVM, data, sheetSetVM.SheetSetParams.Params);
        return sheetComponentVM;
    }

    /// <summary>
    /// Добавляет список дополнительных параметров
    /// </summary>
    private void SetCustomParametersList(
        BaseParamContainerVM baseParamContainer,
        ParamContainerModuleData data,
        ObservableCollection<PluginParamVM> sheetSetParams) {
        // Добавляем список дополнительных параметров
        var customParamsList = new CustomParametersListVM(sheetSetParams, _stringParamSetService);
        foreach(var paramData in data.CustomParamsList?.Params ?? []) {
            var paramVM = new CustomParameterVM(customParamsList, _stringParamSetService) {
                ParamName = paramData.ParamName ?? string.Empty,
                ParamValueFormula = paramData.ParamValueFormula ?? string.Empty,
                ParamValue = paramData.ParamValueFormula ?? string.Empty,
            };
            customParamsList.Params.Add(paramVM);
        }
        baseParamContainer.CustomParamsList = customParamsList;
    }

    /// <summary>
    /// Добавляем список фильтров
    /// </summary>
    private void SetScheduleFilterList(SheetComponentVM sheetComponentVM, SheetComponentData data, ViewSchedule referenceSpec) {
        // Метод предназначен только для модуля спецификаций
        if(sheetComponentVM is not ScheduleViewVM scheduleViewVM || data is not ScheduleViewData scheduleViewData) { return; }

        var scheduleFilterList = new ScheduleFilterListVM(scheduleViewVM, _stringParamSetService);
        foreach(var ruleData in scheduleViewData.ScheduleFilterList?.ScheduleFilterRules ?? []) {
            var ruleVM = new ScheduleFilterRuleVM(scheduleFilterList, _stringParamSetService) {
                SelectedSpecFieldName = ruleData.FieldName,
                SelectedFilterType = _revitRepository.FilterTypes.FirstOrDefault(t => t.FilterType == ruleData.FilterType),
                FilterValueFormula = ruleData.FilterValueFormula ?? string.Empty,
                FilterValue = ruleData.FilterValueFormula ?? string.Empty,
            };
            scheduleFilterList.ScheduleFilterRules.Add(ruleVM);
        }
        scheduleFilterList.SetScheduleToRemember(referenceSpec);
        scheduleViewVM.ScheduleFilterList = scheduleFilterList;
    }

    public PluginParamVM CreateParamVM(SheetSetParametersListVM sheetSetParamsList, PluginParamData paramData) {
        return paramData switch {
            StringParamData data => new StringParamVM(sheetSetParamsList, data.ParamName, data.ParamComment, data.StringValue),
            SelectElemParamData data => new SelectElemParamVM(sheetSetParamsList, data.ParamName, data.ParamComment),
            _ => throw new NotSupportedException($"Тип '{paramData?.GetType().Name}' не поддерживается")
        };
    }
}
