using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RevitPylonDocumentation.Views.Controls;

public partial class ComboBoxSettingControl : UserControl {
    public static readonly DependencyProperty LabelTextProperty = DependencyProperty.Register(
        nameof(LabelText), typeof(string), typeof(ComboBoxSettingControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty ToolTipTextProperty = DependencyProperty.Register(
        nameof(ToolTipText), typeof(string), typeof(ComboBoxSettingControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource), typeof(IEnumerable), typeof(ComboBoxSettingControl), new PropertyMetadata(default(IEnumerable)));

    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem), typeof(object), typeof(ComboBoxSettingControl), new PropertyMetadata(default(object)));

    public static readonly DependencyProperty SelectionChangedCommandProperty = DependencyProperty.Register(
        nameof(SelectionChangedCommand), typeof(ICommand), typeof(ComboBoxSettingControl), new PropertyMetadata(default(ICommand)));

    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
        nameof(ItemTemplate), typeof(DataTemplate), typeof(ComboBoxSettingControl), new PropertyMetadata(default(DataTemplate)));

    public ComboBoxSettingControl() {
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

    public IEnumerable ItemsSource {
        get => (IEnumerable) GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public object SelectedItem {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public ICommand SelectionChangedCommand {
        get => (ICommand) GetValue(SelectionChangedCommandProperty);
        set => SetValue(SelectionChangedCommandProperty, value);
    }

    public DataTemplate ItemTemplate {
        get => (DataTemplate) GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    private void Validation_Error(object sender, ValidationErrorEventArgs e) {
        if(e.Action == ValidationErrorEventAction.Added) {
            GeneralComboBox.Style = (Style) FindResource("ErrorComboBoxStyle");
        } else if(e.Action == ValidationErrorEventAction.Removed) {
            GeneralComboBox.Style = (Style) FindResource("ComboBoxWithTooltipStyle");
        }
    }
}
