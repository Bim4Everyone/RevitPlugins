using System.Windows;

namespace RevitLintelsManager.Views.Edits;

public partial class ItemControl {

    public static readonly DependencyProperty ItemNameProperty = DependencyProperty.Register(
        nameof(ItemName), typeof(string), typeof(ItemControl), new PropertyMetadata(default(string)));

    public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
        nameof(IsChecked), typeof(bool), typeof(ItemControl), new PropertyMetadata(true));


    public ItemControl() {
        InitializeComponent();
    }

    public string ItemName {
        get => (string) GetValue(ItemNameProperty);
        set => SetValue(ItemNameProperty, value);
    }

    public bool IsChecked {
        get => (bool) GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }
}
