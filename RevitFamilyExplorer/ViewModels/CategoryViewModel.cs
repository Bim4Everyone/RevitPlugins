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

using RevitFamilyExplorer.Models;

namespace RevitFamilyExplorer.ViewModels {
    internal class CategoryViewModel : BaseViewModel {
        private readonly DirectoryInfo _categoryFolder;
        private readonly RevitRepository _revitRepository;

        private readonly Dispatcher _dispatcher;
        private readonly FileSystemWatcher _folderWatcher;

        public CategoryViewModel(RevitRepository revitRepository, DirectoryInfo categoryFolder) {
            _categoryFolder = categoryFolder;
            _revitRepository = revitRepository;

            _dispatcher = Dispatcher.CurrentDispatcher;
            _folderWatcher = new FileSystemWatcher(_categoryFolder.FullName, "*.rfa") { EnableRaisingEvents = true };

            _folderWatcher.Created += _folderWatcher_Created;
            _folderWatcher.Renamed += _folderWatcher_Renamed;
            _folderWatcher.Deleted += _folderWatcher_Deleted;

            Name = _categoryFolder.Name;
            FamilyFiles = new ObservableCollection<FamilyFileViewModel>(GetFamilyFiles());
        }

        public string Name { get; }
        public ObservableCollection<FamilyFileViewModel> FamilyFiles { get; }

        private IEnumerable<FamilyFileViewModel> GetFamilyFiles() {
            return _categoryFolder.GetFiles("*.rfa").Select(item => new FamilyFileViewModel(_revitRepository, item));
        }

        private void _folderWatcher_Created(object sender, FileSystemEventArgs e) {
            _dispatcher.Invoke(() => {
                FamilyFiles.Add(new FamilyFileViewModel(_revitRepository, new FileInfo(e.FullPath)));
            });
        }

        private void _folderWatcher_Renamed(object sender, RenamedEventArgs e) {
            _dispatcher.Invoke(() => {
                var familyFile = FamilyFiles.FirstOrDefault(item => item.Name.Equals(e.OldName));
                if(familyFile != null) {
                    familyFile.FileInfo = new FileInfo(e.FullPath);
                }
            });
        }

        private void _folderWatcher_Deleted(object sender, FileSystemEventArgs e) {
            _dispatcher.Invoke(() => {
                var familyFile = FamilyFiles.FirstOrDefault(item => item.Name.Equals(e.Name));
                FamilyFiles.Remove(familyFile);
            });
        }
    }
}