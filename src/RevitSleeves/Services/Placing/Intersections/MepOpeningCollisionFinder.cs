using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.ClashDetection;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.FilterModel;

using RevitSleeves.Models;
using RevitSleeves.Services.Core;

namespace RevitSleeves.Services.Placing.Intersections;
internal abstract class MepOpeningCollisionFinder : MepStructureCollisionFinder {
    protected readonly IOpeningGeometryProvider _openingGeometryProvider;


    protected MepOpeningCollisionFinder(
        RevitRepository revitRepository,
        IMepElementsProvider mepElementsProvider,
        IStructureLinksProvider structureLinksProvider,
        IOpeningGeometryProvider openingGeometryProvider)
        : base(revitRepository, mepElementsProvider, structureLinksProvider) {

        _openingGeometryProvider = openingGeometryProvider
            ?? throw new ArgumentNullException(nameof(openingGeometryProvider));
    }


    /// <summary>
    /// Находит коллизии между элементами ВИС из активного файла 
    /// и чистовыми отверстиями из связей в заданных конструкциях
    /// </summary>
    /// <param name="mepCategory">Категория элементов ВИС</param>
    /// <param name="mepFilterSet">Фильтр элементов ВИС</param>
    /// <param name="structureCategory">Категория элементов конструкций, в которых находятся отверстия</param>
    /// <param name="structureFilterSet">Фильтр конструкций, в которых находятся отверстия</param>
    /// <returns>Коллизии между элементами ВИС из активного файла и чистовыми отверстиями из связей</returns>
    protected ICollection<ClashModel> FindOpeningClashes(
        BuiltInCategory mepCategory,
        Set mepFilterSet,
        BuiltInCategory structureCategory,
        Set structureFilterSet) {

        var mepProvider = GetMepFilterProvider(mepCategory, mepFilterSet);
        var structureLinks = _structureLinksProvider.GetLinks();
        if(structureLinks.Count == 0) {
            return Array.Empty<ClashModel>();
        }
        var openingProviders = GetOpeningsProviders(
            structureLinks,
            structureCategory,
            structureFilterSet,
            _structureLinksProvider.GetOpeningFamilyNames());

        return [.. new ClashDetector(_revitRepository.GetClashRevitRepository(), [mepProvider], openingProviders)
            .FindClashes()];
    }

    protected ICollection<OpeningsFilterProvider> GetOpeningsProviders(
        ICollection<RevitLinkInstance> structureLinks,
        BuiltInCategory structureCategory,
        Set structureFilterSet,
        string[] openingFamilyNames) {

        var clashRepo = _revitRepository.GetClashRevitRepository();

        var structureFilter = new Filter(clashRepo) {
            CategoryIds = [new ElementId(structureCategory)],
            Name = structureCategory.ToString(),
            Set = structureFilterSet
        };
        return [.. structureLinks.Select(link => new OpeningsFilterProvider(
            link.GetLinkDocument(),
            structureFilter,
            link.GetTransform(),
            openingFamilyNames,
            _openingGeometryProvider))];
    }
}
