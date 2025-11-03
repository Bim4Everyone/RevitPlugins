using System.Collections.Generic;

using RevitLoadFamilies.Models;

namespace RevitLoadFamilies.Services;
public class ConfigService : IConfigService {
    public FamilyConfig GetDefaultConfig() {
        var config = new FamilyConfig("Базовая конфигурация") {
            FamilyPaths = new List<string>
            {
                @"C:\Users\Divin_n\Desktop\IFC_Каркас_Пилон_№2_Типовой.rfa",
                @"C:\Users\Divin_n\Desktop\IFC_Каркас_Пилон_№2_Типовой_Copy.rfa"
            }
        };
        return config;
    }
}
