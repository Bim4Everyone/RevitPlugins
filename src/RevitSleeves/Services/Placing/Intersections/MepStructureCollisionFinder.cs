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
internal abstract class MepStructureCollisionFinder {
    protected readonly RevitRepository _revitRepository;
    protected readonly IMepElementsProvider _mepElementsProvider;
    protected readonly IStructureLinksProvider _structureLinksProvider;

    protected MepStructureCollisionFinder(
        RevitRepository revitRepository,
        IMepElementsProvider mepElementsProvider,
        IStructureLinksProvider structureLinksProvider) {

        _revitRepository = revitRepository
            ?? throw new ArgumentNullException(nameof(revitRepository));
        _mepElementsProvider = mepElementsProvider
            ?? throw new ArgumentNullException(nameof(mepElementsProvider));
        _structureLinksProvider = structureLinksProvider
            ?? throw new ArgumentNullException(nameof(structureLinksProvider));
    }


    /// <summary>
    /// Находит коллизии между элементами ВИС из активного файла и конструкциями из связей
    /// </summary>
    /// <param name="mepCategory">Категория элементов ВИС</param>
    /// <param name="mepFilterSet">Фильтр элементов ВИС</param>
    /// <param name="structureCategory">Категория элементов конструкций</param>
    /// <param name="structureFilterSet">Фильтр конструкций</param>
    /// <returns>Коллизии между элементами ВИС из активного файла и конструкциями из связей</returns>
    protected ICollection<ClashModel> FindStructureClashes<TStructure>(
        BuiltInCategory mepCategory,
        Set mepFilterSet,
        BuiltInCategory structureCategory,
        Set structureFilterSet) where TStructure : Element {

        var mepProvider = GetMepFilterProvider(mepCategory, mepFilterSet);
        var structureLinks = _structureLinksProvider.GetLinks();
        if(structureLinks.Count == 0) {
            return Array.Empty<ClashModel>();
        }
        var structureProviders = GetStructureFilterProviders<TStructure>(
            structureLinks, structureCategory, structureFilterSet);
        return [.. new ClashDetector(_revitRepository.GetClashRevitRepository(), [mepProvider], structureProviders)
            .FindClashes()];
    }

    protected FilterProvider GetMepFilterProvider(BuiltInCategory mepCategory, Set mepFilterSet) {
        var clashRepo = _revitRepository.GetClashRevitRepository();

        var pipeFilter = new Filter(clashRepo) {
            CategoryIds = [new ElementId(mepCategory)],
            Name = mepCategory.ToString(),
            Set = mepFilterSet
        };
        return new FilterProvider(_revitRepository.Document,
            pipeFilter,
            Transform.Identity,
            [.. _mepElementsProvider.GetMepElementIds(mepCategory)]);
    }

    protected ICollection<FilterProvider> GetStructureFilterProviders<T>(
        ICollection<RevitLinkInstance> structureLinks,
        BuiltInCategory structureCategory,
        Set structureFilterSet) where T : Element {

        var clashRepo = _revitRepository.GetClashRevitRepository();

        var structureFilter = new Filter(clashRepo) {
            CategoryIds = [new ElementId(structureCategory)],
            Name = structureCategory.ToString(),
            Set = structureFilterSet
        };
        return [.. structureLinks.Select(link => new FilterProvider(
            link.GetLinkDocument(),
            structureFilter,
            link.GetTransform(),
            // дополнительный фильтр по классу, т.к. в плагине обрабатываются только системные семейства
            [.. _revitRepository.GetLinkedElementIds<T>(link)]))];
    }
}
