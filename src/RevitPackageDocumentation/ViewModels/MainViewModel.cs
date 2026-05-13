using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.Models.ConfigSerializer;
using RevitPackageDocumentation.ViewModels.Configuration;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;

namespace RevitPackageDocumentation.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly IMessageBoxService _messageBoxService;
    private readonly IFileDialogService _fileDialogService;
    private readonly ISheetSetVMFactory _sheetSetVMFactory;
    private readonly ISheetSetDataFactory _sheetSetDataFactory;
    private readonly SheetSetConfig _sheetSetConfig;

    private SheetSetVM _currentSheetSet;
    private SheetVM _selectedSheet;

    private string _errorText;
    private string _sheetSetDataPath;

    private List<ViewFamilyType> _sectionViewFamilyTypes;
    private List<ViewPlan> _planViewTemplates;
    private List<ViewSection> _sectionViewTemplates;
    private List<ViewFamilyType> _structuralPlanViewFamilyTypes;

    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    /// <param name="pluginConfig">Настройки плагина.</param>
    /// <param name="revitRepository">Класс доступа к интерфейсу Revit.</param>
    /// <param name="localizationService">Интерфейс доступа к сервису локализации.</param>
    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        IMessageBoxService messageBoxService,
        IFileDialogService fileDialogService,
        ISheetSetVMFactory sheetSetVMFactory,
        ISheetSetDataFactory sheetSetDataFactory,
        SheetSetConfig sheetSetConfig) {

        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _messageBoxService = messageBoxService;
        _fileDialogService = fileDialogService;
        _sheetSetVMFactory = sheetSetVMFactory;
        _sheetSetDataFactory = sheetSetDataFactory;
        _sheetSetConfig = sheetSetConfig;

        ImportCommand = RelayCommand.Create(ImportSheetSet);
        ExportCommand = RelayCommand.Create(ExportSheetSet);

        AddSheetCommand = RelayCommand.Create(AddSheet);
        RemoveSheetCommand = RelayCommand.Create<SheetVM>(RemoveSheet);
        AddComponentCommand = RelayCommand.Create(AddComponent);
        RemoveComponentCommand = RelayCommand.Create<SheetComponentVM>(RemoveComponent);

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand ImportCommand { get; }
    public ICommand ExportCommand { get; }

    public ICommand AddSheetCommand { get; }
    public ICommand RemoveSheetCommand { get; }
    public ICommand AddComponentCommand { get; }
    public ICommand RemoveComponentCommand { get; }


    /// <summary>
    /// Команда загрузки главного окна.
    /// </summary>
    public ICommand LoadViewCommand { get; }

    /// <summary>
    /// Команда применения настроек главного окна. (запуск плагина)
    /// </summary>
    /// <remarks>В случаях, когда используется немодальное окно, требуется данную команду удалять.</remarks>
    public ICommand AcceptViewCommand { get; }

    /// <summary>
    /// Текст ошибки, который отображается при неверном вводе пользователя.
    /// </summary>
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public SheetSetVM CurrentSheetSet {
        get => _currentSheetSet;
        set => RaiseAndSetIfChanged(ref _currentSheetSet, value);
    }

    public SheetVM SelectedSheet {
        get => _selectedSheet;
        set => RaiseAndSetIfChanged(ref _selectedSheet, value);
    }


    public List<ViewFamilyType> StructuralPlanViewFamilyTypes {
        get => _structuralPlanViewFamilyTypes;
        set => RaiseAndSetIfChanged(ref _structuralPlanViewFamilyTypes, value);
    }

    public List<ViewFamilyType> SectionViewFamilyTypes {
        get => _sectionViewFamilyTypes;
        set => RaiseAndSetIfChanged(ref _sectionViewFamilyTypes, value);
    }

    public List<ViewPlan> PlanViewTemplates {
        get => _planViewTemplates;
        set => RaiseAndSetIfChanged(ref _planViewTemplates, value);
    }

    public List<ViewSection> SectionViewTemplates {
        get => _sectionViewTemplates;
        set => RaiseAndSetIfChanged(ref _sectionViewTemplates, value);
    }


    /// <summary>
    /// Метод загрузки главного окна.
    /// </summary>
    /// <remarks>В данном методе должна происходить загрузка настроек окна, а так же инициализация полей окна.</remarks>
    private void LoadView() {
        LoadConfig();

        StructuralPlanViewFamilyTypes = _revitRepository.StructuralPlanViewFamilyTypes;
        SectionViewFamilyTypes = _revitRepository.SectionViewFamilyTypes;
        PlanViewTemplates = _revitRepository.PlanViewTemplates;
        SectionViewTemplates = _revitRepository.SectionViewTemplates;

        if(string.IsNullOrEmpty(_sheetSetDataPath) || !File.Exists(_sheetSetDataPath)) {
            ImportSheetSet();
        } else {
            ImportSheetSet(_sheetSetDataPath);
        }
    }

    private void ImportSheetSet() {
        _sheetSetDataPath = _fileDialogService.OpenFileDialog();
        ImportSheetSet(_sheetSetDataPath);
    }

    private void ImportSheetSet(string sheetSetDataPath) {
        var currentSheetSetData = _sheetSetConfig.Import(sheetSetDataPath);
        CurrentSheetSet = _sheetSetVMFactory.CreateSheetSetVM(currentSheetSetData);
        SelectedSheet = CurrentSheetSet.SheetList.FirstOrDefault();
    }

    private void ExportSheetSet() {
        _sheetSetDataPath = _fileDialogService.SaveFileDialog();
        var currentSheetSetData = _sheetSetDataFactory.CreateSheetSetData(CurrentSheetSet);
        _sheetSetConfig.Export(currentSheetSetData, _sheetSetDataPath);

        _messageBoxService.Show("Export is successful", "Export");
    }


    /// <summary>
    /// Загрузка настроек плагина.
    /// </summary>
    private void LoadConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);

        _sheetSetDataPath = setting?.SheetSetDataPath;
    }

    /// <summary>
    /// Сохранение настроек плагина.
    /// </summary>
    private void SaveConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);

        setting.SheetSetDataPath = _sheetSetDataPath;
        _pluginConfig.SaveProjectConfig();
    }


    /// <summary>
    /// Метод применения настроек главного окна. (выполнение плагина)
    /// </summary>
    /// <remarks>
    /// В данном методе должны браться настройки пользователя и сохраняться в конфиг, а так же быть основной код плагина.
    /// </remarks>
    private void AcceptView() {
        SaveConfig();
    }

    /// <summary>
    /// Метод проверки возможности выполнения команды применения настроек.
    /// </summary>
    /// <returns>В случае когда true - команда может выполниться, в случае false - нет.</returns>
    /// <remarks>
    /// В данном методе происходит валидация ввода пользователя и уведомление его о неверных значениях.
    /// В методе проверяемые свойства окна должны быть отсортированы в таком же порядке как в окне (сверху-вниз)
    /// </remarks>
    private bool CanAcceptView() {
        //if(string.IsNullOrEmpty(SaveProperty)) {
        //    ErrorText = _localizationService.GetLocalizedString("MainWindow.HelloCheck");
        //    return false;
        //}

        //ErrorText = null;
        return true;
    }

    private void AddSheet() {
        CurrentSheetSet.AddSheet();
    }

    private void RemoveSheet(SheetVM sheet) {
        CurrentSheetSet.RemoveSheet(sheet);
        SelectedSheet = null;
    }

    private void AddComponent() {
        SelectedSheet.AddComponent();
    }

    private void RemoveComponent(SheetComponentVM sheetComponent) {
        SelectedSheet.RemoveComponent(sheetComponent);
    }
}
