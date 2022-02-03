using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

namespace RevitLintelPlacement.Models {
    public class RuleConfig : ProjectConfig<RulesSettigs> {
        [JsonIgnore]
        public override string ProjectConfigPath { get; set; }

        [JsonIgnore]
        public override IConfigSerializer Serializer { get; set; }

        public static RuleConfig GetRuleConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new ConfigSerializer())
                .SetPluginName("RevitLintelPlacement")
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(RuleConfig) + ".json")
                .Build<RuleConfig>();
        }

    }

    public class RulesSettigs : ProjectSettings {
        public override string ProjectName { get; set; }
        public List<GroupedRuleSettings> RuleSettings { get; set; } = new List<GroupedRuleSettings>();
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
        public double HalfThickness { get; set; }
        public double OpeningWidth { get; set; }
        public LintelParameterType LintelParameterType { get; set; }
        public string LintelTypeName { get; set; }
    }

    public class ConfigSerializer : IConfigSerializer {
        public T Deserialize<T>(string text) {
            return JsonConvert.DeserializeObject<T>(text);
        }

        public string Serialize<T>(T @object) {
            return JsonConvert.SerializeObject(@object);
        }
    }

}
