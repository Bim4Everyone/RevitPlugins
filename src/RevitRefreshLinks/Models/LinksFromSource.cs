using System.Collections.Generic;

namespace RevitRefreshLinks.Models;
internal class LinksFromSource : ILinksFromSource {
    public LinksFromSource(string sourceName, ICollection<ILink> links) {
        if(string.IsNullOrWhiteSpace(sourceName)) {
            throw new System.ArgumentException(nameof(sourceName));
        }

        SourceName = sourceName;
        Links = links ?? throw new System.ArgumentNullException(nameof(links));
    }


    public string SourceName { get; }

    public ICollection<ILink> Links { get; }
}
