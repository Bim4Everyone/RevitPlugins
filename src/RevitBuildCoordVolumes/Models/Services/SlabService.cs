using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitBuildCoordVolumes.Models.Interfaces;
using RevitBuildCoordVolumes.Models.Utilites;

namespace RevitBuildCoordVolumes.Models.Services;

internal class SlabService : ISlabService {
    private readonly Dictionary<string, IEnumerable<SlabElement>> _slabsByDocName = [];
    private readonly IDocumentService _documentsService;
    private readonly SystemPluginConfig _systemPluginConfig;

    public SlabService(IDocumentService documentsService, SystemPluginConfig systemPluginConfig) {
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
                    FloorName = floor.Name,
                    Level = level,
                    LevelName = GetLevelName(level),
                    Profile = GetProfile(doc, floor),
                    Guid = Guid.NewGuid(),
                    Transform = tansform,
                    IsSloped = IsSloped(doc, floor)
                };
            });
    }

    // Метод получения уровня, на котором расположена плита
    private Level GetLevel(Document doc, Floor floor) {
        var elementId = floor.GetParamValueOrDefault<ElementId>(BuiltInParameter.LEVEL_PARAM);
        return doc.GetElement(elementId) as Level;
    }

    // Метод получения имени уровня
    private string GetLevelName(Level level) {
        string levelName = level.Name;
        string modifyLevelName = levelName.Split(['_']).FirstOrDefault();
        return modifyLevelName ?? string.Empty;
    }

    // Метод получения профиля плиты
    private CurveArrArray GetProfile(Document doc, Floor floor) {
        var profileId = floor.SketchId;
        var sketch = doc.GetElement(profileId) as Sketch;
        return sketch.Profile;
    }

    // Метод определения наклонная ли плита
    private bool IsSloped(Document doc, Floor floor) {
        return IsShapeEdited(doc, floor) || HasSlopeBySlopeLine(doc, floor);
    }

    // Метод проверки редактирована ли плита
    private bool IsShapeEdited(Document doc, Floor floor) {
#if REVIT_2023_OR_LESS
        var slabShapeEditor = floor.SlabShapeEditor;
#else
        var slabShapeEditor = floor.GetSlabShapeEditor();
#endif
        if(slabShapeEditor == null) {
            return false;
        }
        var vertices = slabShapeEditor.SlabShapeVertices
        .Cast<SlabShapeVertex>()
        .ToList();

        if(vertices.Count == 0) {
            return false;
        }

        double firstZ = vertices[0].Position.Z;

        return vertices.Any(v =>
            Math.Abs(v.Position.Z - firstZ) > GeometryTolerance.Model);
    }

    // Метод проверки наклонена ли плита линией уклона
    private bool HasSlopeBySlopeLine(Document doc, Floor floor) {
        var filter = new ElementCategoryFilter(BuiltInCategory.OST_SketchLines);
        var depIds = floor.GetDependentElements(filter);

        var lines = depIds
            .Select(doc.GetElement)
            .Where(element => element.Name.Equals(_systemPluginConfig.SlopeLineName));

        if(!lines.Any()) {
            return false;
        }
        var slopeLine = lines.First();
        double start = slopeLine.GetParamValueOrDefault<double>(BuiltInParameter.SLOPE_START_HEIGHT);
        double end = slopeLine.GetParamValueOrDefault<double>(BuiltInParameter.SLOPE_END_HEIGHT);

        return Math.Abs(start - end) < GeometryTolerance.Model;
    }
}
