using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitBuildCoordVolumes.Models.Interfaces;

namespace RevitBuildCoordVolumes.Models.Services;

internal class ParamAvailabilityService : IParamAvailabilityService {

    private readonly Dictionary<string, ParamAvailabilityCache> _cacheByDocId = [];
    /// <summary>
    /// Проверяет, существует ли параметр в данном документе.
    /// </summary>
    public bool IsParamExist(Document doc, string paramName) {
        var cache = GetOrBuildCache(doc);
        return cache.ParamDefinitions.ContainsKey(paramName);
    }

    /// <summary>
    /// Возвращает Definition по имени параметра (если существует).
    /// </summary>
    public Definition GetDefinitionByName(Document doc, string paramName) {
        var cache = GetOrBuildCache(doc);
        return cache.ParamDefinitions.TryGetValue(paramName, out var def)
            ? def
            : null;
    }

    private ParamAvailabilityCache GetOrBuildCache(Document doc) {
        string docId = doc.GetUniqId();
        if(_cacheByDocId.TryGetValue(docId, out var cache)) {
            return cache;
        }

        cache = BuildCache(doc);
        _cacheByDocId[docId] = cache;
        return cache;
    }

    private ParamAvailabilityCache BuildCache(Document doc) {
        var cache = new ParamAvailabilityCache();
        var bindings = doc.ParameterBindings;
        var it = bindings.ForwardIterator();

        while(it.MoveNext()) {
            var def = it.Key;
            if(it.Current is not ElementBinding binding) {
                continue;
            }
            if(!cache.ParamDefinitions.ContainsKey(def.Name)) {
                cache.ParamDefinitions[def.Name] = def;
            }
        }
        return cache;
    }
}
