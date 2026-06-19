using System.Collections;
using System.Windows;
using System.Windows.Controls;

using RevitPackageDocumentation.ViewModels.FiltrationComboBoxVMs;

namespace RevitPackageDocumentation.Views.Controls;
public partial class FiltrationComboBoxControl : UserControl {
    public static readonly DependencyProperty ComboBoxSourceProperty =
        DependencyProperty.Register(nameof(ComboBoxSource), typeof(IEnumerable), typeof(FiltrationComboBoxControl));

    public static readonly DependencyProperty ComboBoxSelectedProperty =
        DependencyProperty.Register(nameof(ComboBoxSelected), typeof(object), typeof(FiltrationComboBoxControl));

    public static readonly DependencyProperty FilterListProperty =
        DependencyProperty.Register(nameof(FilterList), typeof(FiltrationComboBoxFilterListVM), typeof(FiltrationComboBoxControl));


    public FiltrationComboBoxControl() {
        InitializeComponent();
    }

    public IEnumerable ComboBoxSource {
        get => (IEnumerable) GetValue(ComboBoxSourceProperty);
        set => SetValue(ComboBoxSourceProperty, value);
    }

    public object ComboBoxSelected {
        get => (object) GetValue(ComboBoxSelectedProperty);
        set => SetValue(ComboBoxSelectedProperty, value);
    }

    internal FiltrationComboBoxFilterListVM FilterList {
        get => (FiltrationComboBoxFilterListVM) GetValue(FilterListProperty);
        set => SetValue(FilterListProperty, value);
    }

    private void Button_Click(object sender, RoutedEventArgs e) {
        FiltersItemsControl.Visibility = FiltersItemsControl.Visibility == System.Windows.Visibility.Visible
            ? System.Windows.Visibility.Collapsed
            : System.Windows.Visibility.Visible;
        FiltersAddButton.Visibility = FiltersAddButton.Visibility == System.Windows.Visibility.Visible
            ? System.Windows.Visibility.Collapsed
            : System.Windows.Visibility.Visible;
    }

}
