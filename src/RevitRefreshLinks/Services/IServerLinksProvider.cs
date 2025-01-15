using System.Collections.Generic;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services {
    internal interface IServerLinksProvider {
        ICollection<ILink> GetServerLinks();
    }
}
