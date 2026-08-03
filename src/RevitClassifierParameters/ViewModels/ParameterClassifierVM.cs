using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClassifierParameters.Models;

namespace RevitClassifierParameters.ViewModels;

internal abstract class ParameterClassifierVM : BaseViewModel {
    protected readonly PluginConfig _pluginConfig;
    protected readonly RevitRepository _revitRepository;
    protected readonly ILocalizationService _localizationService;
    protected readonly WorkGroupCode _workGroupCode;
    protected readonly MaterialParamSetter _materialParamSetter;
    protected readonly ReportService _reportService;
    protected readonly ExcelClassifierReader _excelClassifierReader;

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
        ReportService reportService,
        ExcelClassifierReader excelClassifierReader,
        IOpenFileDialogService openFileDialogService,
        IMessageBoxService messageBoxService) {
        
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        
        _workGroupCode = workGroupCode;
        _materialParamSetter = materialParamSetter;
        _reportService = reportService;
        _excelClassifierReader = excelClassifierReader;
        
        MessageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
        OpenFileDialogService = openFileDialogService ?? throw new ArgumentNullException(nameof(openFileDialogService));

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand LoadViewCommand { get; }
    public ICommand AcceptViewCommand { get; }
    
    public IOpenFileDialogService OpenFileDialogService { get; }
    public IMessageBoxService MessageBoxService { get; }

    public List<WorkGroup> CurrentClassifierWorks {
        get => _currentClassifierWorks;
        set => RaiseAndSetIfChanged(ref _currentClassifierWorks, value);
    }

    protected List<Material> MaterialInPj {
        get => _materialInPj;
        set => RaiseAndSetIfChanged(ref _materialInPj, value);
    }
    
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    protected void ReadExcel(){
        if(string.IsNullOrEmpty(_excelClassifierPath) || !File.Exists(_excelClassifierPath)) {
            if(OpenFileDialogService.ShowDialog()) {
                // Если пользователь выбрал файл при выборе файла
                _excelClassifierPath = OpenFileDialogService.File.FullName;
            } else {
                // Если пользователь не выбрал файл
                MessageBoxService.Show("Не выбран файл Классификатора!");
                return;
            }
        }
        CurrentClassifierWorks = _excelClassifierReader.Read(_excelClassifierPath);
        if(CurrentClassifierWorks.Count == 0){
            MessageBoxService.Show("Не найдены работы в Классификатора!");
        }
    }

    protected void GetMaterials() {
        MaterialInPj = _revitRepository.GetElementMaterials();
        if(MaterialInPj is null || MaterialInPj.Count == 0) {
            MessageBoxService.Show("Не найдено материалов!");
        }
    }

    protected abstract void LoadView();
    protected abstract void AcceptView();
    protected abstract bool CanAcceptView();

    protected HashSet<string> GetActiveCodes() {
        return GetType()
            .GetProperties()
            .Where(p => p.Name.StartsWith("WorkWith") 
                        && p.PropertyType == typeof(bool) 
                        && (bool)p.GetValue(this)!)
            .Select(p => typeof(WorkGroupCode).GetProperty(p.Name.Replace("WorkWith", "")))
            .Where(p => p != null)
            .Select(p => p!.GetValue(_workGroupCode)!.ToString()!)
            .ToHashSet();
    }
}
