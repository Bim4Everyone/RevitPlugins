using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitPackageDocumentation.ViewModels.FiltrationComboBoxVMs;

namespace RevitPackageDocumentation.Views.Controls;
public partial class FiltrationComboBoxControl : UserControl {
    public static readonly DependencyProperty ComboBoxSourceProperty =
        DependencyProperty.Register(nameof(ComboBoxSource), typeof(IEnumerable), typeof(FiltrationComboBoxControl),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public static readonly DependencyProperty ComboBoxSelectedProperty =
        DependencyProperty.Register(nameof(ComboBoxSelected), typeof(Element), typeof(FiltrationComboBoxControl));

    public static readonly DependencyProperty FilterListProperty =
        DependencyProperty.Register(nameof(FilterList), typeof(FiltrationComboBoxFilterListVM), typeof(FiltrationComboBoxControl),
            new PropertyMetadata(null, OnItemsSourceChanged));

    public FiltrationComboBoxControl() {
        InitializeComponent();
    }

    public IEnumerable ComboBoxSource {
        get => (IEnumerable) GetValue(ComboBoxSourceProperty);
        set => SetValue(ComboBoxSourceProperty, value);
    }

    public Element ComboBoxSelected {
        get => (Element) GetValue(ComboBoxSelectedProperty);
        set => SetValue(ComboBoxSelectedProperty, value);
    }

    internal FiltrationComboBoxFilterListVM FilterList {
        get => (FiltrationComboBoxFilterListVM) GetValue(FilterListProperty);
        set => SetValue(FilterListProperty, value);
    }

    public ObservableCollection<Element> FilteredItemsSource { get; } = [];

    private void Button_Click(object sender, RoutedEventArgs e) {
        FiltersItemsControl.Visibility = FiltersItemsControl.Visibility == System.Windows.Visibility.Visible
            ? System.Windows.Visibility.Collapsed
            : System.Windows.Visibility.Visible;
        FiltersAddButton.Visibility = FiltersAddButton.Visibility == System.Windows.Visibility.Visible
            ? System.Windows.Visibility.Collapsed
            : System.Windows.Visibility.Visible;
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        var control = (FiltrationComboBoxControl) d;
        control.UpdateFilteredItems();
    }


    private void UpdateFilteredItems() {
        FilteredItemsSource.Clear();

        if(ComboBoxSource is null || FilterList is null) {
            return;
        }

        var filters = FilterList.ValueList
            .Select(x => x?.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        foreach(var element in ComboBoxSource.OfType<Element>()) {
            if(ItemMatchesAllFilters(element, filters)) {
                FilteredItemsSource.Add(element);
            }
        }
    }

    private bool ItemMatchesAllFilters(Element element, IReadOnlyCollection<string> filters) {
        if(filters.Count == 0) {
            return true;
        }

        string name = element.Name ?? string.Empty;
        return filters.All(name.Contains);
    }


    private void DeleteButton_Click(object sender, RoutedEventArgs e) {
        UpdateFilteredItems();
        TaskDialog.Show("e", FilterList.ValueList.Count().ToString());
    }

    private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
        UpdateFilteredItems();
    }

    private void Button_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
        // Проверяем, что команда уже отработала (можно использовать флаг)
        // или просто обновляем список с задержкой
        Dispatcher.BeginInvoke(new Action(() => {
            UpdateFilteredItems();
            TaskDialog.Show("e", FilterList.ValueList.Count().ToString());
        }), System.Windows.Threading.DispatcherPriority.Background);
    }

    private void Button_Click_1(object sender, RoutedEventArgs e) {
        UpdateFilteredItems();
        TaskDialog.Show("e", FilterList.ValueList.Count().ToString());
    }
}
