using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

using Wpf.Ui.Controls;

namespace RevitLintelsManager.Views.Edits;

[DefaultProperty(nameof(EditControl))]
[ContentProperty(nameof(EditControl))]
public partial class CustomGroupEditControl {
    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(IconElement), typeof(CustomGroupEditControl), new PropertyMetadata(default(IconElement)));

    public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
        nameof(Header), typeof(string), typeof(CustomGroupEditControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty HeaderContentProperty = DependencyProperty.Register(
        nameof(HeaderContent), typeof(object), typeof(CustomGroupEditControl), new PropertyMetadata(null));

    public static readonly DependencyProperty ContentPaddingProperty = DependencyProperty.Register(
        nameof(ContentPadding), typeof(Thickness), typeof(CustomGroupEditControl),
        new PropertyMetadata(CardExpander.ContentPaddingProperty.DefaultMetadata.DefaultValue));
    
    public static readonly DependencyProperty EditControlProperty = DependencyProperty.Register(
        nameof(EditControl), typeof(object), typeof(CustomGroupEditControl), new PropertyMetadata(default(object)));

    public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
        nameof(IsExpanded), typeof(bool), typeof(CustomGroupEditControl), new PropertyMetadata(true));

    /// <summary>Создаёт общий раскрывающийся блок настроек или строк менеджера.</summary>
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

    public object HeaderContent {
        get => GetValue(HeaderContentProperty);
        set => SetValue(HeaderContentProperty, value);
    }

    public Thickness ContentPadding {
        get => (Thickness) GetValue(ContentPaddingProperty);
        set => SetValue(ContentPaddingProperty, value);
    }

    public object EditControl {
        get => GetValue(EditControlProperty);
        set => SetValue(EditControlProperty, value);
    }

    public bool IsExpanded {
        get => (bool) GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    // Оставляет строку содержимого для настроек и скрывает её у заголовка плоского списка.
    private void OnCardExpanderLoaded(object sender, RoutedEventArgs e) {
        if(sender is not CardExpander expander || VisualTreeHelper.GetChildrenCount(expander) == 0) {
            return;
        }
        if(VisualTreeHelper.GetChild(expander, 0) is Grid grid && grid.RowDefinitions.Count > 1) {
            // WPF UI 4.0 задаёт Auto для содержимого; спискам нужна ограниченная строка.
            grid.RowDefinitions[1].Height = EditControl is null
                ? new GridLength(0)
                : new GridLength(1, GridUnitType.Star);
        }
    }
}
