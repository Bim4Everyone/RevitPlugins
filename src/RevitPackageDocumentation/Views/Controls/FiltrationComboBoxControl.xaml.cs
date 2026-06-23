using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

using Autodesk.Revit.DB;

using RevitPackageDocumentation.ViewModels.FiltrationComboBoxVMs;

namespace RevitPackageDocumentation.Views.Controls;
public partial class FiltrationComboBoxControl : UserControl {
    public static readonly DependencyProperty ComboBoxSourceProperty =
        DependencyProperty.Register(nameof(ComboBoxSource), typeof(IEnumerable), typeof(FiltrationComboBoxControl),
            new PropertyMetadata(null, OnComboBoxSourceChanged));

    public static readonly DependencyProperty ComboBoxSelectedProperty =
        DependencyProperty.Register(nameof(ComboBoxSelected), typeof(Element), typeof(FiltrationComboBoxControl));

    public static readonly DependencyProperty FilterListProperty =
        DependencyProperty.Register(nameof(FilterList), typeof(FiltrationComboBoxFilterListVM), typeof(FiltrationComboBoxControl),
            new PropertyMetadata(null, OnFilterListSourceChanged));

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
    /// Выполняем обновление значений ComboBox по фильтрам после привязки источника
    /// </summary>
    private static void OnComboBoxSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        var control = (FiltrationComboBoxControl) d;
        control.UpdateFilteredItems();
    }

    /// <summary>
    /// Выполняем обновление значений FilterList после привязки источника
    /// Срабатывает, когда меняется (назначается впервые) ссылка на коллекцию
    /// </summary>
    private static void OnFilterListSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        var control = (FiltrationComboBoxControl) d;
        control.UpdateFilteredItems();
        control.SubscribeToFilterList();
    }

    /// <summary>
    /// Подписываемся на изменения списка фильтров - добавление фильтра, удаление фильтра, изменения значения фильтра
    /// </summary>
    private void SubscribeToFilterList() {
        if(FilterList?.ValueList == null) { return; }

        // Подписываемся на изменения коллекции - добавления и удаления
        FilterList.ValueList.CollectionChanged += OnValueListCollectionChanged;

        // Подписываемся на изменения значения каждого фильтра
        foreach(var item in FilterList.ValueList) {
            SubscribeToFilterItem(item);
        }
    }

    /// <summary>
    /// Получаем фильтры и перезапускаем фильтрацию при добавлении и удалении фильтров
    /// </summary>
    private void OnValueListCollectionChanged(
        object sender,
        System.Collections.Specialized.NotifyCollectionChangedEventArgs e) {

        // Обработка добавленных элементов
        if(e.NewItems != null) {
            foreach(FiltrationComboBoxFilterVM newItem in e.NewItems) {
                SubscribeToFilterItem(newItem);
            }
        }

        // Обработка удаленных элементов
        if(e.OldItems != null) {
            foreach(FiltrationComboBoxFilterVM oldItem in e.OldItems) {
                UnsubscribeFromFilterItem(oldItem);
            }
        }

        // Обновляем фильтры при изменении коллекции
        UpdateFilteredItems();
    }

    private void SubscribeToFilterItem(FiltrationComboBoxFilterVM item) {
        if(item is INotifyPropertyChanged notifyItem) {
            notifyItem.PropertyChanged += OnFilterItemPropertyChanged;
        }
    }

    private void UnsubscribeFromFilterItem(FiltrationComboBoxFilterVM item) {
        if(item is INotifyPropertyChanged notifyItem) {
            notifyItem.PropertyChanged -= OnFilterItemPropertyChanged;
        }
    }

    private void OnFilterItemPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if(e.PropertyName == nameof(FiltrationComboBoxFilterVM.Value)) {
            Dispatcher.BeginInvoke(new Action(UpdateFilteredItems), DispatcherPriority.Background);
        }
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
}
