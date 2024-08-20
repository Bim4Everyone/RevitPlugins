using System;
using System.IO;

using dosymep.Bim4Everyone.ProjectConfigs;

namespace RevitClashDetective.Models {
    public class ProjectConfigBuilder {

        private string _pluginName;

        private string _relativePath;

        private string _projectConfigName;

        private IConfigSerializer _serializer;

        private string _revitVersion;

        public ProjectConfigBuilder SetPluginName(string pluginName) {
            _pluginName = pluginName;
            return this;
        }

        public ProjectConfigBuilder SetRelativePath(string relativePath) {
            _relativePath = relativePath;
            return this;
        }

        public ProjectConfigBuilder SetProjectConfigName(string projectConfigName) {
            _projectConfigName = projectConfigName;
            return this;
        }

        public ProjectConfigBuilder SetSerializer(IConfigSerializer serializer) {
            _serializer = serializer;
            return this;
        }

        public ProjectConfigBuilder SetRevitVersion(string revitVersion) {
            _revitVersion = revitVersion;
            return this;
        }

        public T Build<T>()
            where T : ProjectConfig, new() {

            if(_serializer == null) {
                throw new InvalidOperationException("Перед конструированием объекта, требуется установить сериализатор.");
            }

            if(string.IsNullOrEmpty(_pluginName)) {
                throw new InvalidOperationException("Перед конструированием объекта, требуется установить наименование плагина.");
            }

            if(string.IsNullOrEmpty(_relativePath)) {
                throw new InvalidOperationException("Перед конструированием объекта, требуется установить путь к файлу конфигурации относительно папки плагина.");
            }

            if(string.IsNullOrEmpty(_projectConfigName)) {
                throw new InvalidOperationException("Перед конструированием объекта, требуется установить наименование файла конфигурации проекта.");
            }

            string projectConfigPath = GetConfigPath(Path.Combine(_pluginName, _relativePath), _projectConfigName, _revitVersion);
            if(File.Exists(projectConfigPath)) {
                string fileContent = File.ReadAllText(projectConfigPath);

                T projectConfig = _serializer.Deserialize<T>(fileContent);
                if(projectConfig != null) {
                    projectConfig.Serializer = _serializer;
                    projectConfig.ProjectConfigPath = projectConfigPath;

                    return projectConfig;
                }
            }

            return new T() { Serializer = _serializer, ProjectConfigPath = projectConfigPath };
        }

        private static string GetConfigPath(string pluginName, string projectConfigName, string revitVersion) {
            string _profilePath = RevitRepository.ProfilePath;
            return string.IsNullOrEmpty(revitVersion)
                ? Path.Combine(_profilePath, pluginName, projectConfigName)
                : Path.Combine(_profilePath, revitVersion, pluginName, projectConfigName);
        }
    }
}
