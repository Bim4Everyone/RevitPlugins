using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using pyRevitLabs.Json;

namespace RevitLintelPlacement.Models {
    public class RuleConfig {
        public List<RulesSettigs> RuleSettingsConfig { get; set; } = new List<RulesSettigs>();

        public void AddRulesSettings(RulesSettigs ruleSettingConfig) {
            if(RuleSettingsConfig.Count > 10) {
                foreach(int index in Enumerable.Range(0, RuleSettingsConfig.Count - 10)) {
                    RuleSettingsConfig.RemoveAt(index);
                }
            }

            RuleSettingsConfig.Add(ruleSettingConfig);
        }

        public IEnumerable<RulesSettigs> GetRuleSettingsConfig(string documentName) {
            documentName = string.IsNullOrEmpty(documentName) ? "Без имени.rvt" : documentName;
            return RuleSettingsConfig.Where(item => documentName.Equals(item.DocumentName) || item.IsSystem);
        }

        private static string GetConfigPath() {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "dosymep", "RevitLintelPlacement", "RevitLintelPlacement.json");
        }


        public static RuleConfig GetConfig() {
            RuleConfig ruleConfig;
            if(File.Exists(GetConfigPath())) {
                ruleConfig= JsonConvert.DeserializeObject<RuleConfig>(File.ReadAllText(GetConfigPath()));
            } else {
                ruleConfig = new RuleConfig();
            }
            var rulesSetting = new RulesTemplateInitializer().GetTemplateRules();
            ruleConfig.AddRulesSettings(rulesSetting);
            return ruleConfig;
        }

        public static void SaveConfig(RuleConfig config) {
            Directory.CreateDirectory(Path.GetDirectoryName(GetConfigPath()));
            File.WriteAllText(GetConfigPath(), JsonConvert.SerializeObject(config.RuleSettingsConfig.Where(e => !e.IsSystem)));
        }
    }

    public class RulesSettigs {
        public bool IsSystem { get; set; }
        public string DocumentName { get; set; }
        public List<RuleSetting> RuleSettings { get; set; } = new List<RuleSetting>();
    }


    public class RuleSetting {
        public string Name { get; set; }
        public List<ConditionSetting> ConditionSettingsConfig { get; set; } = new List<ConditionSetting>();
        public List<LintelParameterSetting> LintelParameterSettingsConfig { get; set; } = new List<LintelParameterSetting>();
    }

    public class ConditionSetting {
        public double OpeningWidthMin { get; set; }
        public double OpeningWidthMax { get; set; }
        public ConditionType ConditionType { get; set; }
        public List<string> WallTypes { get; set; }
    }

    public enum ConditionType {
        WallTypes,
        OpeningWidth
    }

    public enum LintelParameterType {
        TypeNameParameter,
        LeftOffsetParameter,
        RightOffsetParameter,
        CornerParameter
    }

    public class LintelParameterSetting {
        public bool IsCornerChecked { get; set; }
        public double Offset{ get; set; }
        public double HalfThickness { get; set; }
        public double OpeningWidth { get; set; }
        public LintelParameterType LintelParameterType { get; set; }
        public string LintelTypeName { get; set; }
    }
}
