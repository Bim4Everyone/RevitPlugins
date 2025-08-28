using System.Collections.Generic;

namespace RevitRefreshLinks.Models;
internal interface ILinksFromSource {
    string SourceName { get; }
    ICollection<ILink> Links { get; }
}
