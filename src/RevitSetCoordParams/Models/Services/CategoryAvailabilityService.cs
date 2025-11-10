using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

using RevitSetCoordParams.Models.Interfaces;

namespace RevitSetCoordParams.Models.Services;

internal class CategoryAvailabilityService : ICategoryAvailabilityService {
    private readonly Dictionary<string, HashSet<ElementId>> _paramToCategories = [];

    public CategoryAvailabilityService(Document doc) {
        BuildCache(doc);
    }

    public bool IsParamAvailableInCategory(RevitParam param, Category category) {
        return _paramToCategories.TryGetValue(param.Name, out var cats)
            && cats.Contains(category.Id);
    }

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
