using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using RevitLoadFamilies.Models;

namespace RevitLoadFamilies.Services;
internal class ConfigService : IConfigService {
    private readonly List<FamilyConfig> _configurations;

    public ConfigService() {
        _configurations = new List<FamilyConfig>();
        InitializeDefaultConfig();
    }

    private void InitializeDefaultConfig() {
        var defaultConfig = new FamilyConfig("Базовая конфигурация") {
            FamilyPaths = new List<string>
            {
                @"C:\Users\Divin_n\Desktop\IFC_Каркас_Пилон_№2_Типовой.rfa",
                @"C:\Users\Divin_n\Desktop\IFC_Каркас_Пилон_№2_Типовой_Copy.rfa"
            }
        };
        _configurations.Add(defaultConfig);
    }

    public FamilyConfig GetDefaultConfig() {
        return _configurations.FirstOrDefault();
    }

    /// <summary>
    /// Возвращает все известные конфигурации
    /// </summary>
    /// <returns></returns>
    public List<FamilyConfig> GetConfigurations() {
        return _configurations;
    }

    /// <summary>
    /// Добавляет конфигурацию, если конфигурации с таким именем еще нет
    /// </summary>
    /// <param name="config"></param>
    public void AddConfig(FamilyConfig config) {
        if(_configurations.All(c => c.Name != config.Name)) {
            _configurations.Add(config);
        }
    }

    /// <summary>
    /// Удаляет конфигурацию о имени, если такая найдена
    /// </summary>
    /// <param name="name">Имя конфигурации</param>
    public void RemoveConfig(string name) {
        var config = _configurations.FirstOrDefault(c => c.Name == name);
        if(config != null) {
            _configurations.Remove(config);
        }
    }

    public void ExportConfig(FamilyConfig config, string filePath) {
        try {
            File.WriteAllLines(filePath, config.FamilyPaths);
        } catch(Exception ex) {
            throw new Exception($"Ошибка экспорта конфигурации: {ex.Message}");
        }
    }

    public FamilyConfig ImportConfig(string filePath) {
        try {
            var configName = Path.GetFileNameWithoutExtension(filePath);
            var paths = File.ReadAllLines(filePath)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            var config = new FamilyConfig(configName) {
                FamilyPaths = paths
            };

            // Если среди существующих конфигураций есть конфиг с таким же именем, то удаляем существующий конфиг 
            RemoveConfig(configName);
            // Добавляем импортированный конфиг
            AddConfig(config);
            return config;
        } catch(Exception ex) {
            throw new Exception($"Ошибка импорта конфигурации: {ex.Message}");
        }
    }
}
