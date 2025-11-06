using System.Collections.Generic;

using RevitLoadFamilies.Models;

namespace RevitLoadFamilies.Services.ConfigServices;
internal interface IConfigService {
    string GetConfigurationFolderPath();
    IEnumerable<FamilyConfig> GetConfigurations(string configurationFolderPath);
}
