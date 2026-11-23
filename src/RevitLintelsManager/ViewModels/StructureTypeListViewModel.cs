using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitLintelsManager.Models.Configs;
using RevitLintelsManager.Models.Settings;

namespace RevitLintelsManager.ViewModels;

internal class StructureTypeListViewModel : BaseViewModel {
    public event Action<StructureTypeViewModel> SelectionChanged;

    private readonly IReadOnlyList<StructureTypeViewModel> _rows;
    private string _searchText;

    /// <summary>Создаёт независимые строки выбора из общего каталога и сохранённых записей.</summary>
    public StructureTypeListViewModel(IEnumerable<StructureTypeSettings> savedTypes,
        IReadOnlyList<BuiltInCategory> categories,
        ILookup<(string Category, string FamilyName, string TypeName), StructureTypeSelection> catalog,
        ILocalizationService localization) {
        var saved = LintelManagerConfigResolver.ResolveStructureTypes(savedTypes, categories, catalog);
        var savedKeys = saved.Where(type => type.Settings is not null)
            .Select(type => type.Settings.GetKey()).ToHashSet();
        var candidates = catalog.Where(group => !savedKeys.Contains(group.Key))
            .Select(group => group.First().Settings);
        var available = LintelManagerConfigResolver.ResolveStructureTypes(candidates, categories, catalog)
            .Where(type => type.Resolution == StructureTypeResolution.Resolved);

        _rows = saved.Select(type => CreateRow(type, true, localization))
            .Concat(available.Select(type => CreateRow(type, false, localization))).ToArray();
        foreach(var row in _rows) {
            row.PropertyChanged += OnRowChanged;
        }
        FilteredTypes = new ListCollectionView(_rows.OrderByDescending(row => row.IsChecked)
            .ThenBy(row => row.Name, StringComparer.CurrentCultureIgnoreCase).ToArray());
    }

    public ICollectionView FilteredTypes { get; }

    public string SearchText {
        get => _searchText;
        set {
            if(_searchText == value) {
                return;
            }
            RaiseAndSetIfChanged(ref _searchText, value);
            string search = value?.Trim();
            FilteredTypes.Filter = string.IsNullOrEmpty(search)
                ? null
                : item => ((StructureTypeViewModel) item).Name.IndexOf(search,
                    StringComparison.CurrentCultureIgnoreCase) >= 0;
        }
    }

    /// <summary>Возвращает отмеченные записи независимо от фильтра и доступности.</summary>
    public IEnumerable<StructureTypeSelection> GetSelectedTypes() {
        return _rows.Where(row => row.IsChecked).Select(row => row.Selection);
    }

    // Формирует подпись по типу, семейству и категории для различения одноимённых строк.
    private static StructureTypeViewModel CreateRow(StructureTypeSelection type, bool isChecked,
        ILocalizationService localization) {
        string category = type.Settings?.Category switch {
            nameof(BuiltInCategory.OST_Walls) or nameof(BuiltInCategory.OST_Floors)
                or nameof(BuiltInCategory.OST_StructuralFraming) or nameof(BuiltInCategory.OST_StructuralColumns)
                or nameof(BuiltInCategory.OST_StructuralFoundation) =>
                localization.GetLocalizedString("General.Category." + type.Settings.Category),
            _ => type.Settings?.Category
        };
        string status = type.Resolution == StructureTypeResolution.Resolved
            ? null
            : localization.GetLocalizedString("General.StructureTypeUnavailable");
        string[] details = [type.Settings?.TypeName, type.Settings?.FamilyName, category, status];
        return new StructureTypeViewModel(type) {
            Name = string.Join(" · ", details.Where(detail => !string.IsNullOrWhiteSpace(detail))),
            IsChecked = isChecked
        };
    }

    // Передаёт изменение отметки подписчикам редактора.
    private void OnRowChanged(object sender, PropertyChangedEventArgs args) {
        if(args.PropertyName == nameof(StructureTypeViewModel.IsChecked)) {
            SelectionChanged?.Invoke((StructureTypeViewModel) sender);
        }
    }
}
