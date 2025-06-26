using System;
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
internal abstract class MepStructureCollisionFinder {
    protected ICollection<ClashModel> FindClashes(
        RevitRepository repository,
        IMepElementsProvider mepElementsProvider,
        IStructureLinksProvider structureLinksProvider,
        MepCategorySettings mepSettings,
        StructureSettings structureSettings) {

        var mepProvider = GetMepFilterProvider(repository, mepElementsProvider, mepSettings);
        var structureLinks = structureLinksProvider.GetLinks();
        if(structureLinks.Count == 0) {
            return Array.Empty<ClashModel>();
        }
        var structureProviders = GetStructureFilterProviders(repository, structureLinks, structureSettings);
        return [.. new ClashDetector(repository.GetClashRevitRepository(), [mepProvider], structureProviders)
            .FindClashes()];
    }

    private FilterProvider GetMepFilterProvider(
        RevitRepository repository,
        IMepElementsProvider mepElementsProvider,
        MepCategorySettings mepSettings) {

        var clashRepo = repository.GetClashRevitRepository();

        var pipeCategory = repository.GetCategory(mepSettings.Category);
        var pipeFilter = new Filter(clashRepo) {
            CategoryIds = [pipeCategory.Id],
            Name = pipeCategory.Name,
            Set = mepSettings.MepFilterSet
        };
        return new FilterProvider(repository.Document,
            pipeFilter,
            Transform.Identity,
            [.. mepElementsProvider.GetMepElementIds(mepSettings.Category)]);
    }

    private ICollection<FilterProvider> GetStructureFilterProviders(
        RevitRepository repository,
        ICollection<RevitLinkInstance> structureLinks,
        StructureSettings structureSettings) {

        var clashRepo = repository.GetClashRevitRepository();

        var structureCategory = repository.GetCategory(structureSettings.Category);
        var structureFilter = new Filter(clashRepo) {
            CategoryIds = [structureCategory.Id],
            Name = structureCategory.Name,
            Set = structureSettings.FilterSet
        };
        return [.. structureLinks.Select(link => new FilterProvider(
            link.GetLinkDocument(),
            structureFilter,
            link.GetTransform(),
            [.. repository.GetLinkedElementIds<Wall>(link)]))];
    }
}
