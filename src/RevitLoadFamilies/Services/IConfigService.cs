using System.Collections.Generic;

using RevitLoadFamilies.Models;

namespace RevitLoadFamilies.Services;
internal interface IConfigService {
    IEnumerable<FamilyConfig> GetConfigurations();
}
