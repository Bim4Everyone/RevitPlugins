using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitBuildCoordVolumes.Models.Interfaces;

namespace RevitBuildCoordVolumes.Models.Services;
internal class SlabsService : ISlabsService {
    private readonly IDocumentsService _documentsService;
    private readonly Dictionary<string, IEnumerable<SlabElement>> _slabsByDocName = [];

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
                var solid = GetSolid(doc, floor);
                return new SlabElement {
                    Name = floor.Name,
                    LevelName = GetLevelName(doc, floor),
                    Floor = floor,
                    FloorSolid = solid,
                    ExternalContourPoints = GetContourSlab(doc, floor),
                    FullExternalContourPoints = GetContourSlabFull(doc, floor)
                };
            });
    }

    private List<XYZ> GetContourSlab(Document doc, Floor floor) {
        var sketchId = floor.SketchId;
        var sketch = doc.GetElement(sketchId) as Sketch;
        var profile = sketch.Profile;
        var externalContour = profile.get_Item(0);

        var transform = doc.IsLinked ? _documentsService.GetTransformByName(doc.GetUniqId()) : null;

        var listXyz = new List<XYZ>();
        foreach(Curve curve in externalContour) {
            var point = transform.OfPoint(curve.GetEndPoint(0));
            listXyz.Add(point);
        }
        return listXyz;
    }

    private List<List<XYZ>> GetContourSlabFull(Document doc, Floor floor) {
        var transform = doc.IsLinked ? _documentsService.GetTransformByName(doc.GetUniqId()) : null;

        var sketchId = floor.SketchId;
        var sketch = doc.GetElement(sketchId) as Sketch;
        var curveArrArray = sketch.Profile;

        var listXyzAll = new List<List<XYZ>>();
        foreach(CurveArray curveArray in curveArrArray) {
            var listXyz = new List<XYZ>();
            foreach(Curve curve in curveArray) {
                var point = transform.OfPoint(curve.GetEndPoint(0));
                listXyz.Add(point);
            }
            listXyzAll.Add(listXyz);
        }
        return listXyzAll;
    }

    private Solid GetSolid(Document doc, Floor floor) {
        var transform = doc.IsLinked ? _documentsService.GetTransformByName(doc.GetUniqId()) : null;
        var solidFloor = floor.GetSolids().First();
        return transform == null
            ? solidFloor
            : SolidUtils.CreateTransformed(solidFloor, transform);
    }

    private string GetLevelName(Document doc, Floor floor) {
        var elementId = floor.GetParamValueOrDefault<ElementId>(BuiltInParameter.LEVEL_PARAM);
        var level = doc.GetElement(elementId) as Level;
        string levelName = level.Name;
        string modifyLevelName = levelName.Split(['_']).First();
        return modifyLevelName;
    }

    public IEnumerable<SlabElement> GetSlabsByName(string name, IEnumerable<Document> documents) {
        throw new System.NotImplementedException();
    }

    public List<Face> GetFloorSurfacesWithoutHoles(Floor floor) {
        // Получаем верхнюю грань
        var topRef = HostObjectUtils.GetTopFaces(floor).First();
        var topFace = floor.GetGeometryObjectFromReference(topRef) as Face;

        // Берём все циклы (и внешние, и внутренние)
        var loops = topFace.GetEdgesAsCurveLoops();

        var solid = GeometryCreationUtilities.CreateExtrusionGeometry(loops, XYZ.BasisZ, 0.1);

        var faces = solid.Faces;

        var upFaces = new List<Face>();
        foreach(PlanarFace face in faces) {
            if(face.FaceNormal.Z > 0) {
                upFaces.Add(face);
            }
        }
        return upFaces;
    }
}
