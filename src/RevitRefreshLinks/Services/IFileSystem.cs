using System.Threading.Tasks;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services {
    internal interface IFileSystem {
        Task<IDirectoryModel> GetRootDirectory();

        Task<IDirectoryModel> GetDirectory(string path);
    }
}
