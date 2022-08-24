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

using RevitClashDetective.Models.Interfaces;

using RevitLintelPlacement.Models.Interfaces;

namespace RevitLintelPlacement.Models {
    public class RuleConfig : ProjectConfig, INamedEntity {
        public string Name {
            get => !string.IsNullOrEmpty(ProjectConfigPath) ? Path.GetFileNameWithoutExtension(ProjectConfigPath) : null;
            set { }
        }

        public RuleConfig() {}

        public List<GroupedRuleSettings> RuleSettings { get; set; } = new List<GroupedRuleSettings>();

        [JsonIgnore]
        public override string ProjectConfigPath { get; set; }
        [JsonIgnore]
        public override IConfigSerializer Serializer { get; set; } = new ConfigSerializer();

        public static RuleConfig GetLocalRuleConfig(string name) {
            return new RuleConfig() { ProjectConfigPath = Path.Combine(RevitRepository.LocalRulePath, name + ".json") };
        }

        public static RuleConfig GetEmptyProjectConfig(string projectPath) {
            return new RuleConfig() { ProjectConfigPath = projectPath };
        }

        public static RuleConfig GetRuleConfigs(string configPath) {
            var configLoader = new ConfigLoader();
            var config = configLoader.Load<RuleConfig>(configPath);
            return config;
        }

        public void UpdateToLocalPath() {
            ProjectConfigPath = Path.Combine(RevitRepository.LocalRulePath, Name + ".json");
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
        public double Offset { get; set; }
        public LintelParameterType LintelParameterType { get; set; }
    }
}
