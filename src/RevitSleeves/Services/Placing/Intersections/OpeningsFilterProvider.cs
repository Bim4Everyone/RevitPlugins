using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models.FilterGenerators;
using RevitClashDetective.Models.FilterModel;
using RevitClashDetective.Models.Interfaces;

using RevitSleeves.Services.Core;

namespace RevitSleeves.Services.Placing.Intersections;
/// <summary>
/// Провайдер отверстий из файлов с конструкциями для поиска пересечений с элементами ВИС из активного файла
/// </summary>
internal class OpeningsFilterProvider : IProvider {
    private readonly Filter _structureFilter;
    private readonly string[] _familyNames;
    private readonly IOpeningGeometryProvider _geometryProvider;

    /// <summary>
    /// Конструирует провайдер отверстий
    /// </summary>
    /// <param name="doc">Документ, в котором расположены отверстия</param>
    /// <param name="structureFilter">Фильтр конструкций, в которых должны находиться отверстия</param>
    /// <param name="transform">Трансформация документа с отверстиями относительно активного документа</param>
    /// <param name="familyNames">Названия семейств отверстий</param>
    /// <param name="geometryProvider">Обработчик геометрии чистовых отврстий</param>
    public OpeningsFilterProvider(Document doc,
        Filter structureFilter,
        Transform transform,
        string[] familyNames,
        IOpeningGeometryProvider geometryProvider) {

        Doc = doc
            ?? throw new ArgumentNullException(nameof(doc));
        _structureFilter = structureFilter
            ?? throw new ArgumentNullException(nameof(structureFilter));
        MainTransform = transform
            ?? throw new ArgumentNullException(nameof(transform));
        _familyNames = familyNames
            ?? throw new ArgumentNullException(nameof(familyNames));
        _geometryProvider = geometryProvider
            ?? throw new ArgumentNullException(nameof(geometryProvider));
    }


    public Document Doc { get; }

    public Transform MainTransform { get; }


    public List<Element> GetElements() {
        var structureCategories = _structureFilter.CategoryIds.Select(item => item.AsBuiltInCategory()).ToList();

        var structures = new FilteredElementCollector(Doc)
            .WhereElementIsNotElementType()
            .WherePasses(new ElementMulticategoryFilter(structureCategories))
            .WherePasses(_structureFilter.GetRevitFilter(Doc, new StraightRevitFilterGenerator()))
            .ToElementIds()
            .ToHashSet();
        List<Element> openings = [];
        foreach(string famName in _familyNames) {
            var openingIds = GetAllFamilyInstances(Doc, famName);
            if(openingIds.Count == 0) {
                continue;
            }
            openings.AddRange(openingIds
                .Select(id => (FamilyInstance) Doc.GetElement(id))
                .Where(f => structures.Contains(f.Host?.Id)));
        }
        return openings;
    }

    public List<Solid> GetSolids(Element element) {
        return [_geometryProvider.GetSolid((FamilyInstance) element)];
    }

    private ElementId GetFamily(Document doc, string familyName) {
        return new FilteredElementCollector(doc)
            .OfClass(typeof(Family))
            .FirstOrDefault(family => family.Name.Equals(familyName, StringComparison.InvariantCultureIgnoreCase))
            ?.Id ?? ElementId.InvalidElementId;
    }

    private ICollection<ElementId> GetAllFamilyInstances(Document doc, string familyName) {
        var familyId = GetFamily(doc, familyName);
        if(familyId.IsNotNull()) {
            var family = (Family) Doc.GetElement(familyId);
            var symbolIds = family.GetFamilySymbolIds();
            List<ElementId> instances = [];
            foreach(var symbolId in symbolIds) {
                instances.AddRange(new FilteredElementCollector(Doc)
                    .WhereElementIsNotElementType()
                    .WherePasses(new FamilyInstanceFilter(Doc, symbolId))
                    .ToElementIds());
            }
            return [.. instances.Distinct()];
        } else {
            return Array.Empty<ElementId>();
        }
    }
}
