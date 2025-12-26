using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitBuildCoordVolumes.Models.Interfaces;

namespace RevitBuildCoordVolumes.Models.Services;
internal class SlabsService : ISlabsService {
    private readonly Dictionary<string, IEnumerable<SlabElement>> _slabsByDocName = [];
    private readonly IDocumentsService _documentsService;

    public SlabsService(IDocumentsService documentsService) {
        _documentsService = documentsService;
    }

    public IEnumerable<SlabElement> GetSlabsByTypesAndDocs(IEnumerable<string> typeSlabs, IEnumerable<Document> documents) {
        if(typeSlabs == null || !typeSlabs.Any()) {
            return [];
        }
        var foundSlabs = GetSlabsByDocs(documents)
            .Where(slab => typeSlabs.Contains(slab.Name));
        return foundSlabs;
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
            .OfType<Floor>()
            .Where(floor => !string.IsNullOrWhiteSpace(floor.Name))
            .Select(floor => {
                var tansform = _documentsService.GetTransformByName(doc.GetUniqId());
                return new SlabElement {
                    Floor = floor,
                    Name = floor.Name,
                    LevelName = GetLevelName(doc, floor),
                    Guid = Guid.NewGuid(),
                    Transform = tansform
                };
            });
    }

    // Метод получения имени уровня, на котором расположена плита
    private string GetLevelName(Document doc, Floor floor) {
        var elementId = floor.GetParamValueOrDefault<ElementId>(BuiltInParameter.LEVEL_PARAM);
        var level = doc.GetElement(elementId) as Level;
        string levelName = level.Name;
        string modifyLevelName = levelName.Split(['_']).FirstOrDefault();
        return modifyLevelName ?? string.Empty;
    }
}
