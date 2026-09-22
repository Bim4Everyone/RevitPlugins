using System.ComponentModel;
using System.Windows;
using System.Windows.Markup;

using Wpf.Ui.Controls;

namespace RevitEnumerateBySpline.Views.Controls;

[DefaultProperty(nameof(HeaderEditControl))]
[ContentProperty(nameof(HeaderEditControl))]
public partial class LabeledControl {
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(IconElement), typeof(LabeledControl), new PropertyMetadata(default(IconElement)));

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header), typeof(string), typeof(LabeledControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description), typeof(string), typeof(LabeledControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty HeaderEditControlProperty = DependencyProperty.Register(
        nameof(HeaderEditControl), typeof(object), typeof(LabeledControl), new PropertyMetadata(default(object)));
    
    public static readonly DependencyProperty MainEditControlProperty = DependencyProperty.Register(
        nameof(MainEditControl), typeof(object), typeof(LabeledControl), new PropertyMetadata(default(object)));
    
    public static readonly DependencyProperty MainEditControlVisibleProperty = DependencyProperty.Register(
        nameof(MainEditControlVisible), typeof(bool), typeof(LabeledControl), new PropertyMetadata(false));
    
    public static readonly DependencyProperty HeaderEditControlBorderVisibleProperty = DependencyProperty.Register(
        nameof(HeaderEditControlBorderVisible), typeof(bool), typeof(LabeledControl), new PropertyMetadata(true));

    public LabeledControl() {
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

    public object HeaderEditControl {
        get => GetValue(HeaderEditControlProperty);
        set => SetValue(HeaderEditControlProperty, value);
    }
    
    public object MainEditControl {
        get => GetValue(MainEditControlProperty);
        set => SetValue(MainEditControlProperty, value);
    }
    
    public bool MainEditControlVisible {
        get => (bool)GetValue(MainEditControlVisibleProperty);
        set => SetValue(MainEditControlVisibleProperty, value);
    }
    
    public bool HeaderEditControlBorderVisible {
        get => (bool)GetValue(HeaderEditControlBorderVisibleProperty);
        set => SetValue(HeaderEditControlBorderVisibleProperty, value);
    }
}
