using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RevitPylonDocumentation.Views.Controls;

public partial class TextBoxSettingControl : UserControl {
    public static readonly DependencyProperty LabelTextProperty = DependencyProperty.Register(
        nameof(LabelText), typeof(string), typeof(TextBoxSettingControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty ToolTipTextProperty = DependencyProperty.Register(
        nameof(ToolTipText), typeof(string), typeof(TextBoxSettingControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty TextBoxTextProperty = DependencyProperty.Register(
        nameof(TextBoxText), typeof(string), typeof(TextBoxSettingControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty TextChangedCommandProperty = DependencyProperty.Register(
        nameof(TextChangedCommand), typeof(ICommand), typeof(TextBoxSettingControl), new PropertyMetadata(default(ICommand)));

    public TextBoxSettingControl() {
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

    public string TextBoxText {
        get => (string) GetValue(TextBoxTextProperty);
        set => SetValue(TextBoxTextProperty, value);
    }

    public ICommand TextChangedCommand {
        get => (ICommand) GetValue(TextChangedCommandProperty);
        set => SetValue(TextChangedCommandProperty, value);
    }

    private void Validation_Error(object sender, ValidationErrorEventArgs e) {
        if(e.Action == ValidationErrorEventAction.Added) {
            GeneralTextBox.Style = (Style) FindResource("ErrorTextBoxStyle");
        } else if(e.Action == ValidationErrorEventAction.Removed) {
            GeneralTextBox.Style = (Style) FindResource("TextBoxWithTooltipStyle");
        }
    }
}
