using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration.Controls;

using RevitClashDetective.Models.ClashDetection;
using RevitClashDetective.Models.Clashes;

using RevitSleeves.Models;
using RevitSleeves.Services.Core;

namespace RevitSleeves.Services.Placing.Intersections;
internal abstract class MepOpeningCollisionFinder : MepStructureCollisionFinder {
    protected readonly IOpeningGeometryProvider _openingGeometryProvider;


    protected MepOpeningCollisionFinder(
        RevitRepository revitRepository,
        IMepElementsProvider mepElementsProvider,
        IStructureLinksProvider structureLinksProvider,
        IOpeningGeometryProvider openingGeometryProvider,
        IFilterContextParser filterContextParser)
        : base(revitRepository, mepElementsProvider, structureLinksProvider, filterContextParser) {

        _openingGeometryProvider = openingGeometryProvider
            ?? throw new ArgumentNullException(nameof(openingGeometryProvider));
    }


    /// <summary>
    /// Находит коллизии между элементами ВИС из активного файла
    /// и чистовыми отверстиями из связей в заданных конструкциях
    /// </summary>
    /// <param name="mepCategory">Категория элементов ВИС</param>
    /// <param name="mepFilterContext">Сериализованный контекст фильтра элементов ВИС</param>
    /// <param name="structureCategory">Категория элементов конструкций, в которых находятся отверстия</param>
    /// <param name="structureFilterContext">Сериализованный контекст фильтра конструкций, в которых находятся отверстия</param>
    /// <returns>Коллизии между элементами ВИС из активного файла и чистовыми отверстиями из связей</returns>
    protected ICollection<ClashModel> FindOpeningClashes(
        BuiltInCategory mepCategory,
        string mepFilterContext,
        BuiltInCategory structureCategory,
        string structureFilterContext) {

        var mepProvider = GetMepFilterProvider(mepCategory, mepFilterContext);
        var structureLinks = _structureLinksProvider.GetLinks();
        if(structureLinks.Count == 0) {
            return Array.Empty<ClashModel>();
        }
        var openingProviders = GetOpeningsProviders(
            structureLinks,
            structureCategory,
            structureFilterContext,
            _structureLinksProvider.GetOpeningFamilyNames());

        return [.. new ClashDetector(_revitRepository.GetClashRevitRepository(), [mepProvider], openingProviders)
            .FindClashes()];
    }

    protected ICollection<OpeningsFilterProvider> GetOpeningsProviders(
        ICollection<RevitLinkInstance> structureLinks,
        BuiltInCategory structureCategory,
        string structureFilterContext,
        string[] openingFamilyNames) {

        var filterContext = ParseFilterContext(structureFilterContext, structureCategory);

        return [.. structureLinks.Select(link => new OpeningsFilterProvider(
            link.GetLinkDocument(),
            filterContext,
            link.GetTransform(),
            openingFamilyNames,
            _openingGeometryProvider))];
    }
}
