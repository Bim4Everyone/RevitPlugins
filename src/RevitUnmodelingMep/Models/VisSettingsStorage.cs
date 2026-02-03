using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.Templates;
using dosymep.Revit;
using dosymep.SimpleServices;

using pyRevitLabs.Json.Linq;

namespace RevitUnmodelingMep.Models;

internal class VisSettingsStorage {
    private readonly Document _doc;
    private readonly ILocalizationService _localizationService;
    private readonly IConfigSerializer _configSerializer;

    public VisSettingsStorage(
        Document doc,
        ILocalizationService localizationService,
        IConfigSerializer configSerializer) {
        _doc = doc;
        _localizationService = localizationService;
        _configSerializer = configSerializer;
    }

    internal static string GetLibFolder() {
        string dllDir = GetAssemblyDirectory();
        string fallbackLibPath = Path.GetFullPath(Path.Combine(
            dllDir,
            "..",
            "Extensions",
            "04.OV-VK.extension",
            "lib"));

        if(!Directory.Exists(fallbackLibPath)) {
            throw new DirectoryNotFoundException(
                $"lib folder not found at expected path: \"{fallbackLibPath}\". " +
                "Expected structure: ...\\pyRevit\\2022 with ..\\Extensions\\04.OV-VK.extension\\lib.");
        }

        return fallbackLibPath;
    }

    public UnmodelingSettingsDocument GetUnmodelingSettings() {
        string value = (string) _doc.ProjectInformation
            .GetSharedParamValue(SharedParamsConfig.Instance.VISSettings.Name);

        return ParseSettings(value);
    }

    public void PrepareSettings() {
        UnmodelingSettingsDocument LoadDefaultSettings() {
            string defaultsPath = Path.Combine(GetLibFolder(), "default_spec_settings.json");
            return ParseSettings(File.ReadAllText(defaultsPath));
        }

        ProjectParameters projectParameters = ProjectParameters.Create(_doc.Application);
        projectParameters.SetupRevitParam(_doc, SharedParamsConfig.Instance.VISSettings);

        string settingsText =
            _doc.ProjectInformation.GetParamValueOrDefault(
                SharedParamsConfig.Instance.VISSettings, string.Empty);

        if(string.IsNullOrWhiteSpace(settingsText)) {
            UnmodelingSettingsDocument defaultSettings = LoadDefaultSettings();
            using(var tr =
                new Transaction(_doc, _localizationService.GetLocalizedString("VisSettingsStorage.TransactionName"))) {
                tr.Start();

                _doc.ProjectInformation.SetParamValue(
                    SharedParamsConfig.Instance.VISSettings,
                    _configSerializer.Serialize(defaultSettings));

                tr.Commit();
            }
        }
    }

    public UnmodelingSettingsDocument GetDefaultSettings() {
        string defaultsPath = Path.Combine(GetLibFolder(), "default_spec_settings.json");
        return ParseSettings(File.ReadAllText(defaultsPath));
    }

    public void SaveUnmodelingSettings(UnmodelingSettingsDocument settings) {
        string settingsText = _configSerializer.Serialize(settings ?? new UnmodelingSettingsDocument());
        SaveSettings(settingsText);
    }

    private UnmodelingSettingsDocument ParseSettings(string settingsText) {
        try {
            if(string.IsNullOrWhiteSpace(settingsText)) {
                return new UnmodelingSettingsDocument();
            }

            return _configSerializer.Deserialize<UnmodelingSettingsDocument>(settingsText)
                   ?? new UnmodelingSettingsDocument();
        } catch {
            return new UnmodelingSettingsDocument();
        }
    }

    private static string GetAssemblyDirectory() {
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

        return Path.GetDirectoryName(assemblyPath)
               ?? AppDomain.CurrentDomain.BaseDirectory;
    }

    private void SaveSettings(string settingsText) {
        using(var tr = new Transaction(_doc,
            _localizationService.GetLocalizedString("VisSettingsStorage.TransactionName"))) {
            tr.Start();

            _doc.ProjectInformation.SetParamValue(
                SharedParamsConfig.Instance.VISSettings,
                settingsText);

            tr.Commit();
        }
    }
}
