using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

using Autodesk.Revit.DB;

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

    /// <summary>
    /// Коллекция элементов для ComboBox, после того как проведена фильтрация по фильтрам
    /// </summary>
    public ObservableCollection<Element> FilteredItemsSource { get; } = [];

    /// <summary>
    /// Показывает/скрывает видимость фильтров
    /// </summary>
    private void Button_Click(object sender, RoutedEventArgs e) {
        FiltersItemsControl.Visibility = FiltersItemsControl.Visibility == System.Windows.Visibility.Visible
            ? System.Windows.Visibility.Collapsed
            : System.Windows.Visibility.Visible;
        FiltersAddButton.Visibility = FiltersAddButton.Visibility == System.Windows.Visibility.Visible
            ? System.Windows.Visibility.Collapsed
            : System.Windows.Visibility.Visible;
    }

    /// <summary>
    /// Выполняем обновление значений ComboBox по фильтрам после привязки источников
    /// </summary>
    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        var control = (FiltrationComboBoxControl) d;
        control.UpdateFilteredItems();
    }

    /// <summary>
    /// Выполняем обновление значений по фильтрам после того, как пользователь изменил строку фильтра
    /// </summary>
    private void TextBox_TextChanged(object sender, TextChangedEventArgs e) {
        UpdateFilteredItems();
    }

    /// <summary>
    /// Выполняем обновление значений ComboBox по фильтрам после того, как отработала команда удаления фильтра
    /// </summary>
    private void Button_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e) {
        Dispatcher.BeginInvoke(new Action(() => {
            UpdateFilteredItems();
        }), System.Windows.Threading.DispatcherPriority.Background);
    }

    /// <summary>
    /// Обновляет значения ComboBox по фильтрам
    /// </summary>
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

        if(FilteredItemsSource.Count == 1) {
            ComboBoxSelected = FilteredItemsSource.First();
        }
    }

    /// <summary>
    /// Проверяет, что имя элемента содержит все строки фильтра
    /// </summary>
    private bool ItemMatchesAllFilters(Element element, IReadOnlyCollection<string> filters) {
        if(filters.Count == 0) {
            return true;
        }
        string name = element.Name ?? string.Empty;
        return filters.All(name.Contains);
    }
}
