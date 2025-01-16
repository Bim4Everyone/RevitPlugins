using System.Collections.Generic;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services {
    internal interface IOneSourceLinksProvider {
        ICollection<ILink> GetLinks();
    }
}
