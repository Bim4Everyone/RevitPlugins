using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClassifierParameters.Models;
using RevitClassifierParameters.Models.FacadeType;
using RevitClassifierParameters.Models.MaterialClassifier;
using RevitClassifierParameters.Models.Work;
using RevitClassifierParameters.Views;

namespace RevitClassifierParameters.ViewModels;

internal abstract class ParameterClassifierVM : BaseViewModel {
    protected readonly PluginConfig _pluginConfig;
    protected readonly RevitRepository _revitRepository;
    protected readonly ILocalizationService _localizationService;
    protected readonly WorkGroupCode _workGroupCode;
    protected readonly MaterialParamSetter _materialParamSetter;
    protected readonly MaterialReportService _materialReportService;
    protected readonly ClassifierExcelReader _classifierExcelReader;
    protected readonly SystemPluginConfig _systemPluginConfig;
    protected readonly ReportV _reportV;

    private string _errorText;
    private List<WorkGroup> _currentClassifierWorks;
    private List<Material> _materialInPj;
    protected string _excelClassifierPath;

    protected ParameterClassifierVM(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        WorkGroupCode workGroupCode,
        MaterialParamSetter materialParamSetter,
        MaterialReportService materialReportService,
        ClassifierExcelReader classifierExcelReader,
        SystemPluginConfig systemPluginConfig,
        ReportV reportV,
        IOpenFileDialogService openFileDialogService,
        IMessageBoxService messageBoxService) {

        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;

        _workGroupCode = workGroupCode;
        _materialParamSetter = materialParamSetter;
        _materialReportService = materialReportService;
        _classifierExcelReader = classifierExcelReader;
        _systemPluginConfig = systemPluginConfig;
        _reportV = reportV;

        MessageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
        OpenFileDialogService = openFileDialogService ?? throw new ArgumentNullException(nameof(openFileDialogService));

        RereadClassifierExcelCommand = RelayCommand.Create(RereadClassifierExcel);
        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand RereadClassifierExcelCommand { get; }
    public ICommand LoadViewCommand { get; }
    public ICommand AcceptViewCommand { get; }

    public IOpenFileDialogService OpenFileDialogService { get; }
    public IMessageBoxService MessageBoxService { get; }

    protected List<WorkGroup> CurrentClassifierWorks {
        get => _currentClassifierWorks;
        private set => RaiseAndSetIfChanged(ref _currentClassifierWorks, value);
    }

    protected List<Material> MaterialInPj {
        get => _materialInPj;
        set => RaiseAndSetIfChanged(ref _materialInPj, value);
    }

    public string ExcelClassifierPath {
        get => _excelClassifierPath;
        set => RaiseAndSetIfChanged(ref _excelClassifierPath, value);
    }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    private void RereadClassifierExcel() {
        string filePath = SelectFile(
            "MainWindow.SelectClassifierFile",
            "MainWindow.NoClassifierFileSelected",
            _systemPluginConfig.ClassifierDirectoryPath);
        if(filePath is null) {
            return;
        }

        ExcelClassifierPath = filePath;
        CurrentClassifierWorks = ReadExcel(
            _classifierExcelReader, ExcelClassifierPath, "MainWindow.NoClassifierWorksFound");
    }

    protected void ReadClassifierExcel() {
        if(string.IsNullOrEmpty(ExcelClassifierPath) || !File.Exists(ExcelClassifierPath)) {
            return;
        }

        CurrentClassifierWorks = ReadExcel(
            _classifierExcelReader, ExcelClassifierPath, "MainWindow.NoClassifierWorksFound");
    }

    /// <summary>
    /// Показывает диалог выбора файла.
    /// </summary>
    /// <param name="titleKey">Ключ локализации заголовка диалога.</param>
    /// <param name="noFileMessageKey">Ключ локализации сообщения о том, что файл не выбран.</param>
    /// <param name="initialDirectory"> Папка, открываемая в диалоге по умолчанию. Если не задана или не существует, не подставляется.
    /// </param>
    /// <returns>
    /// Путь к выбранному файлу, либо <see langword="null" />, если пользователь не выбрал файл.
    /// </returns>
    protected string SelectFile(string titleKey, string noFileMessageKey, string initialDirectory = null) {
        if(!string.IsNullOrEmpty(initialDirectory) && Directory.Exists(initialDirectory)) {
            OpenFileDialogService.InitialDirectory = initialDirectory;
        }

        OpenFileDialogService.Title = _localizationService.GetLocalizedString(titleKey);
        if(OpenFileDialogService.ShowDialog()) {
            return OpenFileDialogService.File.FullName;
        }

        MessageBoxService.Show(_localizationService.GetLocalizedString(noFileMessageKey));
        return null;
    }

    /// <summary>
    /// Читает Excel-файл по указанному пути и сообщает пользователю, если данных не найдено.
    /// </summary>
    /// <param name="reader">Ридер Excel-файла.</param>
    /// <param name="filePath">Путь к читаемому файлу.</param>
    /// <param name="noDataMessageKey">Ключ локализации сообщения о том, что данные не найдены.</param>
    protected TResult ReadExcel<TResult>(
        ExcelReaderBase<TResult> reader,
        string filePath,
        string noDataMessageKey) where TResult : class, ICollection {

        var result = reader.Read(filePath);
        if(result is null || result.Count == 0) {
            MessageBoxService.Show(_localizationService.GetLocalizedString(noDataMessageKey));
        }
        return result;
    }

    protected void GetMaterials() {
        MaterialInPj = _revitRepository.GetElementMaterials();
        if(MaterialInPj is null || MaterialInPj.Count == 0) {
            MessageBoxService.Show(_localizationService.GetLocalizedString("MainWindow.NoMaterialsFound"));
        }
    }

    protected abstract void LoadView();
    protected abstract void AcceptView();
    protected abstract bool CanAcceptView();

    /// <summary>
    /// Показывает окно отчёта, если по итогам обработки есть собранные записи.
    /// </summary>
    protected void ShowReport() {
        if(_materialReportService.Items.Count == 0) {
            return;
        }

        if(_reportV.DataContext is ReportVM reportVM) {
            reportVM.UpdateReportData();
        }
        _reportV.ShowDialog();
    }

    protected HashSet<string> GetActiveCodes() {
        return GetType()
            .GetProperties()
            .Where(p => p.Name.StartsWith("WorkWith")
                        && p.PropertyType == typeof(bool)
                        && (bool) p.GetValue(this)!)
            .Select(p => typeof(WorkGroupCode).GetProperty(p.Name.Replace("WorkWith", "")))
            .Where(p => p != null)
            .Select(p => p!.GetValue(_workGroupCode)!.ToString()!)
            .ToHashSet();
    }
}
