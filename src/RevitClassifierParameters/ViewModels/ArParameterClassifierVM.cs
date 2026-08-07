using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitClassifierParameters.Models;
using RevitClassifierParameters.Models.FacadeType;
using RevitClassifierParameters.Models.MaterialClassifier;
using RevitClassifierParameters.Models.Work;
using RevitClassifierParameters.Views;

namespace RevitClassifierParameters.ViewModels;

internal class ArParameterClassifierVM : ParameterClassifierVM {
    /// <summary>
    /// Стандартный путь к файлу правил заполнения типа фасада.
    /// </summary>
    private string _excelFacadeTypePath =
        @"C:\Users\nikita\Desktop\Проекты\Параметры ВОР АР\XLS\Правила заполнения ФОП_Группировка.xlsx";
    
    private string _facadeTypeParamName = "ФОП_Группирование";

    private readonly FacadeTypeSetter _facadeTypeSetter;

    private bool _workWithMasonryCode;
    private bool _workWithRoofCode;
    private bool _workWithFacadeCode;
    private bool _workWithFacadeType;
    private List<FacadeTypeItem> _facadeTypes;
    private Parameter _paramForFacadeType;
    private ObservableCollection<Parameter> _paramsForFacadeType;

    public ArParameterClassifierVM(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        WorkGroupCode workGroupCode,
        MaterialParamSetter materialParamSetter,
        MaterialReportService materialReportService,
        ClassifierExcelReader classifierExcelReader,
        FacadeTypeExcelReader facadeTypeExcelReader,
        FacadeTypeSetter facadeTypeSetter,
        ReportV reportV,
        IOpenFileDialogService openFileDialogService,
        IMessageBoxService messageBoxService) :
        base(pluginConfig, revitRepository, localizationService, workGroupCode, materialParamSetter, materialReportService,
            classifierExcelReader, facadeTypeExcelReader, reportV, openFileDialogService, messageBoxService) {

        _facadeTypeSetter = facadeTypeSetter;

        ReadFacadeTypeExcelCommand = RelayCommand.Create(ReadFacadeTypeExcel);
    }

    public ICommand ReadFacadeTypeExcelCommand { get; }

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

    public List<FacadeTypeItem> FacadeTypes {
        get => _facadeTypes;
        private set => RaiseAndSetIfChanged(ref _facadeTypes, value);
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
        ReadClassifierExcel();
        GetMaterials();
        GetParamsForFacadeType();
        ReadFacadeTypeExcel();
    }
    
    /// <summary>
    /// Получение параметров для заполнения типа фасада
    /// </summary>
    private void GetParamsForFacadeType() {
        ParamsForFacadeType = new ObservableCollection<Parameter>(_revitRepository.GetParametersForFacadeType()); 
        ParamForFacadeType = ParamsForFacadeType.FirstOrDefault(p => p.Definition.Name == _facadeTypeParamName);
    }

    /// <summary>
    /// Считывает файл правил заполнения типа фасада.
    /// Если путь по умолчанию/из конфига существует — читает без диалога, иначе открывает диалог выбора файла.
    /// </summary>
    private void RereadFacadeTypeExcel() {
        if(OpenFileDialogService.ShowDialog()) {
            // Если пользователь выбрал файл при выборе файла
            _excelFacadeTypePath = OpenFileDialogService.File.FullName;
        } else {
            // Если пользователь не выбрал файл
            MessageBoxService.Show("Не выбран файл правил заполнения типа фасада!");
            return;
        }

        FacadeTypes = _facadeTypeExcelReader.Read(_excelFacadeTypePath);
        if(FacadeTypes is null || FacadeTypes.Count == 0) {
            MessageBoxService.Show("Не найдены типы фасадом в файле!");
        }
    }
    
    private void ReadFacadeTypeExcel() {
        if(string.IsNullOrEmpty(_excelFacadeTypePath) || !File.Exists(_excelFacadeTypePath)) {
            if(OpenFileDialogService.ShowDialog()) {
                // Если пользователь выбрал файл при выборе файла
                _excelFacadeTypePath = OpenFileDialogService.File.FullName;
            } else {
                // Если пользователь не выбрал файл
                MessageBoxService.Show("Не выбран файл правил заполнения типа фасада!");
                return;
            }
        }

        FacadeTypes = _facadeTypeExcelReader.Read(_excelFacadeTypePath);
        if(FacadeTypes is null || FacadeTypes.Count == 0) {
            MessageBoxService.Show("Не найдены типы фасадом в файле!");
        }
    }

    protected override void AcceptView() {
        SaveConfig();
        var activeCodes = GetActiveCodes();
        _materialParamSetter.SetParamValue(activeCodes, CurrentClassifierWorks, MaterialInPj, true);
        if(WorkWithFacadeCode 
           && WorkWithFacadeType 
           && ParamForFacadeType != null 
           && FacadeTypes != null 
           && FacadeTypes.Count != 0) {
            _facadeTypeSetter.SetFacadeType(ParamForFacadeType.Definition.Name, FacadeTypes);
        }
        ShowReport();
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
        
        // Для следующих есть стандартное значение, но пользователь может задать иное
        _facadeTypeParamName = setting?.ParamNameForFacadeType ?? _facadeTypeParamName;
        _excelFacadeTypePath = setting?.ExcelFacadeTypePath ?? _excelFacadeTypePath;
    }

    private void SaveConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
                      ?? _pluginConfig.AddSettings(_revitRepository.Document);

        setting.ExcelClassifierPath = _excelClassifierPath;
        setting.ExcelFacadeTypePath = _excelFacadeTypePath;
        setting.WorkWithMasonryCode = WorkWithMasonryCode;
        setting.WorkWithRoofCode = WorkWithRoofCode;
        setting.WorkWithFacadeCode = WorkWithFacadeCode;
        setting.WorkWithFacadeType = WorkWithFacadeType;
        setting.ParamNameForFacadeType = ParamForFacadeType?.Definition.Name ?? _facadeTypeParamName;

        _pluginConfig.SaveProjectConfig();
    }
}
