using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RevitLintelPlacement.Models.Interfaces;

namespace RevitLintelPlacement.Models.RuleConfigManagers {
    internal class RuleConfigManager : IRuleConfigManager {
        public bool CanSave { get; set; }
        public bool CanRename { get; set; }
        public bool CanDelete { get; set; }

        public static RuleConfigManager GetLocalConfigManager() {
            return new RuleConfigManager() { CanRename = true, CanSave = true, CanDelete = true };
        }

        public static RuleConfigManager GetProjectConfigManager() {
            return new RuleConfigManager() { CanRename = false, CanSave = true, CanDelete = false };
        }

        public static RuleConfigManager GetTemplateConfigManager() {
            return new RuleConfigManager() { CanRename = false, CanSave = false, CanDelete = false };
        }

        public RuleConfig Copy(RuleConfig config, IEnumerable<RuleConfig> configs) {
            if(config is null) {
                throw new ArgumentNullException(nameof(config));
            }

            if(configs is null) {
                throw new ArgumentNullException(nameof(configs));
            }

            var name = Renamer.GetName(config, configs);
            var directory = RevitRepository.LocalRulePath;

            var copyConfig = RuleConfig.GetRuleConfigs(config.ProjectConfigPath);
            copyConfig.ProjectConfigPath = Path.Combine(directory, name + ".json");
            return copyConfig;
        }

        public void Delete(RuleConfig config) {
            if(File.Exists(config.ProjectConfigPath) && config.ProjectConfigPath.EndsWith(".json", StringComparison.CurrentCultureIgnoreCase)) {
                File.Delete(config.ProjectConfigPath);
            }
        }

        public RuleConfig Load(string path) {
            return RuleConfig.GetRuleConfigs(path);
        }

        public void Rename(RuleConfig config, string name) {
            if(File.Exists(config.ProjectConfigPath)) {
                File.Move(config.ProjectConfigPath, Path.Combine(Path.GetDirectoryName(config.ProjectConfigPath), name + ".json"));
            }
        }

        public void Save(RuleConfig config) {
            config.SaveProjectConfig();
        }

        public void SaveAs(RuleConfig config, string path) {
            var saver = new ConfigSaver();
            saver.Save(config, path);
        }
    }
}