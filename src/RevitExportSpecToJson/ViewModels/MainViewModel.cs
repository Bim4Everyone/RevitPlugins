using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitExportSpecToJson.Models;
using RevitExportSpecToJson.Services;

namespace RevitExportSpecToJson.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ISaveToJsonService _saveToJsonService;
    private readonly ILocalizationService _localizationService;

    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ISaveToJsonService saveToJsonService,
        ILocalizationService localizationService,
        IProgressDialogFactory progressDialogFactory,
        IOpenFolderDialogService openFolderDialogService) {
        
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _saveToJsonService = saveToJsonService;
        _localizationService = localizationService;

        ProgressDialogFactory = progressDialogFactory;
        OpenFolderDialogService = openFolderDialogService;

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        
        FilterSchedulesCommand = RelayCommand.Create(FilterSchedules);
        SelectSaveFolderCommand = RelayCommand.Create(SelectSaveFolder);
        
        MainSchedules = [];
        FilteredSchedules = [];
    }

    public IProgressDialogFactory ProgressDialogFactory { get; }
    public IOpenFolderDialogService OpenFolderDialogService { get; }

    /// <summary>
    /// Команда загрузки главного окна.
    /// </summary>
    public ICommand LoadViewCommand { get; }
    
    /// <summary>
    /// Команда применения настроек главного окна. (запуск плагина)
    /// </summary>
    /// <remarks>В случаях, когда используется немодальное окно, требуется данную команду удалять.</remarks>
    public ICommand AcceptViewCommand { get; }
    
    public ICommand FilterSchedulesCommand { get; }
    public ICommand SelectSaveFolderCommand { get; }

    /// <summary>
    /// Текст ошибки, который отображается при неверном вводе пользователя.
    /// </summary>
    public string? ErrorText {
        get;
        set => RaiseAndSetIfChanged(ref field, value);
    }
    
    public bool ClearSaveFolder {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public string? SaveFolder {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    
    public string? FilterSchedulesText {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    public ObservableCollection<ScheduleViewModel> MainSchedules {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }
    
    public ObservableCollection<ScheduleViewModel> FilteredSchedules {
        get;
        set => this.RaiseAndSetIfChanged(ref field, value);
    }

    /// <summary>
    /// Метод загрузки главного окна.
    /// </summary>
    /// <remarks>В данном методе должна происходить загрузка настроек окна, а так же инициализация полей окна.</remarks>
    private void LoadView() {
        MainSchedules = [
            .. _revitRepository.GetSchedules()
                .Select(item => new ScheduleViewModel(item))
        ];

        FilteredSchedules = MainSchedules;
        
        LoadConfig();
    }

    /// <summary>
    /// Метод применения настроек главного окна. (выполнение плагина)
    /// </summary>
    /// <remarks>
    /// В данном методе должны браться настройки пользователя и сохраняться в конфиг, а так же быть основной код плагина.
    /// </remarks>
    private void AcceptView() {
        SaveConfig();

        if(SaveFolder is not null
           && MainSchedules?.Any(item => item.Checked) is true) {

            if(ClearSaveFolder && Directory.Exists(SaveFolder)) {
                foreach(string filePath in Directory.GetFiles(SaveFolder, "*.json")) {
                    try {
                        File.Delete(filePath);
                    } catch {
                        // pass
                    }
                }
            }

            var schedules = MainSchedules
                .Where(item => item.Checked)
                .Select(item => item.Schedule)
                .ToArray();

            using var progressDialog = ProgressDialogFactory.CreateDialog();
            progressDialog.MaxValue = schedules.Length;
            var progress = progressDialog.CreateProgress();
            var cancellationToken = progressDialog.CreateCancellationToken();

            progressDialog.Show();

            _saveToJsonService.SaveToJson(SaveFolder, progress, cancellationToken, schedules);
        }
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
        if(string.IsNullOrEmpty(SaveFolder)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.SaveFolderError");
            return false;
        }

        if(SaveFolder?.IndexOfAny(Path.GetInvalidPathChars()) >= 0) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.SaveFolderInvalidCharsError");
            return false;
        }
        
        if(!MainSchedules.Any(item => item.Checked)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.SaveSelectedSchedules");
            return false;
        }
        
        var schedule = MainSchedules
            .FirstOrDefault(item => item.Checked && string.IsNullOrEmpty(item.TypeSchedule));

        if(schedule is null) {
            ErrorText = null;
            return true;
        }
        
        if(string.IsNullOrEmpty(schedule.TypeSchedule)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.EmptyTypeSchedule", schedule.Name);
            return false;
        }
        
        ErrorText = null;
        return true;
    }

    private void FilterSchedules() {
        FilteredSchedules = string.IsNullOrWhiteSpace(FilterSchedulesText)
            ? FilteredSchedules = MainSchedules
            : [
                .. MainSchedules
                    .Where(item => FilterText(item.Name, FilterSchedulesText!))
            ];
    }

    private void SelectSaveFolder() {
        OpenFolderDialogService.InitialDirectory =
            SaveFolder ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        
        if(OpenFolderDialogService.ShowDialog()) {
            SaveFolder = OpenFolderDialogService.Folder.FullName;
        }
    }

    /// <summary>
    /// Загрузка настроек плагина.
    /// </summary>
    private void LoadConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);
        SaveFolder = setting?.SaveFolder ?? Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        ClearSaveFolder = setting?.ClearSaveFolder ?? false;
    }

    /// <summary>
    /// Сохранение настроек плагина.
    /// </summary>
    private void SaveConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);
        
        setting.SaveFolder = SaveFolder;
        setting?.ClearSaveFolder = ClearSaveFolder;
        
        _pluginConfig.SaveProjectConfig();
    }

    private static bool FilterText(string text, string filter) {
        return text.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
