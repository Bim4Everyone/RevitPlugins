using System.Windows;

namespace RevitBuildCoordVolumes.Views.Edits;

public partial class SlabControl {

    public static readonly DependencyProperty SlabNameProperty = DependencyProperty.Register(
        nameof(SlabName), typeof(string), typeof(SlabControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty WarningDescriptionProperty = DependencyProperty.Register(
        nameof(WarningDescription), typeof(string), typeof(SlabControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
        nameof(IsChecked), typeof(bool), typeof(SlabControl), new PropertyMetadata(true));

    public static readonly DependencyProperty HasWarningProperty = DependencyProperty.Register(
        nameof(HasWarning), typeof(bool), typeof(SlabControl), new PropertyMetadata(true));


    public SlabControl() {
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
