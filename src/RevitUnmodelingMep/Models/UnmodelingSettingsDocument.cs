using System.Collections.Generic;

using pyRevitLabs.Json;
using pyRevitLabs.Json.Linq;

namespace RevitUnmodelingMep.Models;

internal class UnmodelingSettingsDocument {
    [JsonProperty("UNMODELING_CONFIG")]
    public Dictionary<string, UnmodelingConfigItem> UnmodelingConfig { get; set; } = new();

    [JsonProperty("UNMODELING_SETTINGS")]
    public UnmodelingSettingsOptions UnmodelingSettings { get; set; } = new();

    [JsonProperty("UNMODELING")]
    public Dictionary<string, UnmodelingLegacyItem> Unmodeling { get; set; } = new();

    [JsonExtensionData]
    public IDictionary<string, JToken> ExtensionData { get; set; }
}

internal class UnmodelingSettingsOptions {
    [JsonProperty("ONLY_PROJECT_INSTANCES")]
    public bool OnlyProjectInstances { get; set; } = true;

    [JsonExtensionData]
    public IDictionary<string, JToken> ExtensionData { get; set; }
}

internal class UnmodelingConfigItem {
    [JsonProperty("CONFIG_NAME")]
    public string ConfigName { get; set; }

    [JsonProperty("CATEGORY")]
    public string Category { get; set; }

    [JsonProperty("GROUP")]
    public string Group { get; set; }

    [JsonProperty("NAME")]
    public string Name { get; set; }

    [JsonProperty("MARK")]
    public string Mark { get; set; }

    [JsonProperty("CODE")]
    public string Code { get; set; }

    [JsonProperty("UNIT")]
    public string Unit { get; set; }

    [JsonProperty("CREATOR")]
    public string Creator { get; set; }

    [JsonProperty("VALUE_FORMULA")]
    public string ValueFormula { get; set; }

    [JsonProperty("NOTE_FORMAT")]
    public string NoteFormat { get; set; }

    [JsonProperty("ASSIGNED_ELEMENT_IDS")]
    public List<int> AssignedElementIds { get; set; } = new();

    [JsonProperty("ROUND_UP_TOTAL")]
    public bool RoundUpTotal { get; set; }

    [JsonExtensionData]
    public IDictionary<string, JToken> ExtensionData { get; set; }
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
    public IDictionary<string, JToken> ExtensionData { get; set; }
}
