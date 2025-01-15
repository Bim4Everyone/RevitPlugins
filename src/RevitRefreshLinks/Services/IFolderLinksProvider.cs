using System.Collections.Generic;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services {
    internal interface IFolderLinksProvider {
        ICollection<ILink> GetFolderLinks();
    }
}
