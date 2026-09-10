using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration.Controls;

using RevitClashDetective.Models.ClashDetection;
using RevitClashDetective.Models.Clashes;

using RevitSleeves.Models;
using RevitSleeves.Models.Filtration;
using RevitSleeves.Services.Core;

namespace RevitSleeves.Services.Placing.Intersections;
internal abstract class MepStructureCollisionFinder {
    protected readonly RevitRepository _revitRepository;
    protected readonly IMepElementsProvider _mepElementsProvider;
    protected readonly IStructureLinksProvider _structureLinksProvider;
    protected readonly IFilterContextParser _filterContextParser;

    protected MepStructureCollisionFinder(
        RevitRepository revitRepository,
        IMepElementsProvider mepElementsProvider,
        IStructureLinksProvider structureLinksProvider,
        IFilterContextParser filterContextParser) {

        _revitRepository = revitRepository
            ?? throw new ArgumentNullException(nameof(revitRepository));
        _mepElementsProvider = mepElementsProvider
            ?? throw new ArgumentNullException(nameof(mepElementsProvider));
        _structureLinksProvider = structureLinksProvider
            ?? throw new ArgumentNullException(nameof(structureLinksProvider));
        _filterContextParser = filterContextParser
                               ?? throw new ArgumentNullException(nameof(filterContextParser));
    }


    /// <summary>
    /// Находит коллизии между элементами ВИС из активного файла и конструкциями из связей
    /// </summary>
    /// <param name="mepCategory">Категория элементов ВИС</param>
    /// <param name="mepFilterContext">Сериализованный контекст фильтра элементов ВИС</param>
    /// <param name="structureCategory">Категория элементов конструкций</param>
    /// <param name="structureFilterContext">Сериализованный контекст фильтра конструкций</param>
    /// <returns>Коллизии между элементами ВИС из активного файла и конструкциями из связей</returns>
    protected ICollection<ClashModel> FindStructureClashes<TStructure>(
        BuiltInCategory mepCategory,
        string mepFilterContext,
        BuiltInCategory structureCategory,
        string structureFilterContext) where TStructure : Element {
        var mepProvider = GetMepFilterProvider(mepCategory, mepFilterContext);
        var structureLinks = _structureLinksProvider.GetLinks();
        if(structureLinks.Count == 0) {
            return Array.Empty<ClashModel>();
        }
        var structureProviders = GetStructureFilterProviders<TStructure>(
            structureLinks,
            structureCategory,
            structureFilterContext);
        return [.. new ClashDetector(_revitRepository.GetClashRevitRepository(), [mepProvider], structureProviders)
            .FindClashes()];
    }

    protected FilterableElementsProvider GetMepFilterProvider(BuiltInCategory mepCategory, string mepFilterContext) {
        var filterContext = ParseFilterContext(mepFilterContext, mepCategory);
        return new FilterableElementsProvider(
            _revitRepository.Document,
            filterContext,
            Transform.Identity,
            [.. _mepElementsProvider.GetMepElementIds(mepCategory)]);
    }

    protected ICollection<FilterableElementsProvider> GetStructureFilterProviders<T>(
        ICollection<RevitLinkInstance> structureLinks,
        BuiltInCategory structureCategory,
        string structureFilterContext) where T : Element {
        var filterContext = ParseFilterContext(structureFilterContext, structureCategory);
        return [
            .. structureLinks.Select(link => new FilterableElementsProvider(
            link.GetLinkDocument(),
            filterContext,
            link.GetTransform(),
            // дополнительный фильтр по классу, т.к. в плагине обрабатываются только системные семейства
            [.. _revitRepository.GetLinkedElementIds<T>(link)]))];
    }

    protected ILogicalFilterContext ParseFilterContext(string filterContext, BuiltInCategory category) {
        if(_filterContextParser.TryParse(filterContext, out var context)) {
            return context;
        }

        throw new InvalidOperationException($"Не удалось прочитать фильтр категории {category}");
    }
}
