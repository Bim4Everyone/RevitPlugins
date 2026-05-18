using System.Windows;
using System.Windows.Controls;

namespace RevitPackageDocumentation.Views.Controls;
/// <summary>
/// Логика взаимодействия для ParamItemControl.xaml
/// </summary>
public partial class ParamItemControl : UserControl {
    public static readonly DependencyProperty DerivedTemplateProperty =
        DependencyProperty.Register(nameof(DerivedTemplate), typeof(DataTemplate), typeof(ParamItemControl));

    public DataTemplate DerivedTemplate {
        get => (DataTemplate) GetValue(DerivedTemplateProperty);
        set => SetValue(DerivedTemplateProperty, value);
    }

    public ParamItemControl() {
        InitializeComponent();
    }
}
