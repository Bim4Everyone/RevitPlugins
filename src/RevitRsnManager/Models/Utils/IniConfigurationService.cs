using System;
using System.Runtime.InteropServices;
using System.Text;

namespace RevitRsnManager.Models.Utils;

public class IniConfigurationService {
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
        return text != null && text.Length > 0;
    }

    public int? ReadInt(string section, string key, int? @default = null) {
        return int.TryParse(Read(section, key), out int result) ? result : @default;
    }

    public bool? ReadBool(string section, string key, bool? @default = null) {
        return bool.TryParse(Read(section, key), out bool result) ? result : @default;
    }

    public TEnum? ReadEnum<TEnum>(string section, string key, TEnum? @default = null) where TEnum : struct {
        return Enum.TryParse<TEnum>(Read(section, key), out var result) ? result : @default;
    }

    public string Read(string section, string key, string @default = null) {
        var stringBuilder = new StringBuilder(255);
        _ = GetPrivateProfileString(section, key, string.Empty, stringBuilder, 255, _iniPath);
        return !string.IsNullOrEmpty(stringBuilder.ToString()) ? stringBuilder.ToString() : @default;
    }

    public void Write(string section, string key, string value) {
        _ = WritePrivateProfileString(section, key, value, _iniPath);
    }

    public void RemoveSection(string section) {
        Write(section, null, null);
    }

    public void DeleteKey(string section, string key) {
        Write(section, key, null);
    }
}
