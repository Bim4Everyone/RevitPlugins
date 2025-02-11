using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.ViewModels {
    internal class FileViewModel : PathInfoViewModel {
        private readonly IFileModel _fileModel;

        public FileViewModel(IFileModel fileModel) {
            _fileModel = fileModel ?? throw new System.ArgumentNullException(nameof(fileModel));
        }

        public override string Name => _fileModel.Name;

        public override bool IsDirectory => false;
    }
}
