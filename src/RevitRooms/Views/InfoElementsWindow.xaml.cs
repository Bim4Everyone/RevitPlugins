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
using System.Windows.Shapes;

using dosymep.WPF.Views;

namespace RevitRooms.Views {
    /// <summary>
    /// Interaction logic for InfoElementsWindow.xaml
    /// </summary>
    public partial class InfoElementsWindow {
        public InfoElementsWindow() {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitRooms);
        public override string ProjectConfigName => nameof(InfoElementsWindow);
    }

    internal enum TypeInfo {
        None,
        Info,
        Error,
        Warning,
    }
}
