using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Microsoft.Win32;

using RevitLoadFamilies.Models;
using RevitLoadFamilies.Services;

namespace RevitLoadFamilies.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private readonly IConfigService _configService;
    private readonly IFamilyLoadService _familyLoadService;

    private string _errorText;
    private string _saveProperty;
    private FamilyConfig _selectedConfig;

    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    /// <param name="pluginConfig">Настройки плагина.</param>
    /// <param name="revitRepository">Класс доступа к интерфейсу Revit.</param>
    /// <param name="localizationService">Интерфейс доступа к сервису локализации.</param>
    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService) {

        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;

        _configService = new ConfigService();
        _familyLoadService = new FamilyLoadService();

        // Загружаем конфигурации
        Configurations = new ObservableCollection<FamilyConfig>(_configService.GetConfigurations());
        if(Configurations.Any()) {
            SelectedConfig = Configurations.First();
        }

        FamilyPaths = new ObservableCollection<string>();

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        
        ImportConfigCommand = RelayCommand.Create(ImportConfig);
        ExportConfigCommand = RelayCommand.Create(ExportConfig, CanExportConfig);
        UpdateFamilyPathsCommand = RelayCommand.Create(UpdateFamilyPaths);
    }

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
    /// Команда добавления пути к семейству.
    /// </summary>
    public ICommand AddFamilyPathCommand { get; }

    /// <summary>
    /// Команда удаления пути к семейству.
    /// </summary>
    public ICommand RemoveFamilyPathCommand { get; }

    /// <summary>
    /// Команда импорта конфигурации.
    /// </summary>
    public ICommand ImportConfigCommand { get; }

    /// <summary>
    /// Команда экспорта конфигурации.
    /// </summary>
    public ICommand ExportConfigCommand { get; }

    /// <summary>
    /// Команда обновления списка путей семейств.
    /// </summary>
    public ICommand UpdateFamilyPathsCommand { get; }


    public ObservableCollection<FamilyConfig> Configurations { get; set; }

    public FamilyConfig SelectedConfig {
        get => _selectedConfig;
        set => RaiseAndSetIfChanged(ref _selectedConfig, value);
    }

    public ObservableCollection<string> FamilyPaths { get; set; }

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



    /// <summary>
    /// Метод загрузки главного окна.
    /// </summary>
    /// <remarks>В данном методе должна происходить загрузка настроек окна, а так же инициализация полей окна.</remarks>
    private void LoadView() {
        LoadConfig();
        UpdateFamilyPaths();
    }

    /// <summary>
    /// Метод применения настроек главного окна. (выполнение плагина)
    /// </summary>
    /// <remarks>
    /// В данном методе должны браться настройки пользователя и сохраняться в конфиг, а так же быть основной код плагина.
    /// </remarks>
    private void AcceptView() {
        SaveConfig();
        LoadFamilies();
    }


    /// <summary>
    /// Метод импорта конфигурации.
    /// </summary>
    private void ImportConfig() {
        var openFileDialog = new OpenFileDialog {
            Filter = "Текстовые файлы (*.txt)|*.txt",
            Multiselect = false,
            Title = "Выберите файл конфигурации для импорта"
        };

        if(openFileDialog.ShowDialog() == true) {
            try {
                var importedConfig = _configService.ImportConfig(openFileDialog.FileName);

                // Обновляем коллекцию конфигураций
                Configurations.Clear();
                foreach(var config in _configService.GetConfigurations()) {
                    Configurations.Add(config);
                }
                
                // Выбираем импортированную конфигурацию
                SelectedConfig = importedConfig;

                MessageBox.Show(
                    $"Конфигурация '{importedConfig.Name}' успешно импортирована.",
                    "Импорт завершен",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            } catch(Exception ex) {
                MessageBox.Show(
                    ex.Message,
                    "Ошибка импорта",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// Метод экспорта выбранной конфигурации.
    /// </summary>
    private void ExportConfig() {
        if(SelectedConfig == null)
            return;

        var saveFileDialog = new SaveFileDialog {
            Filter = "Текстовые файлы (*.txt)|*.txt",
            FileName = $"{SelectedConfig.Name}.txt",
            Title = "Экспорт конфигурации"
        };

        if(saveFileDialog.ShowDialog() == true) {
            try {
                _configService.ExportConfig(SelectedConfig, saveFileDialog.FileName);

                MessageBox.Show(
                    $"Конфигурация '{SelectedConfig.Name}' успешно экспортирована.",
                    "Экспорт завершен",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            } catch(Exception ex) {
                MessageBox.Show(
                    ex.Message,
                    "Ошибка экспорта",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    /// <summary>
    /// Обновление списка путей при смене конфигурации.
    /// </summary>
    private void UpdateFamilyPaths() {
        FamilyPaths.Clear();
        if(SelectedConfig != null) {
            foreach(var path in SelectedConfig.FamilyPaths) {
                FamilyPaths.Add(path);
            }
        }
    }


    /// <summary>
    /// Проверка возможности экспорта конфигурации.
    /// </summary>
    private bool CanExportConfig() {
        return SelectedConfig != null && SelectedConfig.FamilyPaths.Any();
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
        return SelectedConfig != null && SelectedConfig.FamilyPaths.Any();
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


    private void LoadFamilies() {
        if(SelectedConfig == null)
            return;

        using var transaction = _revitRepository.Document.StartTransaction("LoadFamilies");

        int successCount = 0;
        int fileIsNotExistsCount = 0;
        int errorCount = 0;

        foreach(var filePath in SelectedConfig.FamilyPaths) {
            try {
                if(File.Exists(filePath)) {
                    if(_familyLoadService.LoadFamily(filePath, _revitRepository.Document)) {
                        successCount++;
                    } else {
                        errorCount++;
                    }
                } else {
                    fileIsNotExistsCount++;
                }
            } catch(Exception) {
                errorCount++;
            }
        }

        transaction.Commit();

        MessageBox.Show(
            $"Загрузка завершена:\nУспешно: {successCount}\nНе найдено: {fileIsNotExistsCount}\nОшибок: {errorCount}",
            "Результат загрузки",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
}
