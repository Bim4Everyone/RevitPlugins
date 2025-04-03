using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

using Wpf.Ui.Controls;

namespace RevitPlatformSettings.Views.Edits;

[DefaultProperty(nameof(EditControl))]
[ContentProperty(nameof(EditControl))]
public partial class CustomGroupEditControl {
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(IconElement), typeof(CustomGroupEditControl), new PropertyMetadata(default(IconElement)));

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header), typeof(string), typeof(CustomGroupEditControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description), typeof(string), typeof(CustomGroupEditControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty EditControlProperty = DependencyProperty.Register(
        nameof(EditControl), typeof(object), typeof(CustomGroupEditControl), new PropertyMetadata(default(object)));

    public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
        nameof(IsChecked), typeof(bool), typeof(CustomGroupEditControl), new PropertyMetadata(true));
    
    public static readonly DependencyProperty ToogleSwitchVisibleProperty = DependencyProperty.Register(
        nameof(ToogleSwitchVisible), typeof(bool), typeof(CustomGroupEditControl), new PropertyMetadata(false));

    public CustomGroupEditControl() {
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
    
    public bool IsChecked {
        get => (bool) GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }
    
    public bool ToogleSwitchVisible {
        get => (bool) GetValue(ToogleSwitchVisibleProperty);
        set => SetValue(ToogleSwitchVisibleProperty, value);
    } 
}
