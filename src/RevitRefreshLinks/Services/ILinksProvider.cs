using System.Collections.Generic;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services {
    internal interface ILinksProvider {
        ICollection<ILink> GetFolderLinks();

        ICollection<ILink> GetServerLinks();
    }
}
