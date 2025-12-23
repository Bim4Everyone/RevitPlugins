using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.Templates;
using dosymep.Revit;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Wpf.Ui.Controls;

namespace RevitUnmodelingMep.Models; 

internal class VisSettingsStorage {
    private readonly Document _doc;

    public VisSettingsStorage(Document doc) {
        _doc = doc;
    }

    public JObject GetUnmodelingConfig() {
        string value = (string) _doc.ProjectInformation
            .GetSharedParamValue(SharedParamsConfig.Instance.VISSettings.Name);

        return ParseSettings(value);
    }

    public void PrepareSettings() {
        JObject LoadDefaultSettings() {
            var assembly = Assembly.GetExecutingAssembly();
            string assemblyPath = string.Empty;

            if(!string.IsNullOrWhiteSpace(assembly.CodeBase)) {
                assemblyPath = new Uri(assembly.CodeBase).LocalPath;
            }

            if(string.IsNullOrWhiteSpace(assemblyPath) && !string.IsNullOrWhiteSpace(assembly.Location)) {
                assemblyPath = assembly.Location;
            }

            if(string.IsNullOrWhiteSpace(assemblyPath)) {
                assemblyPath = AppDomain.CurrentDomain.BaseDirectory;
            }

            string dllDir = Path.GetDirectoryName(assemblyPath)
                            ?? AppDomain.CurrentDomain.BaseDirectory;

            // Основной путь: профиль pyRevit -> Extensions -> 04.OV-VK.extension -> lib
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string defaultLibPath = Path.Combine(appData, "pyRevit", "Extensions", "04.OV-VK.extension", "lib");

            // Fallback: если dll действительно лежит в ...\ОВиВК.tab\bin, поднимаемся на две директории вверх и заходим в lib
            string fallbackLibPath = Path.GetFullPath(Path.Combine(dllDir, "..", "..", "lib"));

            string libDir = Directory.Exists(defaultLibPath) ? defaultLibPath : fallbackLibPath;
            string defaultsPath = Path.Combine(libDir, "default_spec_settings.json");

            return JObject.Parse(File.ReadAllText(defaultsPath));
        }

        JObject MergeSettings(JObject current, JObject defaults) {
            foreach(var prop in defaults.Properties()) {
                if(!current.ContainsKey(prop.Name)) {
                    current[prop.Name] = prop.Value.DeepClone();
                } else {
                    if(prop.Value.Type == JTokenType.Object
                       && current[prop.Name].Type == JTokenType.Object) {
                        MergeSettings(
                            (JObject) current[prop.Name],
                            (JObject) prop.Value);
                    }
                }
            }
            return current;
        }

        ProjectParameters projectParameters = ProjectParameters.Create(_doc.Application);
        projectParameters.SetupRevitParam(_doc, SharedParamsConfig.Instance.VISSettings);

        string settingsText =
            _doc.ProjectInformation.GetParamValueOrDefault(
                SharedParamsConfig.Instance.VISSettings, string.Empty);
         
        JObject defaultSettings = LoadDefaultSettings();

        JObject currentSettings;
        try {
            currentSettings = string.IsNullOrWhiteSpace(settingsText)
                ? new JObject()
                : JObject.Parse(settingsText);
        } catch {
            currentSettings = new JObject();
        }

        JObject settingsBeforeMerge =
            (JObject) currentSettings.DeepClone();

        JObject mergedSettings =
            MergeSettings(currentSettings, defaultSettings);

        bool settingsChanged =
            !JToken.DeepEquals(settingsBeforeMerge, mergedSettings)
            || string.IsNullOrWhiteSpace(settingsText);

        if(settingsChanged) {
            using(var tr =
                new Transaction(_doc, "BIM: Подготовка настроек")) {
                tr.Start();

                _doc.ProjectInformation.SetParamValue(
                    SharedParamsConfig.Instance.VISSettings,
                    JsonConvert.SerializeObject(
                        mergedSettings,
                        Formatting.Indented));

                tr.Commit();
            }
        }
    }

