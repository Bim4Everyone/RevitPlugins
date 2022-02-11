using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

namespace RevitLintelPlacement.Models {
    internal class LintelsConfig : ProjectConfig {
        [JsonIgnore]
        public override string ProjectConfigPath { get; set; }
        [JsonIgnore]
        public override IConfigSerializer Serializer { get; set; }

        public string LintelsConfigPath { get; set; } = @"T:\Проектный институт\Отдел стандартизации BIM и RD\BIM-Отдел\Тестирование\Перемычки\TestConfig";

        public List<string> RulesCongigPaths { get; set; } = new List<string> ();

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
        public List<string> ReinforcedConcreteFilter { get; set; } = new List<string>();
        public string HolesFilter { get; set; }
        public List<string> LintelFamilies { get; set; } = new List<string>();

        [JsonIgnore]
        public Dictionary<string, StorageType> ParamterType { get; set; } = new Dictionary<string, StorageType>() {
            {nameof(LintelThickness), StorageType.Double },
            {nameof(LintelWidth), StorageType.Double },
            {nameof(LintelRightOffset), StorageType.Double },
            {nameof(LintelLeftOffset), StorageType.Double },
            {nameof(LintelLeftCorner), StorageType.Integer },
            {nameof(LintelRightCorner), StorageType.Integer },
            {nameof(LintelFixation), StorageType.Integer },
            {nameof(OpeningHeight), StorageType.Double },
            {nameof(OpeningWidth), StorageType.Double },
            {nameof(OpeningFixation), StorageType.Integer }
        };

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
