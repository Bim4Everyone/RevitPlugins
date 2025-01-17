using System.Collections.Generic;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services {
    internal interface ILinkSourceFinder {
        void FindAndSetSources(ICollection<ILinkPair> destinations, ICollection<ILink> sources);
    }
}
