using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace RevitLintelsManager.Views.Edits;

/// <summary>Переключатель диапазонов на основе реализации RevitRoundingOfAreas.</summary>
public partial class ModeSwitcherControl : UserControl {
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
        nameof(ItemsSource), typeof(IEnumerable), typeof(ModeSwitcherControl), new PropertyMetadata(null));
    public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
        nameof(SelectedItem), typeof(object), typeof(ModeSwitcherControl),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
    public static readonly DependencyProperty DisplayMemberPathProperty = DependencyProperty.Register(
        nameof(DisplayMemberPath), typeof(string), typeof(ModeSwitcherControl), new PropertyMetadata("Name"));
    public static readonly DependencyProperty ItemInvokedCommandProperty = DependencyProperty.Register(
        nameof(ItemInvokedCommand), typeof(ICommand), typeof(ModeSwitcherControl), new PropertyMetadata(null));

    /// <summary>Создаёт переключатель с повторным вызовом уже выбранного диапазона.</summary>
    public ModeSwitcherControl() {
        InitializeComponent();
    }

    public IEnumerable ItemsSource {
        get => (IEnumerable) GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }
    public object SelectedItem {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }
    public string DisplayMemberPath {
        get => (string) GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }
    public ICommand ItemInvokedCommand {
        get => (ICommand) GetValue(ItemInvokedCommandProperty);
        set => SetValue(ItemInvokedCommandProperty, value);
    }

    // Выполняет команду также при повторном нажатии выбранной кнопки, без реакции на начальный Binding.
    private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e) {
        if(e.ChangedButton != MouseButton.Left) {
            return;
        }
        if(ItemsControl.ContainerFromElement(_innerListBox, e.OriginalSource as DependencyObject)
            is not ListBoxItem item) {
            return;
        }
        _innerListBox.SetCurrentValue(Selector.SelectedItemProperty, item.DataContext);
        item.Focus();
        InvokeItem(item.DataContext);
        e.Handled = true;
    }

    // Сохраняет управление диапазонами с клавиатуры, включая повторное подтверждение.
    private void OnPreviewKeyUp(object sender, KeyEventArgs e) {
        if(e.Key is Key.Left or Key.Right or Key.Up or Key.Down or Key.Home or Key.End or Key.Enter or Key.Space) {
            InvokeItem(_innerListBox.SelectedItem);
        }
    }

    // Передаёт выбранную строку команде VM, не выполняя прикладных операций в контроле.
    private void InvokeItem(object item) {
        if(item is not null && ItemInvokedCommand?.CanExecute(item) == true) {
            ItemInvokedCommand.Execute(item);
        }
    }
}
