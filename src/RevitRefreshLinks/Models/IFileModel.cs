namespace RevitRefreshLinks.Models;
internal interface IFileModel : IFileSystemInfoModel {
    IDirectoryModel Directory { get; }

    string DirectoryName { get; }

    long Length { get; }
}
