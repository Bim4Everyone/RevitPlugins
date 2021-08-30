using System.Collections.Generic;
using System.ComponentModel;

namespace RevitBatchPrint.ViewModels {
    public class ViewSheetNamesViewModel : INotifyPropertyChanged {
        private ViewSheetNamesCountViewModel _selectedName;
        private List<ViewSheetNamesCountViewModel> _names;

        public ViewSheetNamesCountViewModel SelectedName {
            get => _selectedName;
            set {
                _selectedName = value;
                OnPropertyChanged(nameof(SelectedName));
            }
        }
        
        public List<ViewSheetNamesCountViewModel> Names { 
            get => _names;
            set {
                _names = value;
                OnPropertyChanged(nameof(Names));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
