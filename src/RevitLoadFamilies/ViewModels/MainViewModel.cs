using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
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
    private FamilyConfig _selectedConfig;

    public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository, 
                         ILocalizationService localizationService) {
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;

        _configService = new ConfigService();
        _familyLoadService = new FamilyLoadService();

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        
        UpdateFamilyPathsCommand = RelayCommand.Create(UpdateFamilyPaths);
    }

    public ICommand LoadViewCommand { get; }
    public ICommand AcceptViewCommand { get; }
    public ICommand UpdateFamilyPathsCommand { get; }


    public ObservableCollection<FamilyConfig> Configurations { get; set; }

    public FamilyConfig SelectedConfig {
        get => _selectedConfig;
        set => RaiseAndSetIfChanged(ref _selectedConfig, value);
    }

    public ObservableCollection<string> FamilyPaths { get; set; } = [];

    /// <summary>
    /// Текст ошибки, который отображается при неверном вводе пользователя.
    /// </summary>
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }


    private void LoadView() {
        LoadConfig();

        // Загружаем конфигурации
        Configurations = [_configService.GetDefaultConfig()];

        if(Configurations.Any()) {
            SelectedConfig = Configurations.First();
        }

        UpdateFamilyPaths();
    }

    private void AcceptView() {
        SaveConfig();
        LoadFamilies();
    }

    private bool CanAcceptView() {
        return SelectedConfig != null && SelectedConfig.FamilyPaths.Any();
    }


    /// <summary>
    /// Загрузка настроек плагина.
    /// </summary>
    private void LoadConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);
    }

    /// <summary>
    /// Сохранение настроек плагина.
    /// </summary>
    private void SaveConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);
        _pluginConfig.SaveProjectConfig();
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
    /// Загрузка семейств по путям из выбранной конфигурации
    /// </summary>
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
