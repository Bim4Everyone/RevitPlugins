using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace RevitPlatformSettings.Views.Edits;

public partial class HyperLinkEditWithImage {
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(ImageSource), typeof(HyperLinkEditWithImage),
        new PropertyMetadata(default(ImageSource)));

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header), typeof(object), typeof(HyperLinkEditWithImage), new PropertyMetadata(default(object)));

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description), typeof(string), typeof(HyperLinkEditWithImage), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty NavigationUrlProperty = DependencyProperty.Register(
        nameof(NavigationUrl), typeof(string), typeof(HyperLinkEditWithImage),
        new PropertyMetadata(default(string)));

    public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
        nameof(Command), typeof(ICommand), typeof(HyperLinkEditWithImage), new PropertyMetadata(default(ICommand)));

    public HyperLinkEditWithImage() {
        InitializeComponent();
    }

    public ICommand Command {
        get => (ICommand) GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
    
    public ImageSource Icon {
        get => (ImageSource) GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
    
    public object Header {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }
    
    public string Description {
        get => (string) GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public string NavigationUrl {
        get => (string) GetValue(NavigationUrlProperty);
        set => SetValue(NavigationUrlProperty, value);
    }

    private void Hyperlink_OnClick(object sender, RoutedEventArgs e) {
        if(!string.IsNullOrEmpty(NavigationUrl)) {
            Process.Start(NavigationUrl);
        }
    }
}
