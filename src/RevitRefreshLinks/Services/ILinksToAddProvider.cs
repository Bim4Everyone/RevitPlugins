using System.Collections.Generic;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services {
    internal interface ILinksToAddProvider {
        ICollection<ILink> GetLinks();
    }
}
