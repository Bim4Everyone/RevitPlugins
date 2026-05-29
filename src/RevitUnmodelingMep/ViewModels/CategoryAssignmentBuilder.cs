using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitUnmodelingMep.Models;

namespace RevitUnmodelingMep.ViewModels;

internal sealed class CategoryAssignmentBuilder {
    private readonly RevitRepository _revitRepository;
    
    public CategoryAssignmentBuilder(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }

    /// <summary>
    /// Строит дерево назначений: категории Revit, типы систем и доступные для них конфигурации расходников.
    /// </summary>
    public ObservableCollection<CategoryAssignmentItem> Build(
        IReadOnlyList<CategoryOption> categoryOptions,
        IEnumerable<ConsumableTypeItem> consumableTypes,
        bool onlyPlacedInProject,
        Func<ConsumableTypeItem, int?> resolveCategoryId) {
        List<BuiltInCategory> categories = new List<BuiltInCategory> {
            BuiltInCategory.OST_PipeCurves,
            BuiltInCategory.OST_DuctCurves,
            BuiltInCategory.OST_PipeInsulations,
            BuiltInCategory.OST_DuctInsulations,
            BuiltInCategory.OST_DuctSystem,
            BuiltInCategory.OST_PipingSystem
        };

        var assignments = new List<CategoryAssignmentItem>();

        foreach(BuiltInCategory builtInCategory in categories) {
            CategoryOption option = categoryOptions.FirstOrDefault(o => o.BuiltInCategory == builtInCategory);
            int optionCategoryId = option?.Id ?? (int) builtInCategory;
            string categoryName = option?.Name ?? builtInCategory.ToString();

            List<Element> types = CollectionGenerator.GetElementTypesByCategory(_revitRepository.Doc, builtInCategory)
                                  ?? new List<Element>();
            if(types.Count == 0) {
                continue;
            }

            List<ConsumableTypeItem> configsForCategory = consumableTypes?
                .Where(c => resolveCategoryId(c) == optionCategoryId)
                .OrderBy(c => c?.ConsumableTypeName ?? string.Empty, StringComparer.CurrentCultureIgnoreCase)
                .ToList() ?? new List<ConsumableTypeItem>();

            HashSet<int> placedTypeIds = onlyPlacedInProject ? GetPlacedTypeIds(builtInCategory) : null;

            ObservableCollection<SystemTypeItem> systemTypes =
                new ObservableCollection<SystemTypeItem>(
                    types
                        .OfType<ElementType>()
                        .Where(type => placedTypeIds == null || placedTypeIds.Contains(unchecked((int) type.Id.GetIdValue())))
                        .OrderBy(type => type?.Name ?? string.Empty, StringComparer.CurrentCultureIgnoreCase)
                        .Select(type => CreateSystemTypeItem(type, configsForCategory)));

            if(systemTypes.Count == 0) {
                continue;
            }

            assignments.Add(new CategoryAssignmentItem {
                Name = categoryName,
                Category = builtInCategory,
                SystemTypes = systemTypes
            });
        }

        return new ObservableCollection<CategoryAssignmentItem>(
            assignments.OrderBy(a => a?.Name ?? string.Empty, StringComparer.CurrentCultureIgnoreCase));
    }

    /// <summary>
    /// Возвращает идентификаторы типов, которые реально размещены в проекте для указанной категории.
    /// </summary>
    private HashSet<int> GetPlacedTypeIds(BuiltInCategory builtInCategory) {
        var result = new HashSet<int>();

        var collector = new FilteredElementCollector(_revitRepository.Doc)
            .OfCategory(builtInCategory)
            .WhereElementIsNotElementType();

        foreach(Element element in collector) {
            ElementId typeId = element.GetTypeId();
            if(typeId == null || typeId == ElementId.InvalidElementId) {
                continue;
            }

            result.Add(unchecked((int) typeId.GetIdValue()));
        }

        return result;
    }

    /// <summary>
    /// Создает элемент типа системы с назначениями всех расходников, подходящих для его категории.
    /// </summary>
    private static SystemTypeItem CreateSystemTypeItem(ElementType elementType, List<ConsumableTypeItem> configs) {
        int typeId = unchecked((int) elementType.Id.GetIdValue());

        ObservableCollection<ConfigAssignmentItem> configAssignments =
            new ObservableCollection<ConfigAssignmentItem>(
                configs.Select(config => new ConfigAssignmentItem(config, typeId)));

        return new SystemTypeItem {
            Name = elementType.Name,
            Id = typeId,
            Configs = configAssignments
        };
    }
}
