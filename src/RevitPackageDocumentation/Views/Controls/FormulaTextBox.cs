using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace RevitPackageDocumentation.Views.Controls;

public class FormulaTextBox : TextBox {
    private readonly DispatcherTimer _timer;

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

    public FormulaTextBox() {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _timer.Tick += (s, e) => { _timer.Stop(); UpdateFormula(); };
    }

    protected override void OnTextChanged(TextChangedEventArgs e) {
        _timer.Stop();
        _timer.Start();
    }

    private void UpdateFormula() {
        var propertyName = GetBindingExpression(TextProperty)?.ResolvedSourcePropertyName;
        if(propertyName != null) {
            UpdateFormulaCommand?.Execute(propertyName);
        }
    }
}
