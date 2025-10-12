using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;

using Wpf.Ui.Controls;

namespace RevitSetCoordParams.Views.Edits;

[DefaultProperty(nameof(EditControl))]
[ContentProperty(nameof(EditControl))]
public partial class CustomGroupEditControl {
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(IconElement), typeof(CustomGroupEditControl), new PropertyMetadata(default(IconElement)));

    public static readonly DependencyProperty ExtraIconProperty = DependencyProperty.Register(
        nameof(ExtraIcon), typeof(IconElement), typeof(CustomGroupEditControl), new PropertyMetadata(default(IconElement)));

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header), typeof(string), typeof(CustomGroupEditControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description), typeof(string), typeof(CustomGroupEditControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty EditControlProperty = DependencyProperty.Register(
        nameof(EditControl), typeof(object), typeof(CustomGroupEditControl), new PropertyMetadata(default(object)));

    public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
        nameof(IsChecked), typeof(bool), typeof(CustomGroupEditControl), new PropertyMetadata(true));

    public static readonly DependencyProperty ToggleSwitchVisibleProperty = DependencyProperty.Register(
        nameof(ToggleSwitchVisible), typeof(bool), typeof(CustomGroupEditControl), new PropertyMetadata(false));

    public static readonly DependencyProperty ExtraIconVisibleProperty = DependencyProperty.Register(
        nameof(ExtraIconVisible), typeof(bool), typeof(CustomGroupEditControl), new PropertyMetadata(false));

    public static readonly DependencyProperty SwitchOnContentProperty = DependencyProperty.Register(
        nameof(SwitchOnContent), typeof(string), typeof(CustomGroupEditControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty SwitchOffContentProperty = DependencyProperty.Register(
        nameof(SwitchOffContent), typeof(string), typeof(CustomGroupEditControl), new PropertyMetadata(default(string)));


    public CustomGroupEditControl() {
        InitializeComponent();
    }

    public IconElement Icon {
        get => (IconElement) GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public IconElement ExtraIcon {
        get => (IconElement) GetValue(ExtraIconProperty);
        set => SetValue(ExtraIconProperty, value);
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

    public bool ToggleSwitchVisible {
        get => (bool) GetValue(ToggleSwitchVisibleProperty);
        set => SetValue(ToggleSwitchVisibleProperty, value);
    }

    public string SwitchOnContent {
        get => (string) GetValue(SwitchOnContentProperty);
        set => SetValue(SwitchOnContentProperty, value);
    }

    public string SwitchOffContent {
        get => (string) GetValue(SwitchOffContentProperty);
        set => SetValue(SwitchOffContentProperty, value);
    }

    public bool ExtraIconVisible {
        get => (bool) GetValue(ExtraIconVisibleProperty);
        set => SetValue(ExtraIconVisibleProperty, value);
    }
}
