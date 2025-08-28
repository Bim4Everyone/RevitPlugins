namespace RevitRefreshLinks.Models;
internal interface ILink {
    string Name { get; }

    string NameWithExtension { get; }

    string FullPath { get; }
}
