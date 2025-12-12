using System;
using System.Collections.Generic;
using System.IO;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RevitUnmodelingMep.Models; 

internal class SettingsUpdater {
    private readonly Document _doc;

    public SettingsUpdater(Document doc) {
        _doc = doc;
    }

    public void PrepareSettings() {
        JObject LoadDefaultSettings() {
            string scriptDir = Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().Location);

            string extRoot = Path.GetFullPath(
                Path.Combine(scriptDir, "..", "..", "..", ".."));

            string defaultsPath = Path.Combine(
                extRoot, "lib", "default_spec_settings.json");

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

        JObject data;
        try {
            data = JObject.Parse(settingsText);
        } catch {
            return null;
        }

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

        JObject data;
        try {
            data = JObject.Parse(settingsText);
        } catch {
            data = new JObject();
        }

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
}
