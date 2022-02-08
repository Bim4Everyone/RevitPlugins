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
    internal class LintelsConfig : ProjectConfig {
        private readonly string _lintelsConfigPathDefault = @"C:\Test";
        private readonly string _rulesConfigPathDefault = @"C:\TestRules";
        private string _lintelsConfigPath;

        [JsonIgnore]
        public override string ProjectConfigPath { get; set; }
        [JsonIgnore]
        public override IConfigSerializer Serializer { get; set; }

        public string LintelsConfigPath {
            get {
                if(string.IsNullOrEmpty(_lintelsConfigPath))
                    return _lintelsConfigPathDefault;
                return _lintelsConfigPath;
            }
            set => _lintelsConfigPath = value; }

        public List<string> RulesCongigPaths { get; set; } = new List<string>();

        public static LintelsConfig GetLintelsConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new ConfigSerializer())
                .SetPluginName(nameof(RevitLintelPlacement))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(LintelsConfig) + ".json")
                .Build<LintelsConfig>();
        }
    }

    internal class LintelsCommonConfig {
        public string LintelThickness { get; set; }
        public string LintelWidth { get; set; }
        public string LintelRightOffset { get; set; }
        public string LintelLeftOffset { get; set; }
        public string LintelLeftCorner { get; set; }
        public string LintelRightCorner { get; set; }
        public string LintelFixation { get; set; }
        public string OpeningHeight { get; set; }
        public string OpeningWidth { get; set; }
        public string OpeningFixation { get; set; }
        public string ReinforcedConcreteFilter { get; set; }
        public string HolesFilter { get; set; }
        public List<string> LintelFamilies { get; set; } = new List<string>();

        public static LintelsCommonConfig GetLintelsCommonConfig(string path) {
            if(File.Exists(GetConfigPath(path))) {
                return JsonConvert.DeserializeObject<LintelsCommonConfig>(File.ReadAllText(GetConfigPath(path)));
            }
            return new LintelsCommonConfig();
        }

        public void Save(string path) {
            Directory.CreateDirectory(Path.GetDirectoryName(GetConfigPath(path)));
            File.WriteAllText(GetConfigPath(path), JsonConvert.SerializeObject(this));
        }

        public static string GetConfigPath(string configPath) {
            return Path.Combine(configPath, nameof(RevitLintelPlacement), nameof(LintelsCommonConfig) + ".json");
        }
    }
}
