using System.Collections.Generic;

using RevitLoadFamilies.Models;

namespace RevitLoadFamilies.Services;
internal interface IConfigService {
    FamilyConfig GetDefaultConfig();
}
