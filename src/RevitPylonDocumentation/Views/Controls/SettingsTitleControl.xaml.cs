using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RevitPylonDocumentation.Views.Controls;

public partial class SettingsTitleControl : UserControl {
    public static readonly DependencyProperty LabelTextProperty = DependencyProperty.Register(
        nameof(LabelText), typeof(string), typeof(SettingsTitleControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty ToolTipTextProperty = DependencyProperty.Register(
        nameof(ToolTipText), typeof(string), typeof(SettingsTitleControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty ButtonCommandProperty = DependencyProperty.Register(
        nameof(ButtonCommand), typeof(ICommand), typeof(SettingsTitleControl), new PropertyMetadata(default(ICommand)));

    public static readonly DependencyProperty ButtonMouseLeaveCommandProperty = DependencyProperty.Register(
        nameof(ButtonMouseLeaveCommand), typeof(ICommand), typeof(SettingsTitleControl), new PropertyMetadata(default(ICommand)));

    public SettingsTitleControl() {
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

    public ICommand ButtonCommand {
        get => (ICommand) GetValue(ButtonCommandProperty);
        set => SetValue(ButtonCommandProperty, value);
    }

    public ICommand ButtonMouseLeaveCommand {
        get => (ICommand) GetValue(ButtonMouseLeaveCommandProperty);
        set => SetValue(ButtonMouseLeaveCommandProperty, value);
    }
}
