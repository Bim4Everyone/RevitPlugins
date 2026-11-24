using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

using RevitLintelsManager.Models.Configs;

using Wpf.Ui.Controls;

namespace RevitLintelsManager.Views.Edits;

[DefaultProperty(nameof(EditControl))]
[ContentProperty(nameof(EditControl))]
[TemplatePart(Name = SystemPluginConfig.CustomGroupExpanderPartName, Type = typeof(CardExpander))]
public class CustomGroupEditControl : Control {
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

    private CardExpander _expander;

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

    /// <summary>Подключает раскрывающуюся карточку из шаблона общего блока.</summary>
    public override void OnApplyTemplate() {
        if(_expander is not null) {
            _expander.Loaded -= OnCardExpanderLoaded;
        }
        base.OnApplyTemplate();
        _expander = GetTemplateChild(SystemPluginConfig.CustomGroupExpanderPartName) as CardExpander;
        if(_expander is not null) {
            _expander.Loaded += OnCardExpanderLoaded;
        }
    }

    // Оставляет строку содержимого для настроек и скрывает её у заголовка плоского списка.
    private void OnCardExpanderLoaded(object sender, RoutedEventArgs e) {
        if(sender is not CardExpander expander) {
            return;
        }
        if(BorderBrush is not null) {
            expander.SetBinding(BorderBrushProperty, new Binding(nameof(BorderBrush)) { Source = this });
        }
        if(VisualTreeHelper.GetChildrenCount(expander) > 0
            && VisualTreeHelper.GetChild(expander, 0) is Grid grid && grid.RowDefinitions.Count > 1) {
            // WPF UI 4.0 задаёт Auto для содержимого; спискам нужна ограниченная строка.
            grid.RowDefinitions[1].Height = EditControl is null
                ? new GridLength(0)
                : new GridLength(1, GridUnitType.Star);
        }
    }
}
