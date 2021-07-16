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

namespace RevitServerFolders.Export {
    /// <summary>
    /// Interaction logic for SelectFileControl.xaml
    /// </summary>
    public partial class SelectFileView : UserControl {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(SelectFileView), new PropertyMetadata(null));
        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register(nameof(FilePath), typeof(string), typeof(SelectFileView), new PropertyMetadata(null));
        public static readonly DependencyProperty IsReadOnlyFilePathProperty = DependencyProperty.Register(nameof(IsReadOnlyFilePath), typeof(bool), typeof(SelectFileView), new PropertyMetadata(true));

        public SelectFileView() {
            InitializeComponent();
        }

        public ICommand Command {
            get { return (ICommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public string FilePath {
            get { return (string) GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }

        public bool IsReadOnlyFilePath {
            get { return (bool) GetValue(IsReadOnlyFilePathProperty); }
            set { SetValue(IsReadOnlyFilePathProperty, value); }
        }
    }
}
