using System;
using System.IO;
using System.Threading.Tasks;

using RevitRefreshLinks.Models;
using RevitRefreshLinks.Services;

namespace RevitRefreshLinks.Mock {
    internal class MockFileSystem : IFileSystem {
        public async Task<IDirectoryModel> GetDirectoryAsync(string path) {
            return await Task.FromResult(new MockDirectory(new System.IO.DirectoryInfo(path)));
        }

        public async Task<IDirectoryModel> GetRootDirectoryAsync() {
            return await Task.FromResult(
                new MockDirectory(
                    new DirectoryInfo(
                        Directory.GetDirectoryRoot(
                            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)))));
        }
    }
}
