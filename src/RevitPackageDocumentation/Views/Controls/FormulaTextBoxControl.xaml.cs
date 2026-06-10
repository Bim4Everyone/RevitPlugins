using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace RevitPackageDocumentation.Views.Controls;

public partial class FormulaTextBoxControl : UserControl {
    private readonly DispatcherTimer _timer;

    public static readonly DependencyProperty PropFormulaProperty =
        DependencyProperty.Register(nameof(PropFormula), typeof(string), typeof(FormulaTextBoxControl));

    public static readonly DependencyProperty PropResultProperty =
        DependencyProperty.Register(nameof(PropResult), typeof(string), typeof(FormulaTextBoxControl),
            new FrameworkPropertyMetadata(OnPropResultChanged));

    public static readonly DependencyProperty UpdateCommandProperty =
        DependencyProperty.Register(nameof(UpdateCommand), typeof(ICommand), typeof(FormulaTextBoxControl));

    public static readonly DependencyProperty TextBoxStyleProperty =
        DependencyProperty.Register(nameof(TextBoxStyle), typeof(Style), typeof(FormulaTextBoxControl));

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

    public Style TextBoxStyle {
        get => (Style) GetValue(TextBoxStyleProperty);
        set => SetValue(TextBoxStyleProperty, value);
    }

    private static void OnPropResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        var control = d as FormulaTextBoxControl;
        control?.UpdateResultVisibility();
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
        string propertyName = GetBindingExpression(PropFormulaProperty)?.ResolvedSourcePropertyName;
        if(propertyName != null) {
            UpdateCommand?.Execute(propertyName);
        }
        UpdateResultVisibility();
    }

    private void UpdateResultVisibility() {
        bool areEqual = string.Equals(PropFormula ?? string.Empty, PropResult ?? string.Empty);
        ResultTextBlock.Visibility = areEqual ? Visibility.Collapsed : Visibility.Visible;
    }
}
