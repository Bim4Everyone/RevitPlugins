using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace RevitPackageDocumentation.Views.Controls;

/// <summary>
/// Логика взаимодействия для FormulaTextBoxControl.xaml
/// </summary>
public partial class FormulaTextBoxControl : UserControl {
    private readonly DispatcherTimer _timer;

    public static readonly DependencyProperty PropFormulaProperty =
        DependencyProperty.Register(nameof(PropFormula), typeof(string), typeof(FormulaTextBoxControl));

    public static readonly DependencyProperty PropResultProperty =
        DependencyProperty.Register(nameof(PropResult), typeof(string), typeof(FormulaTextBoxControl));

    public static readonly DependencyProperty UpdateCommandProperty =
        DependencyProperty.Register(nameof(UpdateCommand), typeof(ICommand), typeof(FormulaTextBoxControl));

    public string PropFormula {
        get => (string) GetValue(PropFormulaProperty);
        set => SetValue(PropFormulaProperty, value);
    }

    public string PropResult {
        get => (string) GetValue(PropResultProperty);
        set => SetValue(PropResultProperty, value);
    }

    public ICommand UpdateCommand {
        get => (ICommand) GetValue(UpdateCommandProperty);
        set => SetValue(UpdateCommandProperty, value);
    }

    public FormulaTextBoxControl() {
        InitializeComponent();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _timer.Tick += (s, e) => { _timer.Stop(); UpdateFormula(); };

        FormulaInputTextBox.TextChanged += OnTextChanged;
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e) {
        _timer.Stop();
        _timer.Start();
    }

    private void UpdateFormula() {
        var propertyName = GetBindingExpression(PropFormulaProperty)?.ResolvedSourcePropertyName;
        if(propertyName != null) {
            UpdateCommand?.Execute(propertyName);
        }
    }
}
