using dosymep.SimpleServices;

using RevitClassifierParameters.Models;
using RevitClassifierParameters.Models.MaterialClassifier;
using RevitClassifierParameters.Models.Work;
using RevitClassifierParameters.Views;

namespace RevitClassifierParameters.ViewModels;

internal class KrParameterClassifierVM : ParameterClassifierVM {
    private bool _workWithFoundationCode;
    private bool _workWithConstrBelowZeroCode;
    private bool _workWithConstrAboveZeroCode;
    private bool _workWithConcreteParams;

    public KrParameterClassifierVM(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        WorkGroupCode workGroupCode,
        MaterialParamSetter materialParamSetter,
        MaterialReportService materialReportService,
        ClassifierExcelReader classifierExcelReader,
        ReportV reportV,
        IOpenFileDialogService openFileDialogService,
        IMessageBoxService messageBoxService) :
        base(pluginConfig, revitRepository, localizationService, workGroupCode, materialParamSetter, materialReportService,
            classifierExcelReader, reportV, openFileDialogService, messageBoxService) {
    }

    public bool WorkWithFoundationCode {
        get => _workWithFoundationCode;
        set => RaiseAndSetIfChanged(ref _workWithFoundationCode, value);
    }

    public bool WorkWithConstrBelowZeroCode {
        get => _workWithConstrBelowZeroCode;
        set => RaiseAndSetIfChanged(ref _workWithConstrBelowZeroCode, value);
    }

    public bool WorkWithConstrAboveZeroCode {
        get => _workWithConstrAboveZeroCode;
        set => RaiseAndSetIfChanged(ref _workWithConstrAboveZeroCode, value);
    }

    public bool WorkWithConcreteParams {
        get => _workWithConcreteParams;
        set => RaiseAndSetIfChanged(ref _workWithConcreteParams, value);
    }
    
    protected override void LoadView() {
        LoadConfig();
        ReadClassifierExcel();
        GetMaterials();
    }
    
    protected override void AcceptView() {
        SaveConfig();
        var activeCodes = GetActiveCodes();
        _materialParamSetter.SetParamValue(activeCodes, CurrentClassifierWorks, MaterialInPj, false);
        ShowReport();
    }

    protected override bool CanAcceptView() {
        // Если планируем работать с классификатором
        if(WorkWithFoundationCode || WorkWithConstrBelowZeroCode || WorkWithConstrAboveZeroCode) {
            if(string.IsNullOrEmpty(ExcelClassifierPath)) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorNoClassifierFile");
                return false;
            }
            if(CurrentClassifierWorks is null || CurrentClassifierWorks.Count == 0) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorNoClassifierWorks");
                return false;
            }
        }

        ErrorText = string.Empty;
        return true;
    }

    private void LoadConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document);

        ExcelClassifierPath = setting?.ExcelClassifierPath ?? string.Empty;
        WorkWithFoundationCode = setting?.WorkWithFoundationCode ?? true;
        WorkWithConstrBelowZeroCode = setting?.WorkWithConstrBelowZeroCode ?? true;
        WorkWithConstrAboveZeroCode = setting?.WorkWithConstrAboveZeroCode ?? true;
        WorkWithConcreteParams = setting?.WorkWithConcreteParams ?? true;
    }

    private void SaveConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
                      ?? _pluginConfig.AddSettings(_revitRepository.Document);

        setting.ExcelClassifierPath = ExcelClassifierPath;
        setting.WorkWithFoundationCode = WorkWithFoundationCode;
        setting.WorkWithConstrBelowZeroCode = WorkWithConstrBelowZeroCode;
        setting.WorkWithConstrAboveZeroCode = WorkWithConstrAboveZeroCode;
        setting.WorkWithConcreteParams = WorkWithConcreteParams;

        _pluginConfig.SaveProjectConfig();
    }
}
