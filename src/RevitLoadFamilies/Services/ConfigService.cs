using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

using Autodesk.Revit;
using Autodesk.Revit.DB;

using Microsoft.Win32;

using RevitLoadFamilies.Models;

namespace RevitLoadFamilies.Services;

internal class ConfigService : IConfigService {
    private readonly RevitRepository _revitRepository;

    public ConfigService(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }

    public IEnumerable<FamilyConfig> GetConfigurations() {
        string basePath = GetConfigurationsFolderPath();

        if(string.IsNullOrEmpty(basePath)) {
            return Enumerable.Empty<FamilyConfig>();
        }

        return LoadConfigurationsFromFolder(basePath);
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
        // Получаем версию Revit из продукта
        string productVersion = _revitRepository.Application.VersionNumber;

        // Извлекаем год из версии (например, "2022" из "2022.1.1")
        string year = new string(productVersion.Take(4).ToArray());
        return year;
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
