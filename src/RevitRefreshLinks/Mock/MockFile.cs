using System;
using System.IO;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.Mock {
    internal class MockFile : IFileModel {
        private readonly FileInfo _fileInfo;

        public MockFile(FileInfo fileInfo) {
            _fileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
        }
        public IDirectoryModel Directory => new MockDirectory(_fileInfo.Directory);

        public string DirectoryName => _fileInfo.DirectoryName;

        public long Length => _fileInfo.Length;

        public bool Exists => _fileInfo.Exists;

        public string FullName => _fileInfo.FullName;

        public string Name => _fileInfo.Name;
    }
}
