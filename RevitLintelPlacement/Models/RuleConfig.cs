using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using pyRevitLabs.Json;

namespace RevitLintelPlacement.Models {
    public class RuleConfig {
        public List<RuleSettingsConfig> RuleSettingsConfig { get; set; } = new List<RuleSettingsConfig>();

        public void AddRulesSettings(RuleSettingsConfig ruleSettingConfig) {
            if(RuleSettingsConfig.Count > 10) {
                foreach(int index in Enumerable.Range(0, RuleSettingsConfig.Count - 10)) {
                    RuleSettingsConfig.RemoveAt(index);
                }
            }

            RuleSettingsConfig.Add(ruleSettingConfig);
        }

        public RuleSettingsConfig GetRuleSettingsConfig(string documentName) {
            documentName = string.IsNullOrEmpty(documentName) ? "Без имени.rvt" : documentName;
            return RuleSettingsConfig.FirstOrDefault(item => documentName.Equals(item.DocumentName));
        }

        private static string GetConfigPath() {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "dosymep", "RevitLintelPlacement", "PrintConfig.json");
        }

        public static RuleConfig GetConfig() {
            if(File.Exists(GetConfigPath())) {
                return JsonConvert.DeserializeObject<RuleConfig>(File.ReadAllText(GetConfigPath()));
            }

            return new RuleConfig();
        }

        public static void SaveConfig(RuleConfig config) {
            Directory.CreateDirectory(Path.GetDirectoryName(GetConfigPath()));
            File.WriteAllText(GetConfigPath(), JsonConvert.SerializeObject(config));
        }
    }

    public class RuleSettingsConfig {
        public string DocumentName { get; set; }
        public ConditionSettingConfig ConditionSettingConfig { get; set; }
    }

    public class ConditionSettingConfig {

        //TODO: возможно тут нужен материал, а не тип стены, еще не решили (выбрать между этим свойством и WallMaterialCollection)
        public List<string> WallTypeCollection { get; set; }
        public List<string> WallMaterialCollection { get; set; }
        public double OpeningWidthMin { get; set; }
        public double OpeningWidthMax { get; set; }
        public ConditionType ConditionType { get; set; }
        //TODO: Добавить еще условий
    }

    public enum ConditionType {
        WallTypeCondition,
        OpeningWidth,
    }
}
