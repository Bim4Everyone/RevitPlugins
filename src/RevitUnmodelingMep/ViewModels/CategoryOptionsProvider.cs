using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

namespace RevitUnmodelingMep.ViewModels;

internal sealed class CategoryOptionsProvider {
    private readonly Document _document;
    private readonly ILocalizationService _localizationService;

    /// <summary>
    /// Создает поставщик доступных MEP-категорий для настроек расходников.
    /// </summary>
    public CategoryOptionsProvider(Document document, ILocalizationService localizationService) {
        _document = document;
        _localizationService = localizationService;
    }

    /// <summary>
    /// Создает справочник категорий, доступных для выбора в настройках расходников.
    /// </summary>
    public IReadOnlyList<CategoryOption> CreateCategoryOptions() {
        return new List<CategoryOption> {
            CreateCategoryOption(
                _localizationService.GetLocalizedString("MainViewModel.DuctsName"),
                BuiltInCategory.OST_DuctCurves),
            CreateCategoryOption(
                _localizationService.GetLocalizedString("MainViewModel.PipesName"),
                BuiltInCategory.OST_PipeCurves),
            CreateCategoryOption(
                _localizationService.GetLocalizedString("MainViewModel.PipeInsName"),
                BuiltInCategory.OST_PipeInsulations),
            CreateCategoryOption(
                _localizationService.GetLocalizedString("MainViewModel.DuctInsName"),
                BuiltInCategory.OST_DuctInsulations),
            CreateCategoryOption(
                _localizationService.GetLocalizedString("MainViewModel.DuctSysName"),
                BuiltInCategory.OST_DuctSystem),
            CreateCategoryOption(
                _localizationService.GetLocalizedString("MainViewModel.PipeSysName"),
                BuiltInCategory.OST_PipingSystem)
        };
    }

    /// <summary>
    /// Преобразует сохраненное значение категории в один из доступных вариантов выбора.
    /// </summary>
    public CategoryOption ResolveCategoryOption(
        IReadOnlyList<CategoryOption> categoryOptions,
        string categoryValue) {
        if(string.IsNullOrWhiteSpace(categoryValue)) {
            return categoryOptions.FirstOrDefault();
        }

        if(int.TryParse(categoryValue, out int id)) {
            return categoryOptions.FirstOrDefault(o => o.Id == id);
        }

        string trimmed = categoryValue.Trim();
        if(trimmed.StartsWith("BuiltInCategory.", StringComparison.OrdinalIgnoreCase)) {
            string enumName = trimmed.Substring("BuiltInCategory.".Length);
            CategoryOption byEnumName = categoryOptions.FirstOrDefault(o =>
                string.Equals(o.BuiltInCategory.ToString(), enumName, StringComparison.OrdinalIgnoreCase));
            if(byEnumName != null) {
                return byEnumName;
            }
        }

        return categoryOptions.FirstOrDefault(o =>
            string.Equals(o.Name, categoryValue, StringComparison.OrdinalIgnoreCase))
               ?? categoryOptions.FirstOrDefault();
    }

    /// <summary>
    /// Создает один вариант категории с локализованным именем, BuiltInCategory и фактическим id категории в документе.
    /// </summary>
    private CategoryOption CreateCategoryOption(string name, BuiltInCategory builtInCategory) {
        Category category = Category.GetCategory(_document, builtInCategory);

        long idValue;
#if REVIT_2024_OR_GREATER
        idValue = category?.Id.Value ?? (long) (int) builtInCategory;
#else
        idValue = category?.Id.IntegerValue ?? (int) builtInCategory;
#endif
        int id = unchecked((int) idValue);

        return new CategoryOption {
            Name = name,
            BuiltInCategory = builtInCategory,
            Id = id
        };
    }
}
