using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Mock {
    internal class MockDirectory : IDirectoryModel {
        private readonly DirectoryInfo _directoryInfo;

        public MockDirectory(DirectoryInfo directoryInfo) {
            _directoryInfo = directoryInfo ?? throw new ArgumentNullException(nameof(directoryInfo));
        }
        public bool Exists => _directoryInfo.Exists;

        public string FullName => _directoryInfo.FullName;

        public string Name => _directoryInfo.Name;

        public async Task<IDirectoryModel[]> GetDirectoriesAsync() {
            return await Task.FromResult(
                _directoryInfo.GetDirectories()
                .Select(d => new MockDirectory(d))
                .ToArray());
        }

        public Task<IDirectoryModel[]> GetDirectoriesAsync(SearchOption searchOption) {
            throw new NotImplementedException();
        }

        public async Task<IFileModel[]> GetFilesAsync() {
            return await Task.FromResult(
                _directoryInfo.GetFiles()
                .Select(f => new MockFile(f))
                .ToArray());
        }

        public Task<IFileModel[]> GetFilesAsync(SearchOption searchOption) {
            throw new NotImplementedException();
        }

        public async Task<IDirectoryModel> GetParentAsync() {
            return await Task.FromResult(new MockDirectory(_directoryInfo.Parent));
        }

        public async Task<IDirectoryModel> GetRootAsync() {
            return await Task.FromResult(new MockDirectory(_directoryInfo.Root));
        }
    }
}
