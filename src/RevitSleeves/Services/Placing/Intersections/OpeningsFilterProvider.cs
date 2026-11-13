using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration.Controls;

using dosymep.Revit;

using RevitClashDetective.Models.Interfaces;

using RevitSleeves.Models.Filtration;
using RevitSleeves.Services.Core;

namespace RevitSleeves.Services.Placing.Intersections;
/// <summary>
/// Провайдер отверстий из файлов с конструкциями для поиска пересечений с элементами ВИС из активного файла
/// </summary>
internal class OpeningsFilterProvider : IProvider {
    private readonly ILogicalFilterContext _structureFilterContext;
    private readonly string[] _familyNames;
    private readonly IOpeningGeometryProvider _geometryProvider;

    /// <summary>
    /// Конструирует провайдер отверстий
    /// </summary>
    /// <param name="doc">Документ, в котором расположены отверстия</param>
    /// <param name="structureFilterContext">Контекст фильтра конструкций, в которых должны находиться отверстия</param>
    /// <param name="transform">Трансформация документа с отверстиями относительно активного документа</param>
    /// <param name="familyNames">Названия семейств отверстий</param>
    /// <param name="geometryProvider">Обработчик геометрии чистовых отврстий</param>
    public OpeningsFilterProvider(Document doc,
        ILogicalFilterContext structureFilterContext,
        Transform transform,
        string[] familyNames,
        IOpeningGeometryProvider geometryProvider) {

        Doc = doc
            ?? throw new ArgumentNullException(nameof(doc));
        _structureFilterContext = structureFilterContext
            ?? throw new ArgumentNullException(nameof(structureFilterContext));
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
        var structureCategories = _structureFilterContext.SelectedCategories;

        var structures = new FilteredElementCollector(Doc)
            .WhereElementIsNotElementType()
            .WherePasses(new ElementMulticategoryFilter([.. structureCategories]))
            .WherePasses(_structureFilterContext.GetFilter().Build(Doc, FilterBuildOptions.Create()))
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