    public JToken GetSettingValue(IEnumerable<string> keyPath) {
        string settingsText =
            _doc.ProjectInformation.GetParamValueOrDefault(
                SharedParamsConfig.Instance.VISSettings, string.Empty);

        JObject data = ParseSettings(settingsText);
        JToken node = data;
        foreach(string key in keyPath) {
            if(node is JObject obj && obj.ContainsKey(key)) {
                node = obj[key];
            } else {
                return null;
            }
        }

        return node;
    }

    public string SetSettingValue(
        string settingsText,
        IList<string> keyPath,
        JToken newValue) {

        JObject data = ParseSettings(settingsText);

        JObject node = data;
        for(int i = 0; i < keyPath.Count - 1; i++) {
            string key = keyPath[i];
            if(!node.ContainsKey(key) || node[key].Type != JTokenType.Object) {
                node[key] = new JObject();
            }
            node = (JObject) node[key];
        }

        node[keyPath[keyPath.Count - 1]] = newValue;

        return JsonConvert.SerializeObject(data, Formatting.Indented);
    }

    public string SetSettingValue(IList<string> keyPath, JToken newValue) {
        string settingsText =
            _doc.ProjectInformation.GetParamValueOrDefault(
                SharedParamsConfig.Instance.VISSettings, string.Empty);

        string updatedSettings = SetSettingValue(settingsText, keyPath, newValue);
        SaveSettings(updatedSettings);
        return updatedSettings;
    }

    public JObject GetDefaultSettings() {
        var assembly = Assembly.GetExecutingAssembly();
        string assemblyPath = string.Empty;

        if(!string.IsNullOrWhiteSpace(assembly.CodeBase)) {
            assemblyPath = new Uri(assembly.CodeBase).LocalPath;
        }

        if(string.IsNullOrWhiteSpace(assemblyPath) && !string.IsNullOrWhiteSpace(assembly.Location)) {
            assemblyPath = assembly.Location;
        }

        if(string.IsNullOrWhiteSpace(assemblyPath)) {
            assemblyPath = AppDomain.CurrentDomain.BaseDirectory;
        }

        string dllDir = Path.GetDirectoryName(assemblyPath)
                        ?? AppDomain.CurrentDomain.BaseDirectory;

        string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        string defaultLibPath = Path.Combine(appData, "pyRevit", "Extensions", "04.OV-VK.extension", "lib");
        string fallbackLibPath = Path.GetFullPath(Path.Combine(dllDir, "..", "..", "lib"));

        string libDir = Directory.Exists(defaultLibPath) ? defaultLibPath : fallbackLibPath;
        string defaultsPath = Path.Combine(libDir, "default_spec_settings.json");

        return JObject.Parse(File.ReadAllText(defaultsPath));
    }

    public string RemoveSettingValue(string settingsText, IList<string> keyPath) {
        JObject data = ParseSettings(settingsText);

        JObject node = data;
        for(int i = 0; i < keyPath.Count - 1; i++) {
            string key = keyPath[i];
            if(node.ContainsKey(key) && node[key] is JObject nested) {
                node = nested;
            } else {
                return JsonConvert.SerializeObject(data, Formatting.Indented);
            }
        }

        node.Remove(keyPath[keyPath.Count - 1]);
        return JsonConvert.SerializeObject(data, Formatting.Indented);
    }

    public string RemoveSettingValue(IList<string> keyPath) {
        string settingsText =
            _doc.ProjectInformation.GetParamValueOrDefault(
                SharedParamsConfig.Instance.VISSettings, string.Empty);

        string updatedSettings = RemoveSettingValue(settingsText, keyPath);
        SaveSettings(updatedSettings);
        return updatedSettings;
    }

    private static JObject ParseSettings(string settingsText) {
        try {
            return string.IsNullOrWhiteSpace(settingsText)
                ? new JObject()
                : JObject.Parse(settingsText);
        } catch {
            return new JObject();
        }
    }

    private void SaveSettings(string settingsText) {
        using(var tr = new Transaction(_doc, "BIM: Update VIS settings")) {
            tr.Start();

            _doc.ProjectInformation.SetParamValue(
                SharedParamsConfig.Instance.VISSettings,
                settingsText);

            tr.Commit();
        }
    }
}
