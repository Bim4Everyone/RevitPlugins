using System.Collections.Generic;
using System.ComponentModel;

namespace RevitBatchPrint.ViewModels {
    public class PrintAbumsViewModel : INotifyPropertyChanged {
        private PrintAlbumViewModel _selectedName;
        private List<PrintAlbumViewModel> _names;

        public PrintAlbumViewModel SelectedAlbum {
            get => _selectedName;
            set {
                _selectedName = value;
                OnPropertyChanged(nameof(SelectedAlbum));
            }
        }
        
        public List<PrintAlbumViewModel> Albums { 
            get => _names;
            set {
                _names = value;
                OnPropertyChanged(nameof(Albums));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
