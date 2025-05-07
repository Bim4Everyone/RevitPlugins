using System;
using System.Collections.Generic;
using System.IO;

using dosymep.Bim4Everyone;
using dosymep.Revit.FileInfo;

using RevitRsnManager.Interfaces;
using RevitRsnManager.Models.Utils;

namespace RevitRsnManager.Models
{
    public class RsnConfigService : IRsnConfigService {
        private readonly string _version;
        private readonly string _revitServerIni;

        public RsnConfigService() {
            _version = ModuleEnvironment.RevitVersion;

            string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            _revitServerIni = Path.Combine(programData, "Autodesk", $"Revit Server {_version}", "Config", "RSN.ini");
        }

        public List<string> LoadServersFromIni() {
            var servers = new List<string>();

            if(!File.Exists(_revitServerIni)) {
                Directory.CreateDirectory(Path.GetDirectoryName(_revitServerIni));

                File.WriteAllText(_revitServerIni, string.Empty);
                return servers;
            }

            foreach(string line in File.ReadLines(_revitServerIni)) {
                string trimmed = line.Trim();
                if(!string.IsNullOrWhiteSpace(trimmed) && !trimmed.StartsWith("[") && !trimmed.Contains("=")) {
                    servers.Add(trimmed);
                }
            }

            return servers;
        }

        public void SaveServersToIni(List<string> servers) {
            using(var writer = new StreamWriter(_revitServerIni, false)) {
                foreach(string server in servers) {
                    string trimmed = server?.Trim();
                    if(!string.IsNullOrWhiteSpace(trimmed)) {
                        writer.WriteLine(trimmed);
                    }
                }
            }
        }

        public string GetProjectPathFromRevitIni() {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string iniPath = Path.Combine(appData, "Autodesk", "Revit", $"Autodesk Revit {_version}", "Revit.ini");

            var ini = new IniConfigurationService(iniPath);
            string rawPath = ini.Read("Directories", "ProjectPath");

            return Environment.ExpandEnvironmentVariables(rawPath);
        }
    }
}
