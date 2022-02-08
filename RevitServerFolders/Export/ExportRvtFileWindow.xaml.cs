using System;
using System.Collections.Generic;
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

using dosymep.WPF.Views;

namespace RevitServerFolders.Export {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class ExportRvtFileWindow : PlatformWindow {
        public ExportRvtFileWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitServerFolders);
        public override string ProjectConfigName => nameof(ExportRvtFileWindow);

        private void _btOk_Click(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }
    }

    public class MultiConverter : IMultiValueConverter {
        public object Convert(object[] values, Type targetType, object parameter,
            System.Globalization.CultureInfo culture) {
            bool withNwcFiles = (bool) values[0];
            bool useSourceRevit = (bool) values[1];
            return !useSourceRevit && withNwcFiles;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter,
            System.Globalization.CultureInfo culture) {
            throw new NotImplementedException();
        }
    }
}
