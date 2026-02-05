using System.Windows;

namespace RevitSetCoordParams.Views.Edits;

public partial class CategoryControl {

    public static readonly DependencyProperty CategoryNameProperty = DependencyProperty.Register(
        nameof(CategoryName), typeof(string), typeof(CategoryControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty WarningDescriptionProperty = DependencyProperty.Register(
        nameof(WarningDescription), typeof(string), typeof(CategoryControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
        nameof(IsChecked), typeof(bool), typeof(CategoryControl), new PropertyMetadata(true));

    public static readonly DependencyProperty HasWarningProperty = DependencyProperty.Register(
        nameof(HasWarning), typeof(bool), typeof(CategoryControl), new PropertyMetadata(true));


    public CategoryControl() {
        InitializeComponent();
    }

    public string CategoryName {
        get => (string) GetValue(CategoryNameProperty);
        set => SetValue(CategoryNameProperty, value);
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
