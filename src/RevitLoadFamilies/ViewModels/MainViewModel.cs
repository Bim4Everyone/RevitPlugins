using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitLoadFamilies.Models;
using RevitLoadFamilies.Services;

using Microsoft.Win32;

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
    private string _selectedFamilyPath;

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

        // Загружаем встроенную конфигурацию
        CurrentConfig = _configService.GetDefaultConfig();
        FamilyPaths = new ObservableCollection<string>(CurrentConfig.FamilyPaths);

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        AddFamilyPathCommand = RelayCommand.Create(AddFamilyPath);
        RemoveFamilyPathCommand = RelayCommand.Create(RemoveFamilyPath, CanRemoveFamilyPath);
    }


    private void LoadFamilies() {
        using var transaction = _revitRepository.Document.StartTransaction("LoadFamilies");

        int successCount = 0;
        int fileIsNotExistsCount = 0;
        int errorCount = 0;

        foreach(var filePath in CurrentConfig.FamilyPaths) {
            try {
                if(File.Exists(filePath)) {
                    _familyLoadService.LoadFamily(filePath, _revitRepository.Document);
                    successCount++;
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

    public FamilyConfig CurrentConfig { get; set; }
    public ObservableCollection<string> FamilyPaths { get; set; }

    /// <summary>
    /// Выбранный путь к семейству в списке.
    /// </summary>
    public string SelectedFamilyPath {
        get => _selectedFamilyPath;
        set => RaiseAndSetIfChanged(ref _selectedFamilyPath, value);
    }

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
    /// Метод добавления пути к семейству.
    /// </summary>
    private void AddFamilyPath() {
        var openFileDialog = new OpenFileDialog {
            Filter = "Revit Families (*.rfa)|*.rfa",
            Multiselect = false,
            Title = "Выберите файл семейства Revit"
        };

        if(openFileDialog.ShowDialog() == true) {
            if(!string.IsNullOrEmpty(openFileDialog.FileName)) {
                // Проверяем, нет ли уже такого пути в списке
                if(!CurrentConfig.FamilyPaths.Contains(openFileDialog.FileName)) {
                    CurrentConfig.FamilyPaths.Add(openFileDialog.FileName);
                    FamilyPaths.Add(openFileDialog.FileName);
                } else {
                    MessageBox.Show(
                        "Данный файл уже добавлен в список.",
                        "Предупреждение",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
        }
    }

    /// <summary>
    /// Метод удаления пути к семейству.
    /// </summary>
    private void RemoveFamilyPath() {
        if(!string.IsNullOrEmpty(SelectedFamilyPath)) {
            CurrentConfig.FamilyPaths.Remove(SelectedFamilyPath);
            FamilyPaths.Remove(SelectedFamilyPath);
            SelectedFamilyPath = null;
        }
    }

    /// <summary>
    /// Проверка возможности удаления пути к семейству.
    /// </summary>
    private bool CanRemoveFamilyPath() {
        return !string.IsNullOrEmpty(SelectedFamilyPath);
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
        return CurrentConfig != null && CurrentConfig.FamilyPaths.Count > 0;
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
}
