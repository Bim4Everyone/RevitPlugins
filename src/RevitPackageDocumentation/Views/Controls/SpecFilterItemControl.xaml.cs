using System.Windows;
using System.Windows.Controls;

namespace RevitPackageDocumentation.Views.Controls;
/// <summary>
/// Логика взаимодействия для SpecFilterItemControl.xaml
/// </summary>
public partial class SpecFilterItemControl : UserControl {
    public static readonly DependencyProperty ComboBoxStyleProperty =
        DependencyProperty.Register(nameof(ComboBoxStyle), typeof(Style), typeof(SpecFilterItemControl));

    public SpecFilterItemControl() {
        InitializeComponent();
    }

    public Style ComboBoxStyle {
        get => (Style) GetValue(ComboBoxStyleProperty);
        set => SetValue(ComboBoxStyleProperty, value);
    }
}
