using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services {
    internal interface IFileSystem {
        IDirectoryModel GetRootDirectory();

        IDirectoryModel GetDirectory(string path);
    }
}
