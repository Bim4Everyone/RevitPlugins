using System;
using System.Collections.Generic;

using dosymep.WPF.ViewModels;

using pyRevitLabs.Json.Linq;

using RevitUnmodelingMep.Models;

namespace RevitUnmodelingMep.ViewModels;

internal class ConsumableTypeItem : BaseViewModel {
    private string _title;
    private string _selectedType;
    private string _consumableTypeName;
    private string _grouping;
    private string _name;
    private string _mark;
    private string _code;
    private string _unit;
    private string _maker;
    private string _formula;
    private string _note;
    private bool _roundUpTotal;
    private CategoryOption _selectedCategory;

    public string Title {
        get => _title;
        set => RaiseAndSetIfChanged(ref _title, value);
    }

    public string ConfigKey { get; set; }

    public string SelectedType {
        get => _selectedType;
        set => RaiseAndSetIfChanged(ref _selectedType, value);
    }

    public string ConsumableTypeName {
        get => _consumableTypeName;
        set => RaiseAndSetIfChanged(ref _consumableTypeName, value);
    }

    public string CategoryId { get; set; }

    public string Grouping {
        get => _grouping;
        set => RaiseAndSetIfChanged(ref _grouping, value);
    }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public string Mark {
        get => _mark;
        set => RaiseAndSetIfChanged(ref _mark, value);
    }

    public string Code {
        get => _code;
        set => RaiseAndSetIfChanged(ref _code, value);
    }

    public string Unit {
        get => _unit;
        set => RaiseAndSetIfChanged(ref _unit, value);
    }

    public string Maker {
        get => _maker;
        set => RaiseAndSetIfChanged(ref _maker, value);
    }

    public string Formula {
        get => _formula;
        set => RaiseAndSetIfChanged(ref _formula, value);
    }

    public string Note {
        get => _note;
        set => RaiseAndSetIfChanged(ref _note, value);
    }

    public bool RoundUpTotal {
        get => _roundUpTotal;
        set => RaiseAndSetIfChanged(ref _roundUpTotal, value);
    }

    public CategoryOption SelectedCategory {
        get => _selectedCategory;
        set {
            RaiseAndSetIfChanged(ref _selectedCategory, value);
            CategoryId = value?.Id.ToString();
        }
    }

    public JArray AssignedElementIds { get; set; } = new JArray();

    public JObject RawConfig { get; set; } = new JObject();

    public static ConsumableTypeItem FromConfig(string configKey, UnmodelingConfigItem config) {
        config ??= new UnmodelingConfigItem();
        var item = new ConsumableTypeItem {
            ConfigKey = configKey,
            Title = config.ConfigName,
            ConsumableTypeName = config.ConfigName,
            Name = config.Name,
            CategoryId = config.Category,
            Grouping = config.Group,
            Mark = config.Mark,
            Code = config.Code,
            Unit = config.Unit,
            Maker = config.Creator,
            Formula = config.ValueFormula,
            Note = config.NoteFormat,
            RoundUpTotal = config.RoundUpTotal,
            AssignedElementIds = new JArray(config.AssignedElementIds ?? new List<int>()),
            RawConfig = new JObject()
        };

        if(config.ExtensionData != null) {
            foreach(var kvp in config.ExtensionData) {
                item.RawConfig[kvp.Key] = kvp.Value?.DeepClone();
            }
        }

        return item;
    }

    public static ConsumableTypeItem FromConfig(JProperty configProperty) {
        JObject value = configProperty.Value as JObject ?? new JObject();
        JObject clonedValue = (JObject) value.DeepClone();
        JArray assignedIds = value["ASSIGNED_ELEMENT_IDS"] as JArray ?? new JArray();

        return new ConsumableTypeItem {
            ConfigKey = configProperty.Name,
            Title = (string) value["CONFIG_NAME"],
            ConsumableTypeName = (string) value["CONFIG_NAME"],
            Name = (string) value["NAME"],
            CategoryId = (string) value["CATEGORY"],
            Grouping = (string) value["GROUP"],
            Mark = (string) value["MARK"],
            Code = (string) value["CODE"],
            Unit = (string) value["UNIT"],
            Maker = (string) value["CREATOR"],
            Formula = (string) value["VALUE_FORMULA"],
            Note = (string) value["NOTE_FORMAT"],
            RoundUpTotal = (bool?) value["ROUND_UP_TOTAL"] ?? false,
            AssignedElementIds = new JArray(assignedIds),
            RawConfig = clonedValue
        };
    }

    public JObject ToJObject() {
        JObject result = RawConfig != null
            ? (JObject) RawConfig.DeepClone()
            : new JObject();

        result["CONFIG_NAME"] = ConsumableTypeName ?? string.Empty;
        result["NAME"] = Name ?? string.Empty;
        result["CATEGORY"] = CategoryId ?? string.Empty;
        result["GROUP"] = Grouping ?? string.Empty;
        result["MARK"] = Mark ?? string.Empty;
        result["CODE"] = Code ?? string.Empty;
        result["UNIT"] = Unit ?? string.Empty;
        result["CREATOR"] = Maker ?? string.Empty;
        result.Remove("USE_CATEGORY_RESERVE");
        result["VALUE_FORMULA"] = Formula ?? string.Empty;
        result["NOTE_FORMAT"] = Note ?? string.Empty;
        result["ROUND_UP_TOTAL"] = RoundUpTotal;
        result["ASSIGNED_ELEMENT_IDS"] = AssignedElementIds ?? new JArray();

        return result;
    }

    public UnmodelingConfigItem ToConfigItem() {
        var result = new UnmodelingConfigItem {
            ConfigName = ConsumableTypeName ?? string.Empty,
            Name = Name ?? string.Empty,
            Category = CategoryId ?? string.Empty,
            Group = Grouping ?? string.Empty,
            Mark = Mark ?? string.Empty,
            Code = Code ?? string.Empty,
            Unit = Unit ?? string.Empty,
            Creator = Maker ?? string.Empty,
            ValueFormula = Formula ?? string.Empty,
            NoteFormat = Note ?? string.Empty,
            RoundUpTotal = RoundUpTotal,
            AssignedElementIds = ConvertAssignedElementIds()
        };

        if(RawConfig != null && RawConfig.HasValues) {
            var extensionData = new Dictionary<string, JToken>(StringComparer.OrdinalIgnoreCase);
            foreach(var property in RawConfig.Properties()) {
                if(!IsStandardConfigKey(property.Name)) {
                    extensionData[property.Name] = property.Value?.DeepClone();
                }
            }

            if(extensionData.Count > 0) {
                result.ExtensionData = extensionData;
            }
        }

        return result;
    }

    private List<int> ConvertAssignedElementIds() {
        var result = new List<int>();
        if(AssignedElementIds == null) {
            return result;
        }

        foreach(var token in AssignedElementIds) {
            if(token != null && int.TryParse(token.ToString(), out int value)) {
                result.Add(value);
            }
        }

        return result;
    }

    private static bool IsStandardConfigKey(string key) {
        switch(key) {
            case "CONFIG_NAME":
            case "NAME":
            case "CATEGORY":
            case "GROUP":
            case "MARK":
            case "CODE":
            case "UNIT":
            case "CREATOR":
            case "VALUE_FORMULA":
            case "NOTE_FORMAT":
            case "ROUND_UP_TOTAL":
            case "ASSIGNED_ELEMENT_IDS":
                return true;
            default:
                return false;
        }
    }
}
