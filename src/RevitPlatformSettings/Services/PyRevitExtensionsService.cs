using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using RevitPlatformSettings.Model;

namespace RevitPlatformSettings.Services {
    internal class PyRevitExtensionsService : IPyRevitExtensionsService {
        public static readonly string PyRevitConfigPath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"pyRevit\pyRevit_config.ini");


        private IniFile _iniFile;
        private readonly string[] _extensionsPaths;

        public PyRevitExtensionsService() {
            _iniFile = new IniFile(PyRevitConfigPath);
            _extensionsPaths = GetExtensionsPath()
                .Distinct()
                .Where(item => Directory.Exists(item))
                .SelectMany(item => Directory.GetDirectories(item))
                .Select(item => Path.GetFileName(item))
                .ToArray();
        }

        public bool IsEnabledExtension(Extension extension) {
            if(extension is BuiltinExtension) {
                string disabled = _iniFile.Read($"{extension.Name}.{extension.Type}", "disabled");
                return string.IsNullOrEmpty(disabled) ? false : !bool.Parse(disabled);
            } else if(extension is ThirdPartyExtension) {
                return IsInstalledExtension(extension);
            } else {
                throw new NotSupportedException();
            }
        }

        public void ToggleExtension(Extension extension) {
            if(IsEnabledExtension(extension)) {
                DisableExtension(extension);
            } else {
                EnableExtension(extension);
            }
        }

        public void EnableExtension(Extension extension) {
            if(extension is BuiltinExtension) {
                _iniFile.Write($"{extension.Name}.{extension.Type}", "disabled", "false");
            } else if(extension is ThirdPartyExtension) {
                InstallExtension(extension);
                _iniFile.Write($"{extension.Name}.{extension.Type}", "disabled", "false");
            } else {
                throw new NotSupportedException();
            }
        }

        public void DisableExtension(Extension extension) {
            if(extension is BuiltinExtension) {
                _iniFile.Write($"{extension.Name}.{extension.Type}", "disabled", "true");
            } else if(extension is ThirdPartyExtension) {
                RemoveExtension(extension);
                _iniFile.RemoveSection($"{extension.Name}.{extension.Type}");
            } else {
                throw new NotSupportedException();
            }
        }

        public bool IsInstalledExtension(Extension extension) {
            return _extensionsPaths.Contains($"{extension.Name}.{extension.Type}", StringComparer.OrdinalIgnoreCase);
        }

        private void InstallExtension(Extension extension) {
            var type = extension.Type == "extension" ? "ui" : "lib";
            PyRevitCliStart($"extend {type} {extension.Name} {extension.Url.Value}");
        }

        private void RemoveExtension(Extension extension) {
            PyRevitCliStart($"extensions delete {extension.Name}");
        }

        private IEnumerable<string> GetExtensionsPath() {
            yield return BuiltinExtensionsService.ExtensionsPath;
            yield return ThirdPartyExtensionsService.ExtensionsPath;

            var extensionsPaths = _iniFile.Read("core", "userextensions")
                .Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Trim(']').Trim('[').Trim('"').Trim());

            foreach(string extensionsPath in extensionsPaths) {
                yield return extensionsPath;
            }
        }

        private void PyRevitCliStart(string args) {
            Process.Start(new ProcessStartInfo() {
                CreateNoWindow = true,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                FileName = GetPyRevitCliPath(),
                Arguments = args
            })?.WaitForExit();
        }

        private string GetPyRevitCliPath() {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "pyRevit-Master", "bin", "pyrevit.exe");
        }

        private class IniFile {
            private readonly string _iniPath;

            [DllImport("kernel32", CharSet = CharSet.Unicode)]
            private static extern long WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString,
                string lpFileName);

            [DllImport("kernel32", CharSet = CharSet.Unicode)]
            private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault,
                StringBuilder lpReturnedString, int nSize, string lpFileName);

            public IniFile(string iniPath) {
                _iniPath = iniPath;
            }

            public bool KeyExists(string section, string key) {
                return Read(section, key).Length > 0;
            }

            public string Read(string section, string key) {
                var retVal = new StringBuilder(255);
                GetPrivateProfileString(section, key, "", retVal, 255, _iniPath);
                return retVal.ToString();
            }

            public void Write(string section, string key, string value) {
                WritePrivateProfileString(section, key, value, _iniPath);
            }

            public void DeleteKey(string section, string key) {
                Write(section, key, null);
            }

            public void RemoveSection(string section) {
                Write(section, null, null);
            }
        }
    }
}