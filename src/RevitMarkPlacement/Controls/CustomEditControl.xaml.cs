using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace RevitMarkPlacement.Controls;

[ContentProperty(nameof(EditControl))]
public partial class CustomEditControl {
    public static readonly DependencyProperty HeaderTextProperty = DependencyProperty.Register(
        nameof(HeaderText),
        typeof(string),
        typeof(CustomEditControl),
        new PropertyMetadata(default(string)));

    public static readonly DependencyProperty EditControlProperty = DependencyProperty.Register(
        nameof(EditControl),
        typeof(object),
        typeof(CustomEditControl),
        new PropertyMetadata(default(object)));

    public CustomEditControl() {
        InitializeComponent();
    }

    public string HeaderText {
        get => (string) GetValue(HeaderTextProperty);
        set => SetValue(HeaderTextProperty, value);
    }

    public object EditControl {
        get => (object) GetValue(EditControlProperty);
        set => SetValue(EditControlProperty, value);
    }
}
