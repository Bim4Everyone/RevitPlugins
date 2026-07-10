using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

using Wpf.Ui.Controls;

namespace RevitAreaBoundaries.Views.Edits;

[DefaultProperty(nameof(EditControl))]
[ContentProperty(nameof(EditControl))]
public partial class CustomGroupEditControl {
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(IconElement), typeof(CustomGroupEditControl), new PropertyMetadata(default(IconElement)));

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header), typeof(string), typeof(CustomGroupEditControl), new PropertyMetadata(default(string)));
    
    public static readonly DependencyProperty CountElementsProperty = DependencyProperty.Register(
        nameof(CountElements), typeof(string), typeof(CustomGroupEditControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty EditControlProperty = DependencyProperty.Register(
        nameof(EditControl), typeof(object), typeof(CustomGroupEditControl), new PropertyMetadata(default(object)));

    public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
        nameof(IsExpanded), typeof(bool), typeof(CustomGroupEditControl), new PropertyMetadata(true));
    
    public static readonly DependencyProperty ButtonCommandSelectProperty = DependencyProperty.Register(
        nameof(ButtonCommandSelect), typeof(ICommand), typeof(CustomGroupEditControl), new PropertyMetadata(default(ICommand)));
    
    public static readonly DependencyProperty ButtonCommandUnSelectProperty = DependencyProperty.Register(
        nameof(ButtonCommandUnSelect), typeof(ICommand), typeof(CustomGroupEditControl), new PropertyMetadata(default(ICommand)));

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
    
    public string CountElements {
        get => (string) GetValue(CountElementsProperty);
        set => SetValue(CountElementsProperty, value);
    }

    public object EditControl {
        get => GetValue(EditControlProperty);
        set => SetValue(EditControlProperty, value);
    }

    public bool IsExpanded {
        get => (bool) GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }
    
    public ICommand ButtonCommandSelect {
        get => (ICommand) GetValue(ButtonCommandSelectProperty);
        set => SetValue(ButtonCommandSelectProperty, value);
    }
    
    public ICommand ButtonCommandUnSelect {
        get => (ICommand) GetValue(ButtonCommandUnSelectProperty);
        set => SetValue(ButtonCommandUnSelectProperty, value);
    }
}
