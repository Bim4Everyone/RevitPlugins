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

        File.WriteAllText(configPath, _serializer.Serialize(lintelConfigRule));
    }
}
