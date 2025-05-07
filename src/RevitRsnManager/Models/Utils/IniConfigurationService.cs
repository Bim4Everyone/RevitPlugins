using System;
using System.Runtime.InteropServices;
using System.Text;

namespace RevitRsnManager.Models.Utils
{
    public class IniConfigurationService
    {
        private readonly string _iniPath;

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern long WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

        [DllImport("kernel32", CharSet = CharSet.Unicode)]
        private static extern int GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);

        public IniConfigurationService(string iniPath) {
            _iniPath = iniPath;
        }

        public bool KeyExists(string section, string key) {
            string text = Read(section, key);
            if(text == null) {
                return false;
            }

            return text.Length > 0;
        }

        public int? ReadInt(string section, string key, int? @default = null) {
            if(int.TryParse(Read(section, key), out var result)) {
                return result;
            }

            return @default;
        }

        public bool? ReadBool(string section, string key, bool? @default = null) {
            if(bool.TryParse(Read(section, key), out var result)) {
                return result;
            }

            return @default;
        }

        public TEnum? ReadEnum<TEnum>(string section, string key, TEnum? @default = null) where TEnum : struct {
            if(Enum.TryParse<TEnum>(Read(section, key), out var result)) {
                return result;
            }

            return @default;
        }

        public string Read(string section, string key, string @default = null) {
            StringBuilder stringBuilder = new StringBuilder(255);
            GetPrivateProfileString(section, key, string.Empty, stringBuilder, 255, _iniPath);
            if(!string.IsNullOrEmpty(stringBuilder.ToString())) {
                return stringBuilder.ToString();
            }

            return @default;
        }

        public void Write(string section, string key, string value) {
            WritePrivateProfileString(section, key, value, _iniPath);
        }

        public void RemoveSection(string section) {
            Write(section, null, null);
        }

        public void DeleteKey(string section, string key) {
            Write(section, key, null);
        }
    }
}
