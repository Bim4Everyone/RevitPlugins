using System.IO;
using System.Threading.Tasks;

namespace RevitRefreshLinks.Models;
internal interface IDirectoryModel : IFileSystemInfoModel {
    Task<IDirectoryModel> GetParentAsync();

    Task<IDirectoryModel> GetRootAsync();

    Task<IDirectoryModel[]> GetDirectoriesAsync();

    Task<IDirectoryModel[]> GetDirectoriesAsync(SearchOption searchOption);

    Task<IFileModel[]> GetFilesAsync(string filter = "*.*");

    Task<IFileModel[]> GetFilesAsync(SearchOption searchOption, string filter = "*.*");
}
