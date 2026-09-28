using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RevitPackageDocumentation.Views.Controls;
/// <summary>
/// Логика взаимодействия для SheetComponentItemControl.xaml
/// </summary>
public partial class SheetComponentItemControl : UserControl {
    public static readonly DependencyProperty DerivedTemplateProperty =
        DependencyProperty.Register(nameof(DerivedTemplate), typeof(DataTemplate),
        typeof(SheetComponentItemControl));

    /// <summary>
    /// Цвет фона карточки компонента, свой для каждого типа компонента листа
    /// </summary>
    public static readonly DependencyProperty AccentBackgroundBrushProperty =
        DependencyProperty.Register(nameof(AccentBackgroundBrush), typeof(Brush),
        typeof(SheetComponentItemControl));

    /// <summary>
    /// Цвет рамки карточки компонента, свой для каждого типа компонента листа
    /// </summary>
    public static readonly DependencyProperty AccentBorderBrushProperty =
        DependencyProperty.Register(nameof(AccentBorderBrush), typeof(Brush),
        typeof(SheetComponentItemControl));

    public DataTemplate DerivedTemplate {
        get => (DataTemplate) GetValue(DerivedTemplateProperty);
        set => SetValue(DerivedTemplateProperty, value);
    }

    public Brush AccentBackgroundBrush {
        get => (Brush) GetValue(AccentBackgroundBrushProperty);
        set => SetValue(AccentBackgroundBrushProperty, value);
    }

    public Brush AccentBorderBrush {
        get => (Brush) GetValue(AccentBorderBrushProperty);
        set => SetValue(AccentBorderBrushProperty, value);
    }

    public SheetComponentItemControl() {
        InitializeComponent();
    }
}
