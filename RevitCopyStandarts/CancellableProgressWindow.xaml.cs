using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace RevitCopyStandarts {
    /// <summary>
    /// Interaction logic for CancellableProgressWindow.xaml
    /// </summary>
    public partial class CancellableProgressWindow : Window {
        private readonly CancellationTokenSource _cancellationTokenSource;

        public CancellableProgressWindow() {
            InitializeComponent();

            _cancellationTokenSource = new CancellationTokenSource();
            Progress = new Progress<string>(p => _operation.Text = p);
        }

        public IProgress<string> Progress { get; }

        public CancellationToken CancellationToken {
            get { return _cancellationTokenSource.Token; }
        }

        private void _btCancel_Click(object sender, RoutedEventArgs e) {
            _btCancel.IsEnabled = false;
            _operation.Text = "Отмена операции ...";
            _cancellationTokenSource.Cancel();
        }
    }
}
