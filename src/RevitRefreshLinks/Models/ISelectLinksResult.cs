using System.Collections.Generic;

namespace RevitRefreshLinks.Models {
    internal interface ISelectLinksResult {
        string SourceName { get; }
        ICollection<ILink> Links { get; }
    }
}
