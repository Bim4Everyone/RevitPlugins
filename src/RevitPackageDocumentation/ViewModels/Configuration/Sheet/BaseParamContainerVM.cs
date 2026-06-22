using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Configuration.CustomParameters;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;
using RevitPackageDocumentation.ViewModels.FiltrationComboBoxVMs;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet;
internal abstract class BaseParamContainerVM : BaseViewModel {
    private CustomParametersListVM _customParamsList;
    private readonly StringParamSetService _strParamSetService;

    protected BaseParamContainerVM(
        RevitRepository repository,
        StringParamSetService stringParamSetService,
        ObservableCollection<PluginParamVM> sheetSetParams) {
        Repository = repository;
        _strParamSetService = stringParamSetService;
        SheetSetParams = sheetSetParams;
        CustomParamsList = new CustomParametersListVM(SheetSetParams, _strParamSetService);

        PropUpdateByFormulaCommand = RelayCommand.Create<string>(PropUpdateByFormula);
    }

    public ICommand PropUpdateByFormulaCommand { get; }

    public ObservableCollection<PluginParamVM> SheetSetParams { get; }
    public RevitRepository Repository { get; }


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
            // Если параметр существует на экземпляре и он редактируемый
            if(element.IsExistsParam(param.ParamName) && !element.GetParam(param.ParamName).IsReadOnly) {
                element.SetParamValue(param.ParamName, param.ParamValue);
                continue;
            }
            // Если параметр существует на типе и он редактируемый
            var type = Repository.Document.GetElement(element.GetTypeId());
            if(type != null && type.IsExistsParam(param.ParamName) && !element.GetParam(param.ParamName).IsReadOnly) {
                type.SetParamValue(param.ParamName, param.ParamValue);
            }
        }
    }
}
