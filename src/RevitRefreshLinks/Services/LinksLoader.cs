using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services {
    internal class LinksLoader : ILinksLoader {
        private readonly RevitRepository _revitRepository;

        public LinksLoader(RevitRepository revitRepository) {
            _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        }


        public ICollection<(ILink Link, string Error)> AddLinks(
            ICollection<ILink> links,
            IProgress<int> progress = null,
            CancellationToken ct = default) {

            var errors = new List<(ILink Link, string Error)>();
            var existingLinks = _revitRepository.GetExistingLinks();
            using(var tGroup = _revitRepository.Document.StartTransactionGroup("Добавление связей")) {
                var i = 0;
                foreach(var link in links) {
                    ct.ThrowIfCancellationRequested();
                    using(var transaction = _revitRepository.Document.StartTransaction("Добавление связи")) {
                        bool success;
                        string error;
                        if(LinkExists(existingLinks, link, out RevitLinkType existingLink)) {
                            success = false;
                            error = "Файл уже добавлен в документ";
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
            var i = 0;
            foreach(var item in links) {
                ct.ThrowIfCancellationRequested();
                bool success = _revitRepository.ReloadLink(
                    item.LocalLink,
                    item.SourceLink.FullPath,
                    out string error);
                if(!success) {
                    errors.Add((item.SourceLink, error));
                }
                progress?.Report(++i);
            }
            return errors;
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
}
