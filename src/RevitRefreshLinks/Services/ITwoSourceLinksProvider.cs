using System.Collections.Generic;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services {
    internal interface ITwoSourceLinksProvider {
        ICollection<ILink> GetLocalLinks();
        ICollection<ILink> GetServerLinks();
    }
}
