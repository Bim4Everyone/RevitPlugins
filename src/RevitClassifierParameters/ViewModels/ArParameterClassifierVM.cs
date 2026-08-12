using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitClassifierParameters.Models;
using RevitClassifierParameters.Models.FacadeType;
using RevitClassifierParameters.Models.MaterialClassifier;
using RevitClassifierParameters.Models.Work;
using RevitClassifierParameters.Views;

namespace RevitClassifierParameters.ViewModels;

internal class ArParameterClassifierVM : ParameterClassifierVM {
    private readonly FacadeTypeSetter _facadeTypeSetter;
    private readonly FacadeTypeExcelReader _facadeTypeExcelReader;

    /// <summary>
    /// Стандартный путь к файлу правил заполнения типа фасада.
    /// </summary>
    private string _excelFacadeTypePath;
    /// <summary>
    /// Стандартное наименование параметра стен на экземпляре, куда нужно заполнить тип фасада.
    /// </summary>
    private string _facadeTypeParamName = "ФОП_Группирование";
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
            classifierExcelReader, reportV, openFileDialogService, messageBoxService) {

        _facadeTypeSetter = facadeTypeSetter;
        _facadeTypeExcelReader = facadeTypeExcelReader;

        _excelFacadeTypePath =
            @"W:\Проектный институт\Отд.стандарт.BIM и RD\BIM-Ресурсы\5-Надстройки\Bim4Everyone\A101\"
            + $@"{_revitRepository.Application.VersionNumber}\RevitClassifierParameters\Правила заполнения типа фасада.xlsx";

        ReadFacadeTypeExcelCommand = RelayCommand.Create(RereadFacadeTypeExcel);
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

    public string ExcelFacadeTypePath {
        get => _excelFacadeTypePath;
        set => RaiseAndSetIfChanged(ref _excelFacadeTypePath, value);
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
        OpenFileDialogService.Title = _localizationService.GetLocalizedString("MainWindow.SelectFacadeTypeFile");

        if(OpenFileDialogService.ShowDialog()) {
            // Если пользователь выбрал файл при выборе файла
            ExcelFacadeTypePath = OpenFileDialogService.File.FullName;
        } else {
            // Если пользователь не выбрал файл
            MessageBoxService.Show(_localizationService.GetLocalizedString("MainWindow.NoFacadeTypeFileSelected"));
            return;
        }

        FacadeTypes = _facadeTypeExcelReader.Read(ExcelFacadeTypePath);
        if(FacadeTypes is null || FacadeTypes.Count == 0) {
            MessageBoxService.Show(_localizationService.GetLocalizedString("MainWindow.NoFacadeTypesFound"));
        }
    }

    private void ReadFacadeTypeExcel() {
        if(string.IsNullOrEmpty(ExcelFacadeTypePath) || !File.Exists(ExcelFacadeTypePath)) {
            OpenFileDialogService.Title = _localizationService.GetLocalizedString("MainWindow.SelectFacadeTypeFile");

            if(OpenFileDialogService.ShowDialog()) {
                // Если пользователь выбрал файл при выборе файла
                ExcelFacadeTypePath = OpenFileDialogService.File.FullName;
            } else {
                // Если пользователь не выбрал файл
                MessageBoxService.Show(_localizationService.GetLocalizedString("MainWindow.NoFacadeTypeFileSelected"));
                return;
            }
        }

        FacadeTypes = _facadeTypeExcelReader.Read(ExcelFacadeTypePath);
        if(FacadeTypes is null || FacadeTypes.Count == 0) {
            MessageBoxService.Show(_localizationService.GetLocalizedString("MainWindow.NoFacadeTypesFound"));
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
        // Если планируем работать с классификатором
        if(WorkWithMasonryCode || WorkWithRoofCode || WorkWithFacadeType) {
            if(string.IsNullOrEmpty(ExcelClassifierPath)) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorNoClassifierFile");
                return false;
            }
            if(CurrentClassifierWorks is null || CurrentClassifierWorks.Count == 0) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorNoClassifierWorks");
                return false;
            }
        }

        // Если планируем работать с типами фасадов
        if(WorkWithFacadeType) {
            if(string.IsNullOrEmpty(ExcelFacadeTypePath)) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorNoFacadeTypeFile");
                return false;
            }
            if(FacadeTypes is null || FacadeTypes.Count == 0) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorNoFacadeTypes");
                return false;
            }
            if(ParamForFacadeType is null) {
                ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorNoFacadeTypeParam");
                return false;
            }
        }

        ErrorText = string.Empty;
        return true;
    }

    private void LoadConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document);

        ExcelClassifierPath = setting?.ExcelClassifierPath ?? string.Empty;
        WorkWithMasonryCode = setting?.WorkWithMasonryCode ?? true;
        WorkWithRoofCode = setting?.WorkWithRoofCode ?? true;
        WorkWithFacadeCode = setting?.WorkWithFacadeCode ?? true;
        WorkWithFacadeType = setting?.WorkWithFacadeType ?? true;

        // Для следующих есть стандартное значение, но пользователь может задать иное
        _facadeTypeParamName = setting?.ParamNameForFacadeType ?? _facadeTypeParamName;
        ExcelFacadeTypePath = setting?.ExcelFacadeTypePath ?? _excelFacadeTypePath;
    }

    private void SaveConfig() {
        var setting = _pluginConfig.GetSettings(_revitRepository.Document)
                      ?? _pluginConfig.AddSettings(_revitRepository.Document);

        setting.ExcelClassifierPath = ExcelClassifierPath;
        setting.ExcelFacadeTypePath = ExcelFacadeTypePath;
        setting.WorkWithMasonryCode = WorkWithMasonryCode;
        setting.WorkWithRoofCode = WorkWithRoofCode;
        setting.WorkWithFacadeCode = WorkWithFacadeCode;
        setting.WorkWithFacadeType = WorkWithFacadeType;
        setting.ParamNameForFacadeType = ParamForFacadeType?.Definition.Name ?? _facadeTypeParamName;

        _pluginConfig.SaveProjectConfig();
    }
}
