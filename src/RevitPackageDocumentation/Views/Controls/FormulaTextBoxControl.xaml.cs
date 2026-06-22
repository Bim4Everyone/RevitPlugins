using System;
using System.ComponentModel;
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

    public static readonly DependencyProperty AcceptsReturnProperty =
        DependencyProperty.Register(nameof(AcceptsReturn), typeof(bool), typeof(FormulaTextBoxControl),
            new FrameworkPropertyMetadata(false));


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

    public bool AcceptsReturn {
        get => (bool) GetValue(AcceptsReturnProperty);
        set => SetValue(AcceptsReturnProperty, value);
    }

    /// <summary>
    /// Событие изменения текста TextBox
    /// </summary>
    public static readonly RoutedEvent TextBoxTextChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(TextBoxTextChanged),
            RoutingStrategy.Bubble,
            typeof(TextChangedEventHandler),
            typeof(FormulaTextBoxControl));

    /// <summary>
    /// Событие изменения текста формулы TextBox
    /// </summary>
    public event TextChangedEventHandler TextBoxTextChanged {
        add => AddHandler(TextBoxTextChangedEvent, value);
        remove => RemoveHandler(TextBoxTextChangedEvent, value);
    }

    /// <summary>
    /// Событие изменения текста TextBlock
    /// </summary>
    public static readonly RoutedEvent TextBlockTextChangedEvent =
        EventManager.RegisterRoutedEvent(
            nameof(TextBlockTextChanged),
            RoutingStrategy.Bubble,
            typeof(TextChangedEventHandler),
            typeof(FormulaTextBoxControl));

    /// <summary>
    /// Событие изменения текста значения TextBlock
    /// </summary>
    public event TextChangedEventHandler TextBlockTextChanged {
        add => AddHandler(TextBlockTextChangedEvent, value);
        remove => RemoveHandler(TextBlockTextChangedEvent, value);
    }

    public FormulaTextBoxControl() {
        InitializeComponent();

        // Когда меняется формула
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
        _timer.Tick += (s, e) => {
            _timer.Stop();
            UpdateFormula();
            UpdateResultVisibility();
            OnFormulaInputTextChanged(this, EventArgs.Empty);
            OnResultTextChanged(this, EventArgs.Empty);
        };
        FormulaInputTextBox.TextChanged += OnTextChanged;

        // Когда меняется значение формулы
        // Значение формулы может меняться, когда меняется формула и когда меняются переменные, указанные в формуле
        var textBlockDescriptor = DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty, typeof(TextBlock));
        textBlockDescriptor.AddValueChanged(ResultTextBlock, OnResultTextChanged);
    }

    /// <summary>
    /// При инициализации
    /// </summary>
    private static void OnPropResultChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        var control = d as FormulaTextBoxControl;
        control?.UpdateResultVisibility();
    }

    private void OnTextChanged(object sender, TextChangedEventArgs e) {
        _timer.Stop();
        _timer.Start();
    }

    /// <summary>
    /// Запускает команду, что текст формулы изменился
    /// </summary>
    private void UpdateFormula() {
        string propertyName = GetBindingExpression(PropFormulaProperty)?.ResolvedSourcePropertyName;
        if(propertyName != null) {
            UpdateCommand?.Execute(propertyName);
        }
    }

    private void UpdateResultVisibility() {
        bool areEqual = string.Equals(PropFormula ?? string.Empty, PropResult ?? string.Empty);
        ResultTextBlock.Visibility = areEqual ? Visibility.Collapsed : Visibility.Visible;
    }

    private void OnFormulaInputTextChanged(object sender, EventArgs e) {
        RaiseEvent(new TextChangedEventArgs(TextBoxTextChangedEvent, UndoAction.None));
    }

    private void OnResultTextChanged(object sender, EventArgs e) {
        RaiseEvent(new TextChangedEventArgs(TextBlockTextChangedEvent, UndoAction.None));
    }
}
