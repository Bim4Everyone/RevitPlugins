using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

using RevitRefreshLinks.Models;

namespace RevitRefreshLinks.ViewModels {
    internal class DirectoryViewModel : PathInfoViewModel {
        private readonly IDirectoryModel _directoryModel;

        public DirectoryViewModel(IDirectoryModel directoryModel) {
            _directoryModel = directoryModel ?? throw new System.ArgumentNullException(nameof(directoryModel));
            Content = new ObservableCollection<PathInfoViewModel>();
        }

        public override string Name => _directoryModel.Name;

        public string FullName => _directoryModel.FullName;

        public override bool IsDirectory => true;

        public IDirectoryModel DirectoryModel => _directoryModel;
        public ObservableCollection<PathInfoViewModel> Content { get; }

        public async Task<DirectoryViewModel> GetParent() {
            return new DirectoryViewModel(await _directoryModel.GetParentAsync());
        }


        public async Task LoadContentAsync() {
            Content.Clear();
            var folders = (await _directoryModel.GetDirectoriesAsync()).Select(d => new DirectoryViewModel(d));
            foreach(var folder in folders) {
                Content.Add(folder);
            }
            var files = (await _directoryModel.GetFilesAsync()).Select(f => new FileViewModel(f));
            foreach(var file in files) {
                Content.Add(file);
            }
        }
    }
}
