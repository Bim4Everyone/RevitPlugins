using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

using dosymep.WPF.ViewModels;

namespace RevitFamilyExplorer.ViewModels {
    internal class CategoryViewModel : BaseViewModel {
        private readonly DirectoryInfo _categoryFolder;
        private readonly FileSystemWatcher _folderWatcher;
        private readonly Dispatcher _dispatcher;

        public CategoryViewModel(DirectoryInfo categoryFolder) {
            _categoryFolder = categoryFolder;
            _folderWatcher = new FileSystemWatcher(_categoryFolder.FullName, "*.rfa") { EnableRaisingEvents = true };

            _folderWatcher.Created += _folderWatcher_Created;
            _folderWatcher.Renamed += _folderWatcher_Renamed;
            _folderWatcher.Deleted += _folderWatcher_Deleted;

            _dispatcher = Dispatcher.CurrentDispatcher;

            Name = _categoryFolder.Name;
            FamilyFiles = new ObservableCollection<FamilyFileViewModel>(GetFamilyFiles());
        }

        public string Name { get; }
        public ObservableCollection<FamilyFileViewModel> FamilyFiles { get; }

        private IEnumerable<FamilyFileViewModel> GetFamilyFiles() {
            return _categoryFolder.GetFiles("*.rfa").Select(item => new FamilyFileViewModel(item));
        }

        private void _folderWatcher_Created(object sender, FileSystemEventArgs e) {
            _dispatcher.Invoke(() => {
                FamilyFiles.Add(new FamilyFileViewModel(new FileInfo(e.FullPath)));
            });
        }

        private void _folderWatcher_Renamed(object sender, RenamedEventArgs e) {
            var familyFile = FamilyFiles.FirstOrDefault(item => item.Name.Equals(e.OldName));
            familyFile?.Refresh(new FileInfo(e.FullPath));
        }

        private void _folderWatcher_Deleted(object sender, FileSystemEventArgs e) {
            _dispatcher.Invoke(() => {
                var familyFile = FamilyFiles.FirstOrDefault(item => item.Name.Equals(e.Name));
                FamilyFiles.Remove(familyFile);
            });
        }
    }
}