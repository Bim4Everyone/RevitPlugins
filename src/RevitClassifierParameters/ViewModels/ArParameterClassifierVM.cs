using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitClassifierParameters.Models;
using RevitClassifierParameters.Views;

namespace RevitClassifierParameters.ViewModels;

internal class ArParameterClassifierVM : ParameterClassifierVM {
    private string _defFacadeTypeParamName = "ФОП_Группирование";
    
    private bool _workWithMasonryCode;
    private bool _workWithRoofCode;
    private bool _workWithFacadeCode;
    private bool _workWithFacadeType;
    private Parameter _paramForFacadeType;
    private ObservableCollection<Parameter> _paramsForFacadeType;

    public ArParameterClassifierVM(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        WorkGroupCode workGroupCode,
        MaterialParamSetter materialParamSetter,
        ReportService reportService,
        ExcelClassifierReader excelClassifierReader,
        ReportV reportV,
        IOpenFileDialogService openFileDialogService,
        IMessageBoxService messageBoxService) :
        base(pluginConfig, revitRepository, localizationService, workGroupCode, materialParamSetter, reportService,
            excelClassifierReader, reportV, openFileDialogService, messageBoxService) {

    }

    public bool WorkWithMasonryCode {
        get => _workWithMasonryCode;
        set => RaiseAndSetIfChanged(ref _workWithMasonryCode, value);
    }

    public bool WorkWithRoofCode {
        get => _workWithRoofCode;
        set => RaiseAndSetIfChanged(ref _workWithRoofCode, value);
    }

    public bool WorkWithFacadeCode {
        get => _workWithFacadeCode;
        set => RaiseAndSetIfChanged(ref _workWithFacadeCode, value);
    }

    public bool WorkWithFacadeType {
        get => _workWithFacadeType;
        set => RaiseAndSetIfChanged(ref _workWithFacadeType, value);
    }

    public Parameter ParamForFacadeType {
        get => _paramForFacadeType;
        set => RaiseAndSetIfChanged(ref _paramForFacadeType, value);
    }    
    
    public ObservableCollection<Parameter> ParamsForFacadeType {
        get => _paramsForFacadeType;
        private set => RaiseAndSetIfChanged(ref _paramsForFacadeType, value);
    } 

    protected override void LoadView() {
        LoadConfig();
        ReadExcel();
        GetMaterials();
        GetParamsForFacadeType();
    }
    
    /// <summary>
    /// Получение параметров для заполнения типа фасада
    /// </summary>
    private void GetParamsForFacadeType() {
        ParamsForFacadeType = new ObservableCollection<Parameter>(_revitRepository.GetParametersForFacadeType()); 
        ParamForFacadeType = ParamsForFacadeType.FirstOrDefault(p => p.Definition.Name == _defFacadeTypeParamName);
    }

    protected override void AcceptView() {
        SaveConfig();
        var activeCodes = GetActiveCodes();
        _materialParamSetter.SetParamValue(activeCodes, CurrentClassifierWorks, MaterialInPj, true);
        if(WorkWithFacadeCode && WorkWithFacadeType) {
            SetFacadeType();
        }
        ShowReport();
    }

    private void SetFacadeType() {

    }

    protected override bool CanAcceptView() {
        return true;
    }

    private void LoadConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document);

        _excelClassifierPath = setting?.ExcelClassifierPath ?? string.Empty;
        WorkWithMasonryCode = setting?.WorkWithMasonryCode ?? true;
        WorkWithRoofCode = setting?.WorkWithRoofCode ?? true;
        WorkWithFacadeCode = setting?.WorkWithFacadeCode ?? true;
        WorkWithFacadeType = setting?.WorkWithFacadeType ?? true;
        _defFacadeTypeParamName = setting?.ParamNameForFacadeType ?? _defFacadeTypeParamName;
    }

    private void SaveConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
                      ?? _pluginConfig.AddSettings(_revitRepository.Document);

        setting.ExcelClassifierPath = _excelClassifierPath;
        setting.WorkWithMasonryCode = WorkWithMasonryCode;
        setting.WorkWithRoofCode = WorkWithRoofCode;
        setting.WorkWithFacadeCode = WorkWithFacadeCode;
        setting.WorkWithFacadeType = WorkWithFacadeType;
        setting.ParamNameForFacadeType = ParamForFacadeType.Definition.Name;

        _pluginConfig.SaveProjectConfig();
    }
}
