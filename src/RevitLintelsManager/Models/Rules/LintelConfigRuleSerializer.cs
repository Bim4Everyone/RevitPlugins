using System;
using System.IO;

using dosymep.Bim4Everyone.ProjectConfigs;

namespace RevitLintelsManager.Models.Rules;

public class LintelConfigRuleSerializer {
    private readonly IConfigSerializer _serializer;
    
    public LintelConfigRuleSerializer(IConfigSerializer serializer) {
        _serializer = serializer;
    }

    public LintelConfigRule LoadConfigRule (string configPath) {
        if(string.IsNullOrEmpty(configPath) || !File.Exists(configPath)) {
            return null;
        }
        string fileContent = File.ReadAllText(configPath);
        return _serializer.Deserialize<LintelConfigRule>(fileContent);
    }
    
    public void SaveConfigRule (LintelConfigRule lintelConfigRule, string configPath) {
        if(lintelConfigRule is null) {
            throw new ArgumentNullException(nameof(lintelConfigRule));
        }

        if(string.IsNullOrEmpty(configPath)) {
            throw new ArgumentException(nameof(configPath));
        }

        string directory = Path.GetDirectoryName(configPath);

        if(!string.IsNullOrEmpty(directory)) {
            Directory.CreateDirectory(directory);
        }

        string temporaryPath = configPath + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try {
            File.WriteAllText(temporaryPath, _serializer.Serialize(lintelConfigRule));
            if(File.Exists(configPath)) {
                File.Replace(temporaryPath, configPath, null);
            } else {
                File.Move(temporaryPath, configPath);
            }
        } finally {
            if(File.Exists(temporaryPath)) {
                File.Delete(temporaryPath);
            }
        }
    }
}
