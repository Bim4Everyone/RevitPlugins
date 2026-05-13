using System.Windows;
using System.Windows.Controls;

namespace RevitPackageDocumentation.Views.Controls;
/// <summary>
/// Логика взаимодействия для SheetComponentItemControl.xaml
/// </summary>
public partial class SheetComponentItemControl : UserControl {
    public static readonly DependencyProperty DerivedTemplateProperty =
        DependencyProperty.Register(nameof(DerivedTemplate), typeof(DataTemplate),
        typeof(SheetComponentItemControl));

    public DataTemplate DerivedTemplate {
        get => (DataTemplate) GetValue(DerivedTemplateProperty);
        set => SetValue(DerivedTemplateProperty, value);
    }

    public SheetComponentItemControl() {
        InitializeComponent();
    }
}
