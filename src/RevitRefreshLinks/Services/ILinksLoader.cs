using System.Collections.Generic;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services {
    internal interface ILinksLoader {
        void ReloadLinks(bool loadUnloadedLinks = false);

        void LoadLinks(ICollection<ILink> links);

        void UpdateLinks(ICollection<ILinkPair> links);
    }
}
