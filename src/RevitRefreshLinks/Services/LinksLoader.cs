using System;
using System.Collections.Generic;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services {
    internal class LinksLoader : ILinksLoader {
        public LinksLoader() {
        }

        public void LoadLinks(ICollection<ILink> links) {
            throw new NotImplementedException();
        }

        public void ReloadLinks(bool loadUnloadedLinks = false) {
            throw new NotImplementedException();
        }

        public void UpdateLinks(ICollection<ILinkPair> links) {
            throw new NotImplementedException();
        }
    }
}
