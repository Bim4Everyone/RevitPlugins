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

namespace RevitServerFolders {
    /// <summary>
    /// Interaction logic for FilePathEditView.xaml
    /// </summary>
    public partial class FilePathEditView : UserControl {
        public static readonly DependencyProperty LabelNameProperty = DependencyProperty.Register(nameof(LabelName), typeof(string), typeof(FilePathEditView), new PropertyMetadata("Header"));
        public static readonly DependencyProperty CheckNameProperty = DependencyProperty.Register(nameof(CheckName), typeof(string), typeof(FilePathEditView), new PropertyMetadata("Check"));
        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(nameof(IsChecked), typeof(bool), typeof(FilePathEditView), new PropertyMetadata(false));
        public static readonly DependencyProperty IsCheckedEnabledProperty = DependencyProperty.Register(nameof(IsCheckedEnabled), typeof(bool), typeof(FilePathEditView), new PropertyMetadata(true));

        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(FilePathEditView), new PropertyMetadata(null));
        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register(nameof(FilePath), typeof(string), typeof(FilePathEditView), new PropertyMetadata("File"));
        public static readonly DependencyProperty IsReadOnlyFilePathProperty = DependencyProperty.Register(nameof(IsReadOnlyFilePath), typeof(bool), typeof(FilePathEditView), new PropertyMetadata(true));
        public static readonly DependencyProperty IsEnabledFilePathProperty = DependencyProperty.Register(nameof(IsEnabledFilePath), typeof(bool), typeof(FilePathEditView), new PropertyMetadata(true));

        public FilePathEditView() {
            InitializeComponent();
        }

        public string LabelName {
            get { return (string) GetValue(LabelNameProperty); }
            set { SetValue(LabelNameProperty, value); }
        }

        public string CheckName {
            get { return (string) GetValue(CheckNameProperty); }
            set { SetValue(CheckNameProperty, value); }
        }

        public bool IsChecked {
            get { return (bool) GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        public bool IsCheckedEnabled {
            get { return (bool) GetValue(IsCheckedEnabledProperty); }
            set { SetValue(IsCheckedEnabledProperty, value); }
        }

        public ICommand Command {
            get { return (ICommand) GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public string FilePath {
            get { return (string) GetValue(FilePathProperty); }
            set { SetValue(FilePathProperty, value); }
        }

        public bool IsEnabledFilePath {
            get { return (bool) GetValue(IsEnabledFilePathProperty); }
            set { SetValue(IsEnabledFilePathProperty, value); }
        }

        public bool IsReadOnlyFilePath {
            get { return (bool) GetValue(IsReadOnlyFilePathProperty); }
            set { SetValue(IsReadOnlyFilePathProperty, value); }
        }
    }
}
