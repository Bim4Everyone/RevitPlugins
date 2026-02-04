using System.Collections.Generic;

using pyRevitLabs.Json;
namespace RevitUnmodelingMep.Models;

internal class UnmodelingSettingsDocument {
    public Dictionary<string, UnmodelingConfigItem> UnmodelingConfig { get; set; } = new();

    public UnmodelingSettingsOptions UnmodelingSettings { get; set; } = new();

    [JsonProperty("UNMODELING")]
    public Dictionary<string, UnmodelingLegacyItem> Unmodeling { get; set; } = new();

    [JsonExtensionData]
    public IDictionary<string, object> ExtensionData { get; set; }
}

internal class UnmodelingSettingsOptions {
    public bool OnlyProjectInstances { get; set; } = true;

    [JsonExtensionData]
    public IDictionary<string, object> ExtensionData { get; set; }
}

internal class UnmodelingConfigItem {
    public string ConfigName { get; set; }

    public string Category { get; set; }

    public string Group { get; set; }

    public string Name { get; set; }

    public string Mark { get; set; }

    public string Code { get; set; }

    public string Unit { get; set; }

    public string Creator { get; set; }

    public string ValueFormula { get; set; }

    public string NoteFormat { get; set; }

    public List<int> AssignedElementIds { get; set; } = new();

    public bool RoundUpTotal { get; set; }

    [JsonExtensionData]
    public IDictionary<string, object> ExtensionData { get; set; }
}

internal class UnmodelingLegacyItem {
    [JsonProperty("UNIT")]
    public string Unit { get; set; }

    [JsonProperty("CODE")]
    public string Code { get; set; }

    [JsonProperty("NAME")]
    public string Name { get; set; }

    [JsonProperty("MARK")]
    public string Mark { get; set; }

    [JsonProperty("CREATOR")]
    public string Creator { get; set; }

    [JsonExtensionData]
    public IDictionary<string, object> ExtensionData { get; set; }
}
