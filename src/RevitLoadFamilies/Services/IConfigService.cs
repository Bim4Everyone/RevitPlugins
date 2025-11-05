using System.Collections.Generic;

using RevitLoadFamilies.Models;

namespace RevitLoadFamilies.Services;
internal interface IConfigService {
    FamilyConfig GetDefaultConfig();
    void ExportConfig(FamilyConfig config, string filePath);
    FamilyConfig ImportConfig(string filePath);
    List<FamilyConfig> GetConfigurations();
    void AddConfig(FamilyConfig config);
    void RemoveConfig(string name);
}
