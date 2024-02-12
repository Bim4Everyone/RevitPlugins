using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Serializers;

using pyRevitLabs.Json;

using RevitLintelPlacement.ViewModels;

namespace RevitLintelPlacement.Models {
    internal class LintelsConfig : ProjectConfig<LintelsSettings> {
        [JsonIgnore]
        public override string ProjectConfigPath { get; set; }
        [JsonIgnore]
        public override IConfigSerializer Serializer { get; set; }

        public static LintelsConfig GetLintelsConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new ConfigSerializer())
                .SetPluginName(nameof(RevitLintelPlacement))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(LintelsConfig) + ".json")
                .Build<LintelsConfig>();
        }
    }

    internal class LintelsSettings : ProjectSettings {
        public override string ProjectName { get; set; }
        public string SelectedModeNameRules { get; set; }
        public SampleMode SelectedModeRules { get; set; }
        public SampleMode SelectedModeNavigator { get; set; }
        public string SelectedModeNameNavigator { get; set; }
        public string SelectedPath { get; set; }
        public List<string> SelectedLinks { get; set; } = new List<string>();
    }


    internal class LintelsCommonConfig {
        [Description("Толщина перемычки")]
        public string LintelThickness { get; set; }

        [Description("Ширина перемычки")]
        public string LintelWidth { get; set; }

        [Description("Опирание справа")]
        public string LintelRightOffset { get; set; }

        [Description("Опирание слева")]
        public string LintelLeftOffset { get; set; }

        [Description("Уголок слева")]
        public string LintelLeftCorner { get; set; }

        [Description("Уголок справа")]
        public string LintelRightCorner { get; set; }

        [Description("Фиксация решения")]
        public string LintelFixation { get; set; }

        [JsonIgnore]
        [Description("Высота проема")]
        // не стал переделывать всю логику, оставил как есть
        public string OpeningHeight => LabelUtils.GetLabelFor(BuiltInParameter.INSTANCE_HEAD_HEIGHT_PARAM);

        [JsonIgnore]
        [Description("Ширина проема")]
        // не стал переделывать всю логику, оставил как есть
        public string OpeningWidth => LabelUtils.GetLabelFor(BuiltInParameter.FAMILY_WIDTH_PARAM);
        
        [Description("Фиксация перемычки")]
        public string OpeningFixation { get; set; }

        public List<string> ReinforcedConcreteFilter { get; set; } = new List<string>();
        [Description("Фильтр отверстий")]
        public string HolesFilter { get; set; }
        public string LintelFamily { get; set; }

        public bool IsEmpty() {
            return LintelThickness == null
                || LintelWidth == null
                || LintelRightOffset == null
                || LintelLeftOffset == null
                || LintelLeftCorner == null
                || LintelRightCorner == null
                || LintelFixation == null
                || OpeningFixation == null
                || ReinforcedConcreteFilter == null
                || HolesFilter == null
                || LintelFamily == null;
        }

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

        public static LintelsCommonConfig GetLintelsCommonConfig(string documentName) {
            if(File.Exists(GetDocumentConfigPath(documentName))) {
                return JsonConvert.DeserializeObject<LintelsCommonConfig>(File.ReadAllText(GetDocumentConfigPath(documentName)));
            }
            if(File.Exists(GetConfigPath())) {
                return JsonConvert.DeserializeObject<LintelsCommonConfig>(File.ReadAllText(GetConfigPath()));
            }
            return new LintelsCommonConfig();
        }

        public void Save(string documentName) {
            Directory.CreateDirectory(Path.GetDirectoryName(GetDocumentConfigPath(documentName)));
            File.WriteAllText(GetDocumentConfigPath(documentName), JsonConvert.SerializeObject(this));
        }

        private static string GetConfigPath() {
            return Path.Combine(GetLintelsCommonConfigPath(), nameof(LintelsCommonConfig) + ".json");
        }

        private static string GetDocumentConfigPath(string documentName) {
            return Path.Combine(GetLintelsCommonConfigPath(), documentName + ".json");
        }

        private static string GetLintelsCommonConfigPath() {
            var projectConfigPath = RevitRepository.ProfilePath;
            var pluginName = nameof(RevitLintelPlacement);
            var revitVersion = string.IsNullOrEmpty(ModuleEnvironment.RevitVersion) ? "2020" : ModuleEnvironment.RevitVersion;
            return Path.Combine(projectConfigPath, revitVersion, pluginName);
        }
    }
}
