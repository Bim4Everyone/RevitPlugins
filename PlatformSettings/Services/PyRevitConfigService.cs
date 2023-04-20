using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

using PlatformSettings.Model;

namespace PlatformSettings.Services {
    internal class PyRevitConfigService : IPyRevitConfigService {
        public static readonly string PyRevitConfigPath =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                @"pyRevit\pyRevit_config.ini");


        private IniFile _iniFile;

        public PyRevitConfigService() {
            _iniFile = new IniFile(PyRevitConfigPath);
        }

        public bool IsEnabledExtension(Extension extension) {
            string disabled = _iniFile.Read($"{extension.Name}.{extension.Type}", "disabled");
            return string.IsNullOrEmpty(disabled) ? false : !bool.Parse(disabled);
        }

        public void ToggleExtension(Extension extension) {
            if(IsEnabledExtension(extension)) {
                DisableExtension(extension);
            } else {
                EnableExtension(extension);
            }
        }

        public void EnableExtension(Extension extension) {
            _iniFile.Write($"{extension.Name}.{extension.Type}", "disabled", "false");
        }

        public void DisableExtension(Extension extension) {
            _iniFile.Write($"{extension.Name}.{extension.Type}", "disabled", "true");
        }

        public bool IsInstalledExtension(Extension extension) {
            throw new NotSupportedException();
        }

        public void InstallExtension(Extension extension) {
            EnableExtension(extension);
            var type = extension.Type == "extension" ? "ui" : "lib";
            PyRevitCliStart($"extend {type} {extension.Name} {extension.Url.Value}");
        }

        public void RemoveExtension(Extension extension) {
            PyRevitCliStart($"extensions delete {extension.Name}");
            _iniFile.RemoveSection($"{extension.Name}.{extension.Type}");
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