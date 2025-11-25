using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitBuildCoordVolumes.Models.Interfaces;

namespace RevitBuildCoordVolumes.Models.Services;
internal class SlabsService : ISlabsService {
    private readonly Dictionary<string, IEnumerable<SlabElement>> _slabsByDocName = [];

    public IEnumerable<SlabElement> GetSlabsByName(string name) {
        if(string.IsNullOrWhiteSpace(name)) {
            return null;
        }
        var foundSlabs = _slabsByDocName
            .SelectMany(dic => dic.Value)
            .Where(slab => slab.Name.Equals(name));

        return foundSlabs ?? null;
    }

    public IEnumerable<SlabElement> GetSlabsByDocs(IEnumerable<Document> documents) {
        var result = new List<SlabElement>();

        foreach(var doc in documents) {
            string docKey = doc.GetUniqId();

            if(!_slabsByDocName.TryGetValue(docKey, out var cachedSlabs)) {
                cachedSlabs = LoadSlabsFromDocument(doc);
                _slabsByDocName[docKey] = cachedSlabs;
            }
            result.AddRange(cachedSlabs);
        }
        return result;
    }

    // Метод получения перекрытий и плит из одного документа   
    private IEnumerable<SlabElement> LoadSlabsFromDocument(Document doc) {
        var categoryFilters = RevitConstants.SlabCategories
            .Select(cat => (ElementFilter) new ElementCategoryFilter(cat))
            .ToList();

        var multiFilter = new LogicalOrFilter(categoryFilters);

        return new FilteredElementCollector(doc)
            .WherePasses(multiFilter)
            .WhereElementIsNotElementType()
            .OfType<Element>()
            .Where(element => !string.IsNullOrWhiteSpace(element.Name))
            .Select(element => new SlabElement { Element = element, Name = element.Name });
    }

    public SlabElement GetSlabByName(string name) {
        throw new System.NotImplementedException();
    }

    IEnumerable<SlabElement> ISlabsService.GetSlabsByDocs(IEnumerable<Document> documents) {
        return GetSlabsByDocs(documents);
    }
}
