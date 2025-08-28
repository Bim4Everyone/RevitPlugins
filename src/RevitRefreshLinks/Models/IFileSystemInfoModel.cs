namespace RevitRefreshLinks.Models;
internal interface IFileSystemInfoModel {
    bool Exists { get; }

    string FullName { get; }

    string Name { get; }
}
