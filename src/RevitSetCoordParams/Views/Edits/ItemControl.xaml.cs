using System.Windows;

namespace RevitSetCoordParams.Views.Edits;

public partial class ItemControl {

    public static readonly DependencyProperty SlabNameProperty = DependencyProperty.Register(
        nameof(SlabName), typeof(string), typeof(ItemControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty WarningDescriptionProperty = DependencyProperty.Register(
        nameof(WarningDescription), typeof(string), typeof(ItemControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
        nameof(IsChecked), typeof(bool), typeof(ItemControl), new PropertyMetadata(true));

    public static readonly DependencyProperty HasWarningProperty = DependencyProperty.Register(
        nameof(HasWarning), typeof(bool), typeof(ItemControl), new PropertyMetadata(true));


    public ItemControl() {
        InitializeComponent();
    }

    public string SlabName {
        get => (string) GetValue(SlabNameProperty);
        set => SetValue(SlabNameProperty, value);
    }

    public string WarningDescription {
        get => (string) GetValue(WarningDescriptionProperty);
        set => SetValue(WarningDescriptionProperty, value);
    }

    public bool IsChecked {
        get => (bool) GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public bool HasWarning {
        get => (bool) GetValue(HasWarningProperty);
        set => SetValue(HasWarningProperty, value);
    }
}
