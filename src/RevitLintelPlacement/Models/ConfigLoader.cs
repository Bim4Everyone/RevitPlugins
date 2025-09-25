using System;
using System.IO;

using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

namespace RevitLintelPlacement.Models;

internal class ConfigLoader {
    private readonly IConfigSerializer _serializer = new ConfigSerializer();

    public T Load<T>(string configPath) where T : ProjectConfig, new() {
        if(string.IsNullOrEmpty(configPath)) {
            throw new ArgumentException($"'{nameof(configPath)}' cannot be null or empty.", nameof(configPath));
        }

        if(!File.Exists(configPath)) {
            throw new ArgumentException($"Путь \"{configPath}\" недоступен.", nameof(configPath));
        }

        string fileContent = File.ReadAllText(configPath);

        var projectConfig = _serializer.Deserialize<T>(fileContent);
        projectConfig.Serializer = _serializer;
        projectConfig.ProjectConfigPath = configPath;

        return projectConfig;
    }
}
