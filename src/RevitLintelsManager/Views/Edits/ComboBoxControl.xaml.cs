using System.Collections;
using System.Windows;

namespace RevitLintelsManager.Views.Edits;

public partial class ComboBoxControl {

    public static readonly DependencyProperty WidthTextBlockProperty = DependencyProperty.Register(
        nameof(WidthTextBlock), typeof(double), typeof(ComboBoxControl), new PropertyMetadata(0d));
    
    public static readonly DependencyProperty TextTextBlockProperty = DependencyProperty.Register(
        nameof(TextTextBlock), typeof(string), typeof(ComboBoxControl), new PropertyMetadata(default(string)));
    
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
            nameof(ItemsSource), typeof(IEnumerable), typeof(ComboBoxControl), new PropertyMetadata(null));

    public static readonly DependencyProperty SelectedValueProperty = DependencyProperty.Register(
            nameof(SelectedValue), typeof(object), typeof(ComboBoxControl), 
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public ComboBoxControl() {
        InitializeComponent();
    }
    
    public double WidthTextBlock {
        get => (double) GetValue(WidthTextBlockProperty);
        set => SetValue(WidthTextBlockProperty, value);
    }

    public string TextTextBlock {
        get => (string) GetValue(TextTextBlockProperty);
        set => SetValue(TextTextBlockProperty, value);
    }
    
    public IEnumerable ItemsSource {
        get => (IEnumerable)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public object SelectedValue {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    } 
}
