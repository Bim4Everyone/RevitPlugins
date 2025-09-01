using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.ViewModels;
internal class DirectoryViewModel : PathInfoViewModel, IEquatable<DirectoryViewModel> {
    public DirectoryViewModel(IDirectoryModel directoryModel) {
        DirectoryModel = directoryModel ?? throw new System.ArgumentNullException(nameof(directoryModel));
        Content = [];
    }

    public override string Name => DirectoryModel.Name;

    public override string FullName => DirectoryModel.FullName;

    public override bool IsDirectory => true;

    public IDirectoryModel DirectoryModel { get; }

    public override long Length => 0;

    public ObservableCollection<PathInfoViewModel> Content { get; }


    public async Task<DirectoryViewModel> GetParent() {
        return new DirectoryViewModel(await DirectoryModel.GetParentAsync());
    }

    public async Task LoadContentAsync(bool loadFiles = false, string filter = "*.*") {
        Content.Clear();
        var folders = await DirectoryModel.GetDirectoriesAsync();
        var foldersVms = folders.Select(d => new DirectoryViewModel(d)).ToArray();
        foreach(var folderVm in foldersVms) {
            Content.Add(folderVm);
        }
        if(loadFiles) {
            var files = await DirectoryModel.GetFilesAsync(filter);
            var filesVms = files.Select(f => new FileViewModel(f));
            foreach(var fileVm in filesVms) {
                Content.Add(fileVm);
            }
        }
    }

    public bool Equals(DirectoryViewModel other) {
        if(other is null) { return false; }
        return ReferenceEquals(this, other) || FullName.Equals(other.FullName);
    }

    public override bool Equals(object obj) {
        return Equals(obj as DirectoryViewModel);
    }

    public override int GetHashCode() {
        return 733961487 + EqualityComparer<string>.Default.GetHashCode(FullName);
    }
}
