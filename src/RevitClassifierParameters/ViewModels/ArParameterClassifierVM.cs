using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using RevitClassifierParameters.Models;

namespace RevitClassifierParameters.ViewModels;

internal class ArParameterClassifierVM : ParameterClassifierVM{
    private bool _workWithMasonryCode;
    private bool _workWithRoofCode;
    private bool _workWithFacadeCode;
    private bool _workWithFacadeType;
    private Parameter _paramForFacadeType;

    public ArParameterClassifierVM(
        PluginConfig pluginConfig, 
        RevitRepository revitRepository, 
        ILocalizationService localizationService, 
        WorkGroupCode workGroupCode, 
        MaterialParamSetter materialParamSetter, 
        ReportService reportService, 
        ExcelClassifierReader excelClassifierReader, 
        IOpenFileDialogService openFileDialogService, 
        IMessageBoxService messageBoxService) : 
        base(pluginConfig, revitRepository, localizationService, workGroupCode, materialParamSetter, reportService, 
            excelClassifierReader, openFileDialogService, messageBoxService) {
        
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
    
    
    protected override void LoadView() {
        ReadExcel();
        GetMaterials();
        GetParamForFacadeType();
    }

    private void GetParamForFacadeType() {
        // Получение параметра для типа фасада
    } 

    protected override void AcceptView(){ 
        var activeCodes = GetActiveCodes();
        _materialParamSetter.SetParamValue(activeCodes, CurrentClassifierWorks, MaterialInPj, true);
        if(WorkWithFacadeCode && WorkWithFacadeType){
            SetFacadeType();
        }
    }

    private void SetFacadeType() {
        throw new System.NotImplementedException();
    }

    protected override bool CanAcceptView() {
        throw new System.NotImplementedException();
    }
    
    private void LoadConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document);

        _excelClassifierPath = setting?.ExcelClassifierPath ?? string.Empty;
        WorkWithMasonryCode = setting?.WorkWithMasonryCode ?? true;
        WorkWithRoofCode = setting?.WorkWithRoofCode ?? true;
        WorkWithFacadeCode = setting?.WorkWithFacadeCode ?? true;
        WorkWithFacadeType = setting?.WorkWithFacadeType ?? true;
        //ParamNameForFacadeType = setting?.ParamNameForFacadeType ?? null;
    }

    private void SaveConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
                      ?? _pluginConfig.AddSettings(_revitRepository.Document);

        setting.ExcelClassifierPath = _excelClassifierPath;
        setting.WorkWithMasonryCode = WorkWithMasonryCode;
        setting.WorkWithRoofCode = WorkWithRoofCode;
        setting.WorkWithFacadeCode = WorkWithFacadeCode;
        setting.WorkWithFacadeType = WorkWithFacadeType;
        //setting.ParamNameForFacadeType = ParamForFacadeType.Name;

        _pluginConfig.SaveProjectConfig();
    }
}
