using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.ViewModels;
internal class FileViewModel : PathInfoViewModel {
    public FileViewModel(IFileModel fileModel) {
        FileModel = fileModel ?? throw new System.ArgumentNullException(nameof(fileModel));
    }

    public override string Name => FileModel.Name;

    public override bool IsDirectory => false;

    public override string FullName => FileModel.FullName;

    public IFileModel FileModel { get; }

    public IDirectoryModel DirectoryModel => FileModel.Directory;

    public override long Length => FileModel.Length;
}
