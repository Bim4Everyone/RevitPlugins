using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RevitPackageDocumentation.Views.Controls;

public partial class ParamItemControl : UserControl {
    public static readonly DependencyProperty DerivedTemplateProperty =
        DependencyProperty.Register(nameof(DerivedTemplate), typeof(DataTemplate), typeof(ParamItemControl));

    public ParamItemControl() {
        InitializeComponent();
    }

    public DataTemplate DerivedTemplate {
        get => (DataTemplate) GetValue(DerivedTemplateProperty);
        set => SetValue(DerivedTemplateProperty, value);
    }

    private void EditMenuItem_Click(object sender, RoutedEventArgs e) {
        // Переключаемся в режим редактирования
        DisplayTextBlock.Visibility = Visibility.Collapsed;
        EditTextBox.Visibility = Visibility.Visible;

        EditTextBox.Focus();
        EditTextBox.SelectAll();
    }

    private void EditTextBox_LostFocus(object sender, RoutedEventArgs e) {
        DisplayTextBlock.Visibility = Visibility.Visible;
        EditTextBox.Visibility = Visibility.Collapsed;
    }

    private void EditTextBox_KeyDown(object sender, KeyEventArgs e) {
        if(e.Key == Key.Enter || e.Key == Key.Escape) {
            DisplayTextBlock.Visibility = Visibility.Visible;
            EditTextBox.Visibility = Visibility.Collapsed;
            e.Handled = true;
        }
    }
}
