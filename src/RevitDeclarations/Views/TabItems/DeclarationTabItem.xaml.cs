using System.Linq;
using System.Windows.Input;

namespace RevitDeclarations.Views;
public partial class DeclarationTabItem {
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
