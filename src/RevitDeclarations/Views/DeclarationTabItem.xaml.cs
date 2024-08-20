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

namespace RevitDeclarations.Views {
    /// <summary>
    /// Interaction logic for DeclarationTabItem.xaml
    /// </summary>
    public partial class DeclarationTabItem : UserControl {
        public DeclarationTabItem() {
            InitializeComponent();
        }

        private void IndentValidation(object sender, TextCompositionEventArgs e) {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
            if(e.Key == Key.Space) {
                e.Handled = true;
            }
        }
    }
}
