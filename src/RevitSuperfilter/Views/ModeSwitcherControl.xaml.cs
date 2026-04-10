using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace RevitSuperfilter.Views;

public partial class ModeSwitcherControl {
    /// <summary>
    /// Источник данных — список режимов.
    /// </summary>
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(ModeSwitcherControl),
            new PropertyMetadata(null, OnItemsSourceChanged));

    /// <summary>
    /// Выбранный элемент.
    /// </summary>
    public static readonly DependencyProperty SelectedItemProperty =
        DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(ModeSwitcherControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    /// <summary>
    /// Путь к свойству для отображения (по умолчанию "Name").
    /// </summary>
    public static readonly DependencyProperty DisplayMemberPathProperty =
        DependencyProperty.Register(
            nameof(DisplayMemberPath),
            typeof(string),
            typeof(ModeSwitcherControl),
            new PropertyMetadata("Name", OnDisplayMemberPathChanged));

    public ModeSwitcherControl() {
        InitializeComponent();
    }

    /// <summary>
    /// Источник данных — список режимов.
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
    /// Путь к свойству для отображения (по умолчанию "Name").
    /// </summary>
    public string DisplayMemberPath {
        get => (string) GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if(d is ModeSwitcherControl control) {
            control._innerListBox.ItemsSource = (IEnumerable) e.NewValue;
        }
    }

    private static void OnDisplayMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if(d is ModeSwitcherControl control) {
            control._innerListBox.DisplayMemberPath = (string) e.NewValue;
        }
    }
}
