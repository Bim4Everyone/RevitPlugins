using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services;
internal interface IOpenFolderDialog : IOpenDialogBase {
    IDirectoryModel Folder { get; }

    IDirectoryModel[] Folders { get; }
}
