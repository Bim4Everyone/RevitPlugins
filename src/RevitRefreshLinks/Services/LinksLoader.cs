using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services;
internal class LinksLoader : ILinksLoader {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    public LinksLoader(RevitRepository revitRepository, ILocalizationService localizationService) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _localizationService = localizationService ?? throw new ArgumentNullException(nameof(localizationService));
    }


    public ICollection<(ILink Link, string Error)> AddLinks(
        ICollection<ILink> links,
        IProgress<int> progress = null,
        CancellationToken ct = default) {

        var errors = new List<(ILink Link, string Error)>();
        var existingLinks = _revitRepository.GetExistingLinks();
        string transTitle = _localizationService.GetLocalizedString("LinkLoader.Transaction.AddLinks");
        using(var tGroup = _revitRepository.Document.StartTransactionGroup(transTitle)) {
            int i = 0;
            foreach(var link in links) {
                ct.ThrowIfCancellationRequested();
                using(var transaction = _revitRepository.Document.StartTransaction(transTitle)) {
                    bool success;
                    string error;
                    if(LinkExists(existingLinks, link, out var existingLink)) {
                        success = false;
                        error = _localizationService.GetLocalizedString("LinkLoader.Error.AlreadyAdded");
                    } else {
                        success = _revitRepository.AddLink(link.FullPath, out error);
                    }
                    if(success) {
                        transaction.Commit();
                    } else {
                        transaction.RollBack();
                        errors.Add((link, error));
                    }
                }
                progress?.Report(++i);
            }
            tGroup.Assimilate();
        }
        return errors;
    }

    public ICollection<(ILink Link, string Error)> UpdateLinks(
        ICollection<ILinkPair> links,
        IProgress<int> progress = null,
        CancellationToken ct = default) {

        var errors = new List<(ILink Link, string Error)>();
        int i = 0;
        List<RevitLinkType> reloadedLinkTypes = [];
        foreach(var item in links) {
            ct.ThrowIfCancellationRequested();
            bool success = _revitRepository.ReloadLink(
                item.LocalLink,
                item.SourceLink.FullPath,
                out string error);
            if(success) {
                reloadedLinkTypes.Add(item.LocalLink);
            } else {
                errors.Add((item.SourceLink, error));
            }
            progress?.Report(++i);
        }

        // закрепление связей после обновления, т.к. ревит запрещает обновлять связи при запущенной транзакции
        PinLinkInstances(reloadedLinkTypes);

        return errors;
    }

    private void PinLinkInstances(ICollection<RevitLinkType> linkTypes) {
        using var transaction = _revitRepository.Document.StartTransaction(
            _localizationService.GetLocalizedString("LinkLoader.Transaction.PinLinks"));
        foreach(var link in linkTypes) {
            var instances = new FilteredElementCollector(_revitRepository.Document)
                .OfClass(typeof(RevitLinkInstance))
                .OfType<RevitLinkInstance>()
                .Where(r => r.GetTypeId() == link.Id)
                .ToArray();
            foreach(var instance in instances) {
                instance.Pinned = true;
            }
        }

        transaction.Commit();
    }

    private bool LinkExists(
        ICollection<RevitLinkType> existingLinks,
        ILink linkToCheck,
        out RevitLinkType existingLink) {

        existingLink = existingLinks.FirstOrDefault(
            t => t.Name.Equals(linkToCheck.NameWithExtension, StringComparison.InvariantCultureIgnoreCase));
        return existingLink != null;
    }
}
