using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RevitPylonDocumentation.Views.Controls;

public partial class DoubleTextBoxSettingControl : UserControl {
    public static readonly DependencyProperty LabelTextProperty = DependencyProperty.Register(
        nameof(LabelText), typeof(string), typeof(DoubleTextBoxSettingControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty ToolTipTextProperty = DependencyProperty.Register(
        nameof(ToolTipText), typeof(string), typeof(DoubleTextBoxSettingControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty FirstTextBoxTextProperty = DependencyProperty.Register(
        nameof(FirstTextBoxText), typeof(string), typeof(DoubleTextBoxSettingControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty SecondTextBoxTextProperty = DependencyProperty.Register(
        nameof(SecondTextBoxText), typeof(string), typeof(DoubleTextBoxSettingControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty TextChangedCommandProperty = DependencyProperty.Register(
        nameof(TextChangedCommand), typeof(ICommand), typeof(DoubleTextBoxSettingControl), new PropertyMetadata(default(ICommand)));

    public DoubleTextBoxSettingControl() {
        InitializeComponent();
    }

    public string LabelText {
        get => (string) GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    public string ToolTipText {
        get => (string) GetValue(ToolTipTextProperty);
        set => SetValue(ToolTipTextProperty, value);
    }

    public string FirstTextBoxText {
        get => (string) GetValue(FirstTextBoxTextProperty);
        set => SetValue(FirstTextBoxTextProperty, value);
    }

    public string SecondTextBoxText {
        get => (string) GetValue(SecondTextBoxTextProperty);
        set => SetValue(SecondTextBoxTextProperty, value);
    }

    public ICommand TextChangedCommand {
        get => (ICommand) GetValue(TextChangedCommandProperty);
        set => SetValue(TextChangedCommandProperty, value);
    }
}
