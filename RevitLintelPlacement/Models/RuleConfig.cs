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
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "dosymep", "RevitLintelPlacement", "RevitLintelPlacement.json");
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
        public string Name { get; set; }
        public string DocumentName { get; set; }
        public List<ConditionSettingConfig> ConditionSettingsConfig { get; set; } = new List<ConditionSettingConfig>();
        public List<LintelParameterSettingConfig> LintelParameterSettingsConfig { get; set; } = new List<LintelParameterSettingConfig>();
    }

    public class ConditionSettingConfig {
        public double OpeningWidthMin { get; set; }
        public double OpeningWidthMax { get; set; }
        public ConditionType ConditionType { get; set; }

        //TODO: возможно тут нужен материал, а не тип стены, еще не решили (выбрать между этим свойством и WallMaterials)
        public List<string> WallTypes { get; set; }
        public List<string> WallMaterialClasses { get; set; }
        public List<string> WallMaterials { get; set; }
        public List<string> ExclusionWallTypes { get; set; }
        //TODO: Добавить еще условий
    }

    public enum ConditionType {
        WallTypes,
        OpeningWidth,
        WallMaterialClasses,
        ExclusionWallTypes,
    }

    public enum LintelParameterType {
        YesNoLintelParameter,
        NumberParameter,
        ReletiveOpeneingParameter,
        RelativeWallParameter,
    }

    public class LintelParameterSettingConfig {
        public bool IsChecked { get; set; }
        public double RelationValue { get; set; }
        public double NumberValue { get; set; }
        public LintelParameterType LintelParameterType { get; set; }
        public string Name { get; set; }
        public string OpeninigParameterName { get; set; }
        public string WallParameterName { get; set; }
    }
}
