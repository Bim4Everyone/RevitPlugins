using System;
using System.Collections.Generic;

using dosymep.WPF.ViewModels;

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

    public List<int> AssignedElementIds { get; set; } = new List<int>();

    public IDictionary<string, object> ExtensionData { get; set; }

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
            AssignedElementIds = config.AssignedElementIds != null
                ? new List<int>(config.AssignedElementIds)
                : new List<int>(),
            ExtensionData = config.ExtensionData != null
                ? new Dictionary<string, object>(config.ExtensionData)
                : null
        };

        return item;
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
            AssignedElementIds = AssignedElementIds != null
                ? new List<int>(AssignedElementIds)
                : new List<int>(),
            ExtensionData = ExtensionData != null
                ? new Dictionary<string, object>(ExtensionData)
                : null
        };

        return result;
    }
}
