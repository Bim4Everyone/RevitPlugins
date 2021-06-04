using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using dosymep.Revit.ServerClient;

namespace RevitServerFolders {
    /// <summary>
    /// Interaction logic for SelectServerRvtFolderWindow.xaml
    /// </summary>
    public partial class SelectServerRvtFolderWindow : Window {
        public SelectServerRvtFolderWindow() {
            InitializeComponent();
        }

        private void _btOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }
    }

    internal class RevitServerViewModel : INotifyPropertyChanged {
        private readonly IRevitServerClient _revitServerClient;
        
        private string _currentFolder;
        private ObservableCollection<string> _revitFolders;

        public RevitServerViewModel() { }

        public RevitServerViewModel(IRevitServerClient revitServerClient) {
            _revitServerClient = revitServerClient;
        }

        public string CurrentFolder {
            get => _currentFolder;
            set {
                _currentFolder = value;
                OnPropertyChanged(nameof(CurrentFolder));
            }
        }

        public ObservableCollection<string> RevitFolders {
            get => _revitFolders;
            set {
                _revitFolders = value;
                OnPropertyChanged(nameof(RevitFolders));
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
