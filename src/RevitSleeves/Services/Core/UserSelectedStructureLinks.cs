using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

using Ninject;
using Ninject.Syntax;

using RevitSleeves.Models;
using RevitSleeves.ViewModels.Filtration;
using RevitSleeves.Views.Filtration;

namespace RevitSleeves.Services.Core;
internal class UserSelectedStructureLinks : IStructureLinksProvider {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly IResolutionRoot _resolutionRoot;
    private List<ElementId> _linkTypeIds;

    public UserSelectedStructureLinks(RevitRepository revitRepository,
        ILocalizationService localizationService,
        IResolutionRoot resolutionRoot) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
        _resolutionRoot = resolutionRoot ?? throw new ArgumentNullException(nameof(resolutionRoot));
    }


    public ICollection<RevitLinkInstance> GetLinks() {
        if(_linkTypeIds is null) {
            SetLinks();
        }
        var links = _revitRepository.GetStructureLinkInstances()
                 .Where(l => _linkTypeIds.Contains(l.GetTypeId()))
                 .ToArray();
        string duplicatedLink = links.GroupBy(l => l.GetLinkDocument().Title).FirstOrDefault(g => g.Count() > 1)?.Key;
        if(!string.IsNullOrEmpty(duplicatedLink)) {
            throw new InvalidOperationException(
                string.Format(_localizationService.GetLocalizedString("Errors.DuplicatedLinks"), duplicatedLink));
        }
        if(links.Length == 0) {
            throw new InvalidOperationException(
                _localizationService.GetLocalizedString("Errors.CannotFindStructureLinks"));
        }
        return links;
    }

    public string[] GetOpeningFamilyNames() {
        return [.. NamesProvider.FamilyNamesAllOpenings];
    }

    private void SetLinks() {
        var window = _resolutionRoot.Get<StructureLinksSelectorWindow>();
        if(!(window.ShowDialog() ?? false)) {
            throw new OperationCanceledException();
        }
        var vm = (StructureLinksSelectorViewModel) window.DataContext;
        var linkTypes = vm.Links.Where(l => l.IsSelected).Select(l => l.GetLinkType()).ToArray();

        _linkTypeIds = [];
        foreach(var linkType in linkTypes) {
            if(!RevitLinkType.IsLoaded(_revitRepository.Document, linkType.Id)) {
                try {
                    var result = linkType.Load();
                    if(result.LoadResult != LinkLoadResultType.LinkLoaded) {
                        continue;
                    }
                } catch(Autodesk.Revit.Exceptions.ApplicationException) {
                    continue;
                }
            }
            _linkTypeIds.Add(linkType.Id);
        }

        _revitRepository.GetClashRevitRepository().InitializeDocInfos();
    }
}
