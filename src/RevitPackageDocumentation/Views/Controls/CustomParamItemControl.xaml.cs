using System.Windows;
using System.Windows.Controls;

namespace RevitPackageDocumentation.Views.Controls;
/// <summary>
/// Логика взаимодействия для CustomParamItemControl.xaml
/// </summary>
public partial class CustomParamItemControl : UserControl {
    public static readonly DependencyProperty TextBoxStyleProperty =
        DependencyProperty.Register(nameof(TextBoxStyle), typeof(Style), typeof(CustomParamItemControl));

    public CustomParamItemControl() {
        InitializeComponent();
    }

    public Style TextBoxStyle {
        get => (Style) GetValue(TextBoxStyleProperty);
        set => SetValue(TextBoxStyleProperty, value);
    }
}
