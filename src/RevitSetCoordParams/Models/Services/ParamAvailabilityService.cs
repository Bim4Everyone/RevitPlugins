using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

using RevitSetCoordParams.Models.Interfaces;

namespace RevitSetCoordParams.Models.Services;

public class ParamAvailabilityService : IParamAvailabilityService {
    private readonly Dictionary<string, HashSet<ElementId>> _paramToCategories = [];
    private readonly Dictionary<string, Definition> _paramDefinitions = [];

    public ParamAvailabilityService(Document doc) {
        BuildCache(doc);
    }

    public bool IsParamAvailable(RevitParam param, Category category) {
        return _paramToCategories.TryGetValue(param.Name, out var cats)
            && cats.Contains(category.Id);
    }

    public bool IsParamExist(string paramName) {
        return _paramToCategories.ContainsKey(paramName);
    }

    public Definition GetDefinitionByName(string paramName) {
        return _paramDefinitions.TryGetValue(paramName, out var def)
            ? def
            : null;
    }

    private void BuildCache(Document doc) {
        var bindingMap = doc.ParameterBindings;
        var it = bindingMap.ForwardIterator();

        while(it.MoveNext()) {
            var def = it.Key;

            if(it.Current is not ElementBinding binding) {
                continue;
            }

            if(!_paramDefinitions.ContainsKey(def.Name)) {
                _paramDefinitions[def.Name] = def;
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
