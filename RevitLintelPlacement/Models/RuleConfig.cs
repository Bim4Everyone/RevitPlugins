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

        public RulesType RulesType { get; set; }

        public string Name { get; set; }

        public static RuleConfig GetRuleConfig(string documentName) {
            if(File.Exists(GetDocumentConfigPath(documentName))) {
                return JsonConvert.DeserializeObject<RuleConfig>(File.ReadAllText(GetDocumentConfigPath(documentName)));
            }
            if(File.Exists(GetConfigPath())) {
                return JsonConvert.DeserializeObject<RuleConfig>(File.ReadAllText(GetConfigPath()));
            }
            return new RuleConfig();
        }

        public static Dictionary<string, RuleConfig> GetRuleConfigs(string documentName) {
            var ruleDictionary = new Dictionary<string, RuleConfig>();

            var projectConfig = GetProjectConfig(documentName);
            ruleDictionary.Add(documentName, projectConfig);

            Directory.CreateDirectory(GetLintelsUserConfigPath());
            var files = Directory.GetFiles(GetLintelsUserConfigPath(), "*.json");
            if(files.Count() == 0)
                return ruleDictionary;
            foreach(var file in files) {
                var userConfig = GetUserConfig(file);
                if(!ruleDictionary.ContainsKey(userConfig.Name)) {
                    ruleDictionary.Add(userConfig.Name, userConfig);
                }
            }
            return ruleDictionary;
        }

        public void Save(string documentName) {
            Directory.CreateDirectory(Path.GetDirectoryName(GetDocumentConfigPath(documentName)));
            File.WriteAllText(GetDocumentConfigPath(documentName), JsonConvert.SerializeObject(this));
        }

        public void SaveAs(string name) {
            Directory.CreateDirectory(GetLintelsUserConfigPath());
            Name = name;
            File.WriteAllText(Path.Combine(GetLintelsUserConfigPath(), Name + $"_{Environment.UserName}" + ".json"), JsonConvert.SerializeObject(this));
        }

        public static RuleConfig LoadConfigFromFile(string path) {
            if(File.Exists(path)) {
                try {
                    var loadedConfig = JsonConvert.DeserializeObject<RuleConfig>(File.ReadAllText(path));
                    if(loadedConfig != null) {
                        loadedConfig.СonfigureConfig(Path.GetFileNameWithoutExtension(path), RulesType.User);
                        return loadedConfig;
                    }
                } catch {}
            }
            var emptyConfig = new RuleConfig();
            emptyConfig.СonfigureConfig(Path.GetFileNameWithoutExtension(path), RulesType.User);
            return emptyConfig;
        }

        public static RuleConfig CreateNewConfig() {
            var commonConfig = GeCommonConfig();
            commonConfig.СonfigureConfig(string.Empty, RulesType.New);
            return commonConfig;
        }

        private static RuleConfig GeCommonConfig() {
            if(File.Exists(GetConfigPath())) {
                var commonConfig = JsonConvert.DeserializeObject<RuleConfig>(File.ReadAllText(GetConfigPath()));
                if(commonConfig != null) {
                    commonConfig.СonfigureConfig(nameof(RuleConfig), RulesType.Common);
                    return commonConfig;
                }
            }
            var emptyCommonConfig = new RuleConfig();
            emptyCommonConfig.СonfigureConfig(nameof(RuleConfig), RulesType.Common);
            return emptyCommonConfig;
        }

        private static RuleConfig GetProjectConfig(string documentName) {
            if(File.Exists(GetDocumentConfigPath(documentName))) {
                var projectCpnfig = JsonConvert.DeserializeObject<RuleConfig>(File.ReadAllText(GetDocumentConfigPath(documentName)));
                if(projectCpnfig != null) {
                    projectCpnfig.СonfigureConfig(documentName, RulesType.Project);
                    return projectCpnfig;
                }
            }
            var emptyProjectConfig = new RuleConfig();
            emptyProjectConfig.СonfigureConfig(documentName, RulesType.Project);
            return emptyProjectConfig;
        }

        private static RuleConfig GetUserConfig(string file) {
            var userConfig = JsonConvert.DeserializeObject<RuleConfig>(File.ReadAllText(file));
            if(userConfig == null) {
                userConfig = new RuleConfig();
            }
            userConfig.СonfigureConfig(userConfig.Name, RulesType.User);
            return userConfig;
        }

        private void СonfigureConfig(string name, RulesType rulesType) {
            if(name != null) { 
                Name = name;
            }
            RulesType = rulesType;
        }

        private static string GetConfigPath() {
            return Path.Combine(GetLintelsCommonConfigPath(), nameof(RuleConfig) + ".json");
        }

        private static string GetDocumentConfigPath(string documentName) {
            return Path.Combine(GetLintelsCommonConfigPath(), documentName + ".json");
        }

        private static string GetLintelsCommonConfigPath() {
            var projectConfigPath = RevitRepository.ProfilePath;
            var pluginName = nameof(RevitLintelPlacement);
            var revitVersion = string.IsNullOrEmpty(ModuleEnvironment.RevitVersion) ? "2020" : ModuleEnvironment.RevitVersion;
            return Path.Combine(projectConfigPath, revitVersion, pluginName, "Rules");
        }

        private static string GetLintelsUserConfigPath() {
            var revitVersion = string.IsNullOrEmpty(ModuleEnvironment.RevitVersion) ? "2020" : ModuleEnvironment.RevitVersion;
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "dosymep", revitVersion, nameof(RevitLintelPlacement), "Rules");
        }
    }

    public enum RulesType {
        Common,
        Project,
        User,
        New
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
