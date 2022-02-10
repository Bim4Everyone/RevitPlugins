using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitLintelPlacement.Models {
    public class RuleConfig {

        public List<GroupedRuleSettings> RuleSettings { get; set; } = new List<GroupedRuleSettings>();

        public static RuleConfig GetRuleConfig(string path = null) {
            if(path != null && File.Exists(GetConfigPath(path))) {
                return JsonConvert.DeserializeObject<RuleConfig>(File.ReadAllText(GetConfigPath(path)));
            }
            return new RuleConfig();
        }

        public static RuleConfig GetRuleFromFile(string path = null) {
            if(path == null || File.Exists(path)) {
                return JsonConvert.DeserializeObject<RuleConfig>(File.ReadAllText(path));
            }
            return new RuleConfig();
        }

        public void Save(string path) {
            Directory.CreateDirectory(Path.GetDirectoryName(GetConfigPath(path)));
            File.WriteAllText(GetConfigPath(path), JsonConvert.SerializeObject(this));
        }

        public static string GetConfigPath(string configPath) {
            return Path.Combine(configPath, nameof(RevitLintelPlacement), nameof(RuleConfig) + ".json");
        }
    }

    public class GroupedRuleSettings {
        public string Name { get; set; }
        public ConditionSetting WallTypes { get; set; }
        public List<RuleSetting> Rules { get; set; }
    }

    public class RuleSetting {
        public string LintelTypeName { get; set; }
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
        LeftOffsetParameter,
        RightOffsetParameter,
        CornerParameter
    }

    public class LintelParameterSetting {
        public bool IsCornerChecked { get; set; }
        public double Offset{ get; set; }
        public LintelParameterType LintelParameterType { get; set; }
    }

}
