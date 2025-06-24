using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.ClashDetection;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.FilterModel;

using RevitSleeves.Models;
using RevitSleeves.Models.Config;
using RevitSleeves.Services.Core;

namespace RevitSleeves.Services.Placing.Intersections;
internal abstract class MepOpeningCollisionFinder {
    private readonly RevitRepository _revitRepository;
    private readonly IOpeningGeometryProvider _openingGeometryProvider;

    protected MepOpeningCollisionFinder(
        RevitRepository revitRepository,
        IOpeningGeometryProvider openingGeometryProvider) {

        _revitRepository = revitRepository
            ?? throw new System.ArgumentNullException(nameof(revitRepository));
        _openingGeometryProvider = openingGeometryProvider
            ?? throw new System.ArgumentNullException(nameof(openingGeometryProvider));
    }


    protected ICollection<ClashModel> FindClashes(
        IMepElementsProvider mepElementsProvider,
        IStructureLinksProvider structureLinksProvider,
        MepCategorySettings mepSettings,
        StructureSettings structureSettings,
        string[] openingFamilyNames) {

        var mepProvider = GetMepFilterProvider(mepElementsProvider, mepSettings);
        var openingProviders = GetOpeningsProviders(structureLinksProvider, structureSettings, openingFamilyNames);
        return [.. new ClashDetector(_revitRepository.GetClashRevitRepository(), [mepProvider], openingProviders)
            .FindClashes()];
    }

    private FilterProvider GetMepFilterProvider(
        IMepElementsProvider mepElementsProvider,
        MepCategorySettings mepSettings) {

        var clashRepo = _revitRepository.GetClashRevitRepository();

        var pipeCategory = _revitRepository.GetCategory(mepSettings.Category);
        var pipeFilter = new Filter(clashRepo) {
            CategoryIds = [pipeCategory.Id],
            Name = pipeCategory.Name,
            Set = mepSettings.MepFilterSet
        };
        return new FilterProvider(_revitRepository.Document,
            pipeFilter,
            Transform.Identity,
            [.. mepElementsProvider.GetMepElementIds(mepSettings.Category)]);
    }

    private ICollection<OpeningsFilterProvider> GetOpeningsProviders(
        IStructureLinksProvider structureLinksProvider,
        StructureSettings structureSettings,
        string[] openingFamilyNames) {

        var clashRepo = _revitRepository.GetClashRevitRepository();

        var structureCategory = _revitRepository.GetCategory(structureSettings.Category);
        var structureFilter = new Filter(clashRepo) {
            CategoryIds = [structureCategory.Id],
            Name = structureCategory.Name,
            Set = structureSettings.FilterSet
        };
        return [.. structureLinksProvider.GetLinks()
            .Select(link => new OpeningsFilterProvider(
                link.GetLinkDocument(),
                structureFilter,
                link.GetTransform(),
                openingFamilyNames,
                _openingGeometryProvider))];
    }
}
