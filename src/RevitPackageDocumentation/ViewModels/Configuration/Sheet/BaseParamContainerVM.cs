using System;
using System.Collections.Generic;
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

/// <summary>
/// Базовый класс модулей со свойствами-формулами и дополнительными параметрами. Отвечает за пересчет формул
/// при изменении параметров комплекта и за запись дополнительных параметров в созданные элементы
/// </summary>
internal abstract class BaseParamContainerVM : ValidatableVM {
    private readonly StringParamSetService _strParamSetService;

    private CustomParametersListVM _customParamsList;

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

    /// <summary>
    /// Команда пересчета свойства по формуле, вызывается при изменении текста формулы.
    /// Параметр команды - имя свойства формулы
    /// </summary>
    public ICommand PropUpdateByFormulaCommand { get; }

    /// <summary>
    /// Параметры комплекта, на которые могут ссылаться формулы
    /// </summary>
    public ObservableCollection<PluginParamVM> SheetSetParams { get; }

    public RevitRepository Repository { get; }

    /// <summary>
    /// Дополнительные параметры, записываемые в созданный элемент
    /// </summary>
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

        foreach(var filter in GetFiltrationComboBoxFilters()) {
            filter.UpdateDueParamNameChange();
        }

        if(this is ScheduleViewVM scheduleViewVM) {
            foreach(var rule in scheduleViewVM.ScheduleFilterList.ScheduleFilterRules) {
                rule.UpdateDueParamNameChange();
            }
        }
    }

    /// <summary>
    /// В случае изменения значения текстового параметра конфигурации нужно пересчитать формулы,
    /// которые на него ссылаются
    /// </summary>
    public void UpdateDueParamValueChange(StringParamVM stringParam) {
        if(stringParam.StringValue is null) {
            return;
        }
        _strParamSetService.SetAll(this, SheetSetParams, stringParam);

        foreach(var parameter in CustomParamsList.Params) {
            parameter.UpdateDueParamValueChange(stringParam);
        }

        foreach(var filter in GetFiltrationComboBoxFilters()) {
            filter.UpdateDueParamValueChange(stringParam);
        }

        if(this is ScheduleViewVM scheduleViewVM) {
            foreach(var rule in scheduleViewVM.ScheduleFilterList.ScheduleFilterRules) {
                rule.UpdateDueParamValueChange(stringParam);
            }
        }
    }

    /// <summary>
    /// Возвращает значения всех фильтров выпадающих списков модуля: свойства типа FiltrationComboBoxFilterListVM
    /// ищутся у фактического типа модуля. Список материализуется, так как обновление фильтров меняет свойства модуля
    /// </summary>
    private List<FiltrationComboBoxFilterVM> GetFiltrationComboBoxFilters() {
        return GetType().GetProperties()
            .Where(p => p.PropertyType == typeof(FiltrationComboBoxFilterListVM))
            .Select(p => p.GetValue(this) as FiltrationComboBoxFilterListVM)
            .Where(filterList => filterList != null)
            .SelectMany(filterList => filterList.ValueList)
            .ToList();
    }

    /// <summary>
    /// Записывает дополнительные параметры в элемент: в параметр экземпляра, а если его нет - в параметр типа.
    /// Параметры, которых нет или которые доступны только для чтения, пропускаются
    /// </summary>
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
