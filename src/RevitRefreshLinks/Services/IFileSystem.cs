using System.Threading.Tasks;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Services;
internal interface IFileSystem {
    Task<IDirectoryModel> GetRootDirectoryAsync();

    Task<IDirectoryModel> GetDirectoryAsync(string path);
}
