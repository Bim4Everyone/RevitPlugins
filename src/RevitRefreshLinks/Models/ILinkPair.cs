using Autodesk.Revit.DB;

namespace RevitRefreshLinks.Models;
internal interface ILinkPair {
    RevitLinkType LocalLink { get; }

    ILink SourceLink { get; set; }
}
