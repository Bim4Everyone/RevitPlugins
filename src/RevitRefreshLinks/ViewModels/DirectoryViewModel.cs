using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.ViewModels {
    internal class DirectoryViewModel : PathInfoViewModel, IEquatable<DirectoryViewModel> {
        private readonly IDirectoryModel _directoryModel;

        public DirectoryViewModel(IDirectoryModel directoryModel) {
            _directoryModel = directoryModel ?? throw new System.ArgumentNullException(nameof(directoryModel));
            Content = new ObservableCollection<PathInfoViewModel>();
        }

        public override string Name => _directoryModel.Name;

        public override string FullName => _directoryModel.FullName;

        public override bool IsDirectory => true;

        public IDirectoryModel DirectoryModel => _directoryModel;

        public override long Length => 0;

        public ObservableCollection<PathInfoViewModel> Content { get; }


        public async Task<DirectoryViewModel> GetParent() {
            return new DirectoryViewModel(await _directoryModel.GetParentAsync());
        }

        public async Task LoadContentAsync(bool loadFiles = false, string filter = "*.*") {
            Content.Clear();
            var folders = await _directoryModel.GetDirectoriesAsync();
            var foldersVms = folders.Select(d => new DirectoryViewModel(d)).ToArray();
            foreach(var folderVm in foldersVms) {
                Content.Add(folderVm);
            }
            if(loadFiles) {
                var files = await _directoryModel.GetFilesAsync(filter);
                var filesVms = files.Select(f => new FileViewModel(f));
                foreach(var fileVm in filesVms) {
                    Content.Add(fileVm);
                }
            }
        }

        public bool Equals(DirectoryViewModel other) {
            if(ReferenceEquals(null, other)) { return false; }
            if(ReferenceEquals(this, other)) { return true; }

            return FullName.Equals(other.FullName);
        }

        public override bool Equals(object obj) {
            return Equals(obj as DirectoryViewModel);
        }

        public override int GetHashCode() {
            return 733961487 + EqualityComparer<string>.Default.GetHashCode(FullName);
        }
    }
}
