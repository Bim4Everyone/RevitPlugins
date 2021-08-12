using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.Bim4Everyone;

namespace PlatformSettings.SharedParams {
    public abstract class RevitParams {
        public string Name { get; set; }
        public string KeyName { get; set; }
        public string ConfigFileName { get; set; }

        public abstract RevitParamsConfig GetConfig();

        public string GetConfigPath() {
            string sharedParamsPath = pyRevitLabs.PyRevit.PyRevitConfigs.GetConfigFile().GetValue("PlatformSettings", KeyName);
            return string.IsNullOrEmpty(sharedParamsPath) ? null : new System.IO.FileInfo(sharedParamsPath.Trim('\"')).FullName;
        }

        public void SaveSettings(RevitParamsConfig revitParamsConfig, string configPath) {
            if(!string.IsNullOrEmpty(configPath)) {
                revitParamsConfig.Save(configPath);
                pyRevitLabs.PyRevit.PyRevitConfigs.GetConfigFile().SetValue("PlatformSettings", KeyName, $"\"{configPath}\"");
            }
        }
    }
}
