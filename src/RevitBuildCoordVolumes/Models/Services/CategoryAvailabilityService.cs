using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitBuildCoordVolumes.Models.Interfaces;

namespace RevitBuildCoordVolumes.Models.Services;

internal class CategoryAvailabilityService : ICategoryAvailabilityService {
    private readonly Dictionary<string, HashSet<ElementId>> _paramToCategories = [];

    public CategoryAvailabilityService(Document doc) {
        BuildCache(doc);
    }

    public bool IsParamAvailableInCategory(string paramName, Category category) {
        return _paramToCategories.TryGetValue(paramName, out var cats)
            && cats.Contains(category.Id);
    }

    // Метод построение кэша документов
    private void BuildCache(Document doc) {
        var bindingMap = doc.ParameterBindings;
        var it = bindingMap.ForwardIterator();

        while(it.MoveNext()) {
            var def = it.Key;

            if(it.Current is not ElementBinding binding) {
                continue;
            }

            if(!_paramToCategories.TryGetValue(def.Name, out var cats)) {
                cats = [];
                _paramToCategories[def.Name] = cats;
            }

            foreach(Category cat in binding.Categories) {
                cats.Add(cat.Id);
            }
        }
    }
}

