using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Configuration.CustomParameters;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;
using RevitPackageDocumentation.ViewModels.FiltrationComboBoxVMs;
using RevitPackageDocumentation.ViewModels.Validation;
using RevitPackageDocumentation.ViewModels.Validation.Attributes;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet;
internal abstract class BaseParamContainerVM : ValidatableVM {
    private CustomParametersListVM _customParamsList;
    private readonly StringParamSetService _strParamSetService;

    protected BaseParamContainerVM(
        RevitRepository repository,
        StringParamSetService stringParamSetService,
        ObservableCollection<PluginParamVM> sheetSetParams,
        ILocalizationService localizationService)
        : base(localizationService) {

        Repository = repository;
        _strParamSetService = stringParamSetService;
        SheetSetParams = sheetSetParams;
        CustomParamsList = new CustomParametersListVM(SheetSetParams, _strParamSetService, localizationService);

        PropUpdateByFormulaCommand = RelayCommand.Create<string>(PropUpdateByFormula);
    }

    public ICommand PropUpdateByFormulaCommand { get; }

    public ObservableCollection<PluginParamVM> SheetSetParams { get; }
    public RevitRepository Repository { get; }

    [ChildHasErrors(ErrorMessage = "Validation.ErrorInCustomParams")]
    public CustomParametersListVM CustomParamsList {
        get => _customParamsList;
        set => RaiseAndSetIfChanged(ref _customParamsList, value);
    }

    private void PropUpdateByFormula(string formulaPropertyName) {
        _strParamSetService.Set(this, formulaPropertyName, SheetSetParams);
    }

    /// <summary>
    /// В случае изменения имени параметра конфигурации нужно обновить свойства компонента листа, а также 
    /// его дополнительные параметры
    /// </summary>
    public void UpdateDueParamNameChange() {
        _strParamSetService.SetAll(this, SheetSetParams);
        foreach(var parameter in CustomParamsList.Params) {
            parameter.UpdateDueParamNameChange();
        }

        // Проходимся по наследникам и ищем свойства FiltrationComboBoxFilterListVM, в фильтрах вызываем метод обновления
        this.GetType().GetProperties()
            .Where(p => p.PropertyType == typeof(FiltrationComboBoxFilterListVM))
            .Select(p => p.GetValue(this) as FiltrationComboBoxFilterListVM)
            .Where(filterList => filterList != null)
            .SelectMany(filterList => filterList.ValueList)
            .ToList()
            .ForEach(f => f.UpdateDueParamNameChange());

        if(this is ScheduleViewVM scheduleViewVM) {
            foreach(var rule in scheduleViewVM.ScheduleFilterList.ScheduleFilterRules) {
                rule.UpdateDueParamNameChange();
            }
        }
    }

    public void UpdateDueParamValueChange(StringParamVM stringParam) {
        if(stringParam.StringValue is null) {
            return;
        }
        _strParamSetService.SetAll(this, SheetSetParams, stringParam);

        foreach(var parameter in CustomParamsList.Params) {
            parameter.UpdateDueParamValueChange(stringParam);
        }

        // Проходимся по наследникам и ищем свойства FiltrationComboBoxFilterListVM, в фильтрах вызываем метод обновления
        this.GetType().GetProperties()
            .Where(p => p.PropertyType == typeof(FiltrationComboBoxFilterListVM))
            .Select(p => p.GetValue(this) as FiltrationComboBoxFilterListVM)
            .Where(filterList => filterList != null)
            .SelectMany(filterList => filterList.ValueList)
            .ToList()
            .ForEach(f => f.UpdateDueParamValueChange(stringParam));

        if(this is ScheduleViewVM scheduleViewVM) {
            foreach(var rule in scheduleViewVM.ScheduleFilterList.ScheduleFilterRules) {
                rule.UpdateDueParamValueChange(stringParam);
            }
        }
    }


    public void SetCustomParams(Element element) {
        if(element is null) {
            return;
        }
        foreach(var param in CustomParamsList.Params) {
            // Если на экземпляре есть редактируемый параметр с таким именем
            var instanceParam = GetWritableParam(element, param.ParamName);
            if(instanceParam != null) {
                SetValue(instanceParam, param.ParamValue);
                continue;
            }
            // Если на типе есть редактируемый параметр с таким именем
            var type = Repository.Document.GetElement(element.GetTypeId());
            var typeParam = GetWritableParam(type, param.ParamName);
            if(typeParam != null) {
                SetValue(typeParam, param.ParamValue);
            }
        }
    }

    /// <summary>
    /// Возвращает первый редактируемый параметр элемента с указанным именем или null, если такого нет.
    /// </summary>
    private static Parameter GetWritableParam(Element element, string paramName) {
        if(element is null || string.IsNullOrWhiteSpace(paramName)) {
            return null;
        }
        return element.GetParameters(paramName).FirstOrDefault(p => !p.IsReadOnly);
    }

    /// <summary>
    /// Записывает строковое значение в параметр с учетом типа его хранения.
    /// Если значение не удалось преобразовать или Revit его отклонил, параметр не изменяется.
    /// </summary>
    private static void SetValue(Parameter parameter, string value) {
        value ??= string.Empty;
        try {
            switch(parameter.StorageType) {
                case StorageType.String:
                    parameter.Set(value);
                    break;
                case StorageType.Integer:
                    if(int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int intValue)) {
                        parameter.Set(intValue);
                    }
                    break;
                case StorageType.Double:
                    parameter.SetValueString(value);
                    break;
            }
        } catch(Exception) { }
    }
}
