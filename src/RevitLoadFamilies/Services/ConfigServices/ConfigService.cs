using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

using RevitLoadFamilies.Models;

namespace RevitLoadFamilies.Services.ConfigServices;

internal class ConfigService : IConfigService {
    private readonly RevitRepository _revitRepository;

    public ConfigService(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }

    public IEnumerable<FamilyConfig> GetConfigurations(string configurationFolderPath) {
        // Проверяем сохраненный путь, если он не действителен, тогда получаем стандартный
        if (string.IsNullOrEmpty(configurationFolderPath) || !Directory.Exists(configurationFolderPath)) {
            configurationFolderPath = GetConfigurationsFolderPath();
        }
        // Если стандартный путь также не действителен, тогда запрашиваем выбор папки
        if(string.IsNullOrEmpty(configurationFolderPath) || !Directory.Exists(configurationFolderPath)) {
            throw new ArgumentException();
        }
        return LoadConfigurationsFromFolder(configurationFolderPath);
    }

    private string GetConfigurationsFolderPath() {
        // Получаем версию Revit
        string revitVersion = GetRevitVersion();

        // Формируем путь к папке конфигураций
        string configPath = $@"W:\Проектный институт\Отд.стандарт.BIM и RD\BIM-Ресурсы\5-Надстройки\Bim4Everyone\A101\{revitVersion}\RevitLoadFamilies";

        // Проверяем существование папки
        if(Directory.Exists(configPath)) {
            return configPath;
        }

        // Если папки нет, показываем диалог выбора
        return ShowFolderSelectionDialog();
    }

    private string GetRevitVersion() {
        return _revitRepository.Application.VersionNumber;
    }

    private string ShowFolderSelectionDialog() {
        var dialog = new System.Windows.Forms.FolderBrowserDialog {
            Description = "Выберите папку с конфигурациями (.txt файлы)",
            ShowNewFolderButton = false
        };

        if(dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
            return dialog.SelectedPath;
        } else {
            // Пользователь нажал отмену
            MessageBox.Show(
                "Папка с конфигурациями не выбрана. Плагин будет закрыт.",
                "Внимание",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return null;
        }
    }

    private IEnumerable<FamilyConfig> LoadConfigurationsFromFolder(string folderPath) {
        var configurations = new List<FamilyConfig>();
        try {
            // Ищем все .txt файлы в папке
            var txtFiles = Directory.GetFiles(folderPath, "*.txt");

            foreach(var filePath in txtFiles) {
                var config = CreateConfigFromFile(filePath);
                if(config != null) {
                    configurations.Add(config);
                }
            }
        } catch(Exception ex) {
            MessageBox.Show(
                $"Ошибка при чтении конфигураций: {ex.Message}",
                "Ошибка",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        return configurations;
    }

    private FamilyConfig CreateConfigFromFile(string filePath) {
        try {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            var familyPaths = File.ReadAllLines(filePath)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            return new FamilyConfig(fileName) {
                FamilyPaths = familyPaths
            };
        } catch(Exception) {
            // В случае ошибки пропускаем файл
            return null;
        }
    }
}
