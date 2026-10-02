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
    private readonly Dictionary<string, IReadOnlyList<SlabElement>> _slabsByDocName = [];

    public IEnumerable<SlabElement> GetSlabsByTypesAndDocs(IEnumerable<string> typeSlabs, IEnumerable<Document> documents) {
        var enumerable = typeSlabs.ToList();
        if(typeSlabs == null || !enumerable.Any()) {
            return [];
        }
        var foundSlabs = GetSlabsByDocs(documents)
            .Where(slab => enumerable.Contains(slab.Floor.Name));
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
                cachedSlabs = LoadSlabsFromDocument(doc).ToList();
                _slabsByDocName[docKey] = cachedSlabs;
            }
            result.AddRange(cachedSlabs);
        }
        return result;
    }

    // Метод получения перекрытий и плит из одного документа   
    private IEnumerable<SlabElement> LoadSlabsFromDocument(Document doc) {
        var categoryFilters = systemPluginConfig.SlabCategories
            .Select(cat => (ElementFilter) new ElementCategoryFilter(cat))
            .ToList();

        var multiFilter = new LogicalOrFilter(categoryFilters);

        return new FilteredElementCollector(doc)
            .WherePasses(multiFilter)
            .WhereElementIsNotElementType()
            .OfType<Floor>()
            .Where(floor => !string.IsNullOrWhiteSpace(floor.Name))
            .Select(floor => CreateSlabElement(doc, floor));
    }

    private SlabElement CreateSlabElement(Document doc, Floor floor) {
        var transformFromDoc = documentsService.GetTransformByName(doc.GetUniqId());
        var geometryData = slabGeometryService.GetSlabGeometryData(floor, transformFromDoc);
        var level = GetSlabLevel(floor);
        string levelName = GetLevelName(level);
        
        return new SlabElement {
            Guid = Guid.NewGuid(),
            Floor = floor,
            Level = level,
            LevelName = levelName,
            TopContour = geometryData.Contour,
            TopFaces = geometryData.TopFaces,
            IsSloped = geometryData.IsSloped,
            
            Profile = GetProfile(floor),
            Transform = transformFromDoc
        };
    }

    // Метод получения уровня, на котором расположена плита
    private Level GetSlabLevel(Floor floor) {
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
    
    // Метод получения профиля плиты
    private static CurveArrArray GetProfile(Floor floor) {
        var doc = floor.Document;
        var profileId = floor.SketchId;
        var sketch = doc.GetElement(profileId) as Sketch;
        return sketch?.Profile;
    }
}
