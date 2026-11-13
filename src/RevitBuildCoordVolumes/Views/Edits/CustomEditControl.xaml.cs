using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;

using Wpf.Ui.Controls;

namespace RevitBuildCoordVolumes.Views.Edits;

[DefaultProperty(nameof(EditControl))]
[ContentProperty(nameof(EditControl))]
public partial class CustomEditControl {
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(IconElement), typeof(CustomEditControl), new PropertyMetadata(default(IconElement)));

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header), typeof(string), typeof(CustomEditControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description), typeof(string), typeof(CustomEditControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty EditControlProperty = DependencyProperty.Register(
        nameof(EditControl), typeof(object), typeof(CustomEditControl), new PropertyMetadata(default(object)));

    public CustomEditControl() {
        InitializeComponent();
    }

    public IconElement Icon {
        get => (IconElement) GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Header {
        get => (string) GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public string Description {
        get => (string) GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public object EditControl {
        get => GetValue(EditControlProperty);
        set => SetValue(EditControlProperty, value);
    }
}
