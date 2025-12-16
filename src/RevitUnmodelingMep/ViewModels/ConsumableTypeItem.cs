using dosymep.WPF.ViewModels;

using Newtonsoft.Json.Linq;

namespace RevitUnmodelingMep.ViewModels;

internal class ConsumableTypeItem : BaseViewModel {
    private string _title;
    private string _selectedType;
    private string _name;
    private string _grouping;
    private string _naming;
    private string _brand;
    private string _code;
    private string _unit;
    private string _factory;
    private string _numberFormula;
    private string _noteFormat;
    private string _enamel;
    private string _primer;
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

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public string CategoryId { get; set; }

    public string Grouping {
        get => _grouping;
        set => RaiseAndSetIfChanged(ref _grouping, value);
    }

    public string Naming {
        get => _naming;
        set => RaiseAndSetIfChanged(ref _naming, value);
    }

    public string Brand {
        get => _brand;
        set => RaiseAndSetIfChanged(ref _brand, value);
    }

    public string Code {
        get => _code;
        set => RaiseAndSetIfChanged(ref _code, value);
    }

    public string Unit {
        get => _unit;
        set => RaiseAndSetIfChanged(ref _unit, value);
    }

    public string Factory {
        get => _factory;
        set => RaiseAndSetIfChanged(ref _factory, value);
    }

    public string NumberFormula {
        get => _numberFormula;
        set => RaiseAndSetIfChanged(ref _numberFormula, value);
    }

    public string NoteFormat {
        get => _noteFormat;
        set => RaiseAndSetIfChanged(ref _noteFormat, value);
    }

    public string Enamel {
        get => _enamel;
        set => RaiseAndSetIfChanged(ref _enamel, value);
    }

    public string Primer {
        get => _primer;
        set => RaiseAndSetIfChanged(ref _primer, value);
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

    public static ConsumableTypeItem FromConfig(JProperty configProperty) {
        JObject value = configProperty.Value as JObject ?? new JObject();
        JObject clonedValue = (JObject) value.DeepClone();
        JArray assignedIds = value["ASSIGNED_ELEMENT_IDS"] as JArray ?? new JArray();

        return new ConsumableTypeItem {
            ConfigKey = configProperty.Name,
            Title = (string) value["CONFIG_NAME"],
            Name = (string) value["CONFIG_NAME"],
            Naming = (string) value["NAME"],
            CategoryId = (string) value["CATEGORY"],
            Grouping = (string) value["GROUP"],
            Brand = (string) value["MARK"],
            Code = (string) value["CODE"],
            Unit = (string) value["UNIT"],
            Factory = (string) value["CREATOR"],
            NumberFormula = (string) value["VALUE_FORMULA"],
            NoteFormat = (string) value["NOTE_FORMAT"],
            AssignedElementIds = new JArray(assignedIds),
            RawConfig = clonedValue
        };
    }

    public JObject ToJObject() {
        JObject result = RawConfig != null
            ? (JObject) RawConfig.DeepClone()
            : new JObject();

        result["CONFIG_NAME"] = Name ?? string.Empty;
        result["NAME"] = Naming ?? string.Empty;
        result["CATEGORY"] = CategoryId ?? string.Empty;
        result["GROUP"] = Grouping ?? string.Empty;
        result["MARK"] = Brand ?? string.Empty;
        result["CODE"] = Code ?? string.Empty;
        result["UNIT"] = Unit ?? string.Empty;
        result["CREATOR"] = Factory ?? string.Empty;
        result["VALUE_FORMULA"] = NumberFormula ?? string.Empty;
        result["NOTE_FORMAT"] = NoteFormat ?? string.Empty;
        result["ASSIGNED_ELEMENT_IDS"] = AssignedElementIds ?? new JArray();

        return result;
    }
}
