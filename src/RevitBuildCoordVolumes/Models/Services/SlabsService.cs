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
            .OfType<Floor>()
            .Where(floor => !string.IsNullOrWhiteSpace(floor.Name))
            .Select(floor => new SlabElement { Floor = floor, Name = floor.Name, ContourPoints = GetContourSlab(doc, floor), Document = doc });
    }

    private IList<XYZ> GetContourSlab(Document doc, Floor floor) {
        var sketchId = floor.SketchId;
        var sketch = doc.GetElement(sketchId) as Sketch;
        var profile = sketch.Profile;
        var externalContour = profile.get_Item(0);

        var listXyz = new List<XYZ>();
        foreach(Curve curve in externalContour) {
            listXyz.Add(curve.GetEndPoint(0));
        }
        return listXyz;
    }


}
