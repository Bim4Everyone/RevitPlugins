using System;
using System.Collections.Generic;
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

namespace RevitBatchPrint {
    /// <summary>
    /// Interaction logic for PrintViewSheetNames.xaml
    /// </summary>
    public partial class PrintViewSheetNamesWindow : Window {
        public PrintViewSheetNamesWindow() {
            InitializeComponent();
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }

    public class ViewSheetNamesViewModel : INotifyPropertyChanged {
        private string _selectedName;
        private List<string> _names;

        public string SelectedName {
            get => _selectedName;
            set {
                _selectedName = value;
                OnPropertyChanged(nameof(SelectedName));
            }
        }
        
        public List<string> Names { 
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
