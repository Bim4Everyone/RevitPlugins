using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace RevitRoundingOfAreas.Views.Edits;

public partial class ModeSwitcherControl : UserControl {
    /// <summary>
    /// Источник данных.
    /// </summary>
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(ModeSwitcherControl),
            new PropertyMetadata(null));

    /// <summary>
    /// Выбранный элемент.
    /// </summary>
    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(ModeSwitcherControl),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>
    /// Путь к отображаемому свойству.
    /// </summary>
    public static readonly DependencyProperty DisplayMemberPathProperty =
        DependencyProperty.Register(
            nameof(DisplayMemberPath),
            typeof(string),
            typeof(ModeSwitcherControl),
            new PropertyMetadata("Name"));

    public ModeSwitcherControl() {
        InitializeComponent();
    }

    /// <summary>
    /// Источник данных.
    /// </summary>
    public IEnumerable ItemsSource {
        get => (IEnumerable) GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// Выбранный элемент.
    /// </summary>
    public object SelectedItem {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    /// <summary>
    /// Путь к отображаемому свойству.
    /// </summary>
    public string DisplayMemberPath {
        get => (string) GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }
}
