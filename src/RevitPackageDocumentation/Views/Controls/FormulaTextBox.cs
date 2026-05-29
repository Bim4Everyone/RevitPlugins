using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RevitPackageDocumentation.Views.Controls;
public class FormulaTextBox : TextBox {
    public static readonly DependencyProperty UpdateFormulaCommandProperty =
        DependencyProperty.Register(
            nameof(UpdateFormulaCommand),
            typeof(ICommand),
            typeof(FormulaTextBox),
            new PropertyMetadata(null));

    public ICommand UpdateFormulaCommand {
        get => (ICommand) GetValue(UpdateFormulaCommandProperty);
        set => SetValue(UpdateFormulaCommandProperty, value);
    }

    protected override void OnTextChanged(TextChangedEventArgs e) {
        base.OnTextChanged(e);

        if(UpdateFormulaCommand != null) {
            var bindingExpression = GetBindingExpression(TextProperty);
            if(bindingExpression?.ResolvedSourcePropertyName is string propertyName) {
                if(UpdateFormulaCommand.CanExecute(propertyName)) {
                    UpdateFormulaCommand.Execute(propertyName);
                }
            }
        }
    }
}
