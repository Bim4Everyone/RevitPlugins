using System;

using Autodesk.Revit.DB;

namespace RevitRefreshLinks.Models;
internal class LinkPair : ILinkPair {
    public LinkPair(RevitLinkType localLink, ILink sourceLink) {
        LocalLink = localLink ?? throw new ArgumentNullException(nameof(localLink));
        SourceLink = sourceLink ?? throw new ArgumentNullException(nameof(sourceLink));
    }


    public RevitLinkType LocalLink { get; }

    public ILink SourceLink { get; set; }
}
