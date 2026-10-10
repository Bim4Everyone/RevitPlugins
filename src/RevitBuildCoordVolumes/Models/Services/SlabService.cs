using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitBuildCoordVolumes.Models.Interfaces;

namespace RevitBuildCoordVolumes.Models.Services;

internal class SlabService(
    IDocumentService documentsService,
    ISlabGeometryService slabGeometryService,
    SystemPluginConfig systemPluginConfig)
    : ISlabService {
    //private readonly Dictionary<string, IReadOnlyList<SlabElement>> _slabsByDocName = [];
    private readonly Dictionary<string, IEnumerable<SlabElement>> _slabsByDocName = [];
    
    public IEnumerable<SlabElement> GetSlabsByTypesAndDocs(IEnumerable<string> typeSlabs, IEnumerable<Document> documents) {
        if(typeSlabs == null || !typeSlabs.Any()) {
            return [];
        }
        var foundSlabs = GetSlabsByDocs(documents)
            .Where(slab => typeSlabs.Contains(slab.FloorName));
        return foundSlabs;
    }

    public IEnumerable<SlabElement> GetSlabsByTypesDocsAndLevels(
        IEnumerable<string> typeSlabs, IEnumerable<Document> documents, List<Level> levels) {

        var slabs = GetSlabsByTypesAndDocs(typeSlabs, documents);
        var levelIds = levels.Select(level => level.Id);
        return slabs
            .Where(slab => levelIds.Contains(slab.Level.Id));
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

    // public IEnumerable<SlabElement> GetSlabsByTypesAndDocs(IEnumerable<string> typeSlabs, IEnumerable<Document> documents) {
    //     var typeSlabsList = typeSlabs.ToList();
    //     var documentsList = documents.ToList();
    //     if(typeSlabsList.Count == 0 || documentsList.Count == 0) {
    //         return [];
    //     }
    //     var foundSlabs = GetSlabsByDocs(documentsList)
    //         .Where(slab => typeSlabsList.Contains(slab.Floor.Name));
    //     return foundSlabs;
    // }
    //
    // public IEnumerable<SlabElement> GetSlabsByTypesDocsAndLevels(
    //     IEnumerable<string> typeSlabs, IEnumerable<Document> documents, List<Level> levels) {
    //
    //     var slabs = GetSlabsByTypesAndDocs(typeSlabs, documents);
    //     var levelIds = levels.Select(level => level.Id);
    //     return slabs
    //         .Where(slab => levelIds.Contains(slab.Level.Id));
    // }
    //
    // public IEnumerable<SlabElement> GetSlabsByDocs(IEnumerable<Document> documents) {
    //     var result = new List<SlabElement>();
    //
    //     foreach(var doc in documents) {
    //         string docKey = doc.GetUniqId();
    //
    //         if(!_slabsByDocName.TryGetValue(docKey, out var cachedSlabs)) {
    //             cachedSlabs = LoadSlabsFromDocument(doc).ToList();
    //             _slabsByDocName[docKey] = cachedSlabs;
    //         }
    //         result.AddRange(cachedSlabs);
    //     }
    //     return result;
    // }
    
    private IList<SlabElement> LoadSlabsFromDocument(Document doc) {
        var categories = systemPluginConfig.SlabCategories;

        if (categories == null || categories.Count == 0)
            return new List<SlabElement>();

        var filters = categories
            .Distinct()
            .Select(ElementFilter (cat) => new ElementCategoryFilter(cat))
            .ToList();

        var categoryFilter = new LogicalOrFilter(filters);

        var floors = new FilteredElementCollector(doc)
            .WhereElementIsNotElementType()
            .WherePasses(categoryFilter)
            .OfClass(typeof(Floor))
            .Cast<Floor>();

        var result = new List<SlabElement>();

        foreach (var floor in floors) {
            if (string.IsNullOrWhiteSpace(floor.Name)) {
                continue;
            }
            result.Add(CreateSlabElement(doc, floor));
        }

        return result;
    }

    // Метод получения перекрытий и плит из одного документа   
    // private IEnumerable<SlabElement> LoadSlabsFromDocument(Document doc) {
    //     var categoryFilters = systemPluginConfig.SlabCategories
    //         .Select(cat => (ElementFilter) new ElementCategoryFilter(cat))
    //         .ToList();
    //
    //     var multiFilter = new LogicalOrFilter(categoryFilters);
    //
    //     return new FilteredElementCollector(doc)
    //         .WherePasses(multiFilter)
    //         .WhereElementIsNotElementType()
    //         .OfType<Floor>()
    //         .Where(floor => !string.IsNullOrWhiteSpace(floor.Name))
    //         .Select(floor => CreateSlabElement(doc, floor));
    // }
    
    // Метод создания SlabElement
    private SlabElement CreateSlabElement(Document doc, Floor floor) {
        var transformFromDoc = documentsService.GetTransformByName(doc.GetUniqId());
        var geometryData = slabGeometryService.GetSlabGeometryData(floor, transformFromDoc);
        var level = GetSlabLevel(floor);
        string levelName = GetLevelName(level);
        return new SlabElement {
            Guid = Guid.NewGuid(),
            Floor = floor,
            FloorName = floor.Name,
            Level = level,
            LevelName = levelName,
            TopContour = geometryData.Contour,
            TopFaces = geometryData.TopFaces,
            SlabType = geometryData.SlabType
        };
    }

    // Метод получения уровня, на котором расположена плита
    private static Level GetSlabLevel(Floor floor) {
        var doc = floor.Document;
        var elementId = floor.GetParamValueOrDefault<ElementId>(BuiltInParameter.LEVEL_PARAM);
        return doc.GetElement(elementId) as Level;
    }

    // Метод получения имени уровня
    private static string GetLevelName(Level level) {
        string levelName = level.Name;
        string modifyLevelName = levelName.Split('_').FirstOrDefault();
        return modifyLevelName ?? string.Empty;
    }
}
