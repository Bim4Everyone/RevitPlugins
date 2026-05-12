using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.Models.ConfigSerializer;
using RevitPackageDocumentation.ViewModels.Configuration;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet;

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
    private readonly SheetSetConfig _sheetSetConfig;

    private SheetSetData _currentSheetSetData;
    private SheetSetVM _currentSheetSet;
    private SheetVM _selectedSheet;

    private string _errorText;
    private string _saveProperty;

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
        SheetSetConfig sheetSetConfig) {

        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _messageBoxService = messageBoxService;
        _fileDialogService = fileDialogService;
        _sheetSetVMFactory = sheetSetVMFactory;
        _sheetSetConfig = sheetSetConfig;

        AddSheetCommand = RelayCommand.Create(AddSheet);
        AddComponentCommand = RelayCommand.Create(AddComponent);

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand AddSheetCommand { get; }
    public ICommand AddComponentCommand { get; }


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

    /// <summary>
    /// Свойство для примера. (требуется удалить)
    /// </summary>
    public string SaveProperty {
        get => _saveProperty;
        set => RaiseAndSetIfChanged(ref _saveProperty, value);
    }

    public SheetSetVM CurrentSheetSet {
        get => _currentSheetSet;
        set => RaiseAndSetIfChanged(ref _currentSheetSet, value);
    }

    public SheetVM SelectedSheet {
        get => _selectedSheet;
        set => RaiseAndSetIfChanged(ref _selectedSheet, value);
    }



    /// <summary>
    /// Метод загрузки главного окна.
    /// </summary>
    /// <remarks>В данном методе должна происходить загрузка настроек окна, а так же инициализация полей окна.</remarks>
    private void LoadView() {
        LoadConfig();

        ImportSheetSet();
        //ExportSheetSet();
    }

    private void ImportSheetSet() {
        _messageBoxService.Show("Import", "Title");

        string path = _fileDialogService.OpenFileDialog();
        _currentSheetSetData = _sheetSetConfig.Import(path);

        CurrentSheetSet = _sheetSetVMFactory.CreateSheetSetVM(_currentSheetSetData);
    }

    private void ExportSheetSet() {
        _messageBoxService.Show("Export", "Title");

        string path = _fileDialogService.SaveFileDialog();
        _sheetSetConfig.Export(_currentSheetSetData, path);
    }



    /// <summary>
    /// Загрузка настроек плагина.
    /// </summary>
    private void LoadConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);

        SaveProperty = setting?.SaveProperty ?? _localizationService.GetLocalizedString("MainWindow.Hello");
    }

    /// <summary>
    /// Сохранение настроек плагина.
    /// </summary>
    private void SaveConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);

        setting.SaveProperty = SaveProperty;
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
        if(string.IsNullOrEmpty(SaveProperty)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.HelloCheck");
            return false;
        }

        ErrorText = null;
        return true;
    }

    private void AddSheet() {
        CurrentSheetSet.AddSheet();
    }

    private void AddComponent() {
        SelectedSheet.AddComponent();
    }
}
