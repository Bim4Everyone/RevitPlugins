using System.Windows;
using System.Windows.Controls;

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
}
