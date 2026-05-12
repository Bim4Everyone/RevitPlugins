using System.Windows;
using System.Windows.Controls;

namespace RevitPackageDocumentation.Views;
/// <summary>
/// Логика взаимодействия для BaseSheetComponentView.xaml
/// </summary>
public partial class BaseSheetComponentView : UserControl {
    public static readonly DependencyProperty DerivedTemplateProperty =
        DependencyProperty.Register(nameof(DerivedTemplate), typeof(DataTemplate),
        typeof(BaseSheetComponentView));

    public DataTemplate DerivedTemplate {
        get => (DataTemplate) GetValue(DerivedTemplateProperty);
        set => SetValue(DerivedTemplateProperty, value);
    }

    public BaseSheetComponentView() {
        InitializeComponent();
    }
}
