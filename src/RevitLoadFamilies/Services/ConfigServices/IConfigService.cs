using System.Collections.Generic;

using RevitLoadFamilies.Models;

namespace RevitLoadFamilies.Services.ConfigServices;
internal interface IConfigService {
    IEnumerable<FamilyConfig> GetConfigurations(string configurationFolderPath);
}
