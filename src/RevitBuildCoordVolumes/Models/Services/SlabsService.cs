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
    private readonly SystemPluginConfig _systemPluginConfig;

    public SlabsService(IDocumentsService documentsService, SystemPluginConfig systemPluginConfig) {
        _documentsService = documentsService;
        _systemPluginConfig = systemPluginConfig;
    }

    public IEnumerable<SlabElement> GetSlabsByTypesAndDocs(IEnumerable<string> typeSlabs, IEnumerable<Document> documents) {
        if(typeSlabs == null || !typeSlabs.Any()) {
            return [];
        }
        var foundSlabs = GetSlabsByDocs(documents)
            .Where(slab => typeSlabs.Contains(slab.FloorName));
        return foundSlabs;
    }

    public IEnumerable<SlabElement> GetSlabsByTypesDocsAndLevels(
        IEnumerable<string> typeSlabs,
        IEnumerable<Document> documents,
        Level upLevel,
        Level bottomLevel) {

        double minElevation = Math.Min(bottomLevel.Elevation, upLevel.Elevation);
        double maxElevation = Math.Max(bottomLevel.Elevation, upLevel.Elevation);

        var slabs = GetSlabsByTypesAndDocs(typeSlabs, documents);

        return slabs.Where(slab => {
            double elev = slab.Level.Elevation;
            return elev >= minElevation && elev <= maxElevation;
        });
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
        var categoryFilters = _systemPluginConfig.SlabCategories
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
                var level = GetLevel(doc, floor);
                return new SlabElement {
                    Floor = floor,
                    Level = level,
                    FloorName = floor.Name,
                    LevelName = GetLevelName(level),
                    Guid = Guid.NewGuid(),
                    Transform = tansform
                };
            });
    }

    // Метод получения имени уровня, на котором расположена плита
    private Level GetLevel(Document doc, Floor floor) {
        var elementId = floor.GetParamValueOrDefault<ElementId>(BuiltInParameter.LEVEL_PARAM);
        return doc.GetElement(elementId) as Level;
    }

    // Метод получения имени уровня, на котором расположена плита
    private string GetLevelName(Level level) {
        string levelName = level.Name;
        string modifyLevelName = levelName.Split(['_']).FirstOrDefault();
        return modifyLevelName ?? string.Empty;
    }
}
