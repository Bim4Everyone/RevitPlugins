using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

using Autodesk.Revit.DB;

using RevitPackageDocumentation.ViewModels.FiltrationComboBoxVMs;

namespace RevitPackageDocumentation.Views.Controls;
public partial class FiltrationComboBoxControl : UserControl {
    public static readonly DependencyProperty ComboBoxSourceProperty =
        DependencyProperty.Register(nameof(ComboBoxSource), typeof(IEnumerable), typeof(FiltrationComboBoxControl),
            new PropertyMetadata(null, OnComboBoxSourceChanged));

    public static readonly DependencyProperty ComboBoxSelectedProperty =
        DependencyProperty.Register(nameof(ComboBoxSelected), typeof(object), typeof(FiltrationComboBoxControl));

    /// <summary>
    /// Путь до отображаемого свойства элемента. По умолчанию "Name" (для элементов Revit).
    /// Для списка строк нужно передать пустую строку.
    /// </summary>
    public static readonly DependencyProperty DisplayMemberPathProperty =
        DependencyProperty.Register(nameof(DisplayMemberPath), typeof(string), typeof(FiltrationComboBoxControl),
            new PropertyMetadata("Name"));

    public static readonly DependencyProperty FilterListProperty =
        DependencyProperty.Register(nameof(FilterList), typeof(FiltrationComboBoxFilterListVM), typeof(FiltrationComboBoxControl),
            new PropertyMetadata(null, OnFilterListSourceChanged));

    public static readonly DependencyProperty ComboBoxSelectionChangedCommandProperty =
        DependencyProperty.Register(nameof(ComboBoxSelectionChangedCommand), typeof(ICommand), typeof(FiltrationComboBoxControl));

    public static readonly DependencyProperty ItemTemplateProperty = DependencyProperty.Register(
        nameof(ItemTemplate), typeof(DataTemplate), typeof(FiltrationComboBoxControl), new PropertyMetadata(default(DataTemplate)));

    public static readonly DependencyProperty ComboBoxStyleProperty =
        DependencyProperty.Register(nameof(ComboBoxStyle), typeof(Style), typeof(FiltrationComboBoxControl));

    public FiltrationComboBoxControl() {
        InitializeComponent();
    }

    public IEnumerable ComboBoxSource {
        get => (IEnumerable) GetValue(ComboBoxSourceProperty);
        set => SetValue(ComboBoxSourceProperty, value);
    }

    /// <summary>
    /// Выбранный элемент: элемент Revit (Element) или строка
    /// </summary>
    public object ComboBoxSelected {
        get => GetValue(ComboBoxSelectedProperty);
        set => SetValue(ComboBoxSelectedProperty, value);
    }

    public string DisplayMemberPath {
        get => (string) GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    internal FiltrationComboBoxFilterListVM FilterList {
        get => (FiltrationComboBoxFilterListVM) GetValue(FilterListProperty);
        set => SetValue(FilterListProperty, value);
    }

    public ICommand ComboBoxSelectionChangedCommand {
        get => (ICommand) GetValue(ComboBoxSelectionChangedCommandProperty);
        set => SetValue(ComboBoxSelectionChangedCommandProperty, value);
    }

    public DataTemplate ItemTemplate {
        get => (DataTemplate) GetValue(ItemTemplateProperty);
        set => SetValue(ItemTemplateProperty, value);
    }

    public Style ComboBoxStyle {
        get => (Style) GetValue(ComboBoxStyleProperty);
        set => SetValue(ComboBoxStyleProperty, value);
    }

    /// <summary>
    /// Коллекция элементов для ComboBox, после того как проведена фильтрация по фильтрам
    /// </summary>
    public ObservableCollection<object> FilteredItemsSource { get; } = [];


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
        control.UnsubscribeFromFilterList(e.OldValue as FiltrationComboBoxFilterListVM);
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

    /// <summary>
    /// Отписывается от предыдущего списка фильтров, чтобы выгруженный контрол не реагировал на его изменения
    /// </summary>
    private void UnsubscribeFromFilterList(FiltrationComboBoxFilterListVM oldFilterList) {
        if(oldFilterList?.ValueList == null) { return; }

        oldFilterList.ValueList.CollectionChanged -= OnValueListCollectionChanged;
        foreach(var item in oldFilterList.ValueList) {
            UnsubscribeFromFilterItem(item);
        }
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
        // Источник или фильтры становятся null, когда контрол выгружается (например, при переключении листа
        // шаблон компонента уничтожается и привязки к DataContext сбрасываются). Если в этот момент очистить список,
        // ComboBox сбросит SelectedItem в null и через еще не отвязанную двустороннюю привязку ComboBoxSelected
        // запишет null в ViewModel - выбор будет потерян. Поэтому в этом случае список не трогаем.
        if(ComboBoxSource is null || FilterList is null) {
            return;
        }

        var filters = FilterList.ValueList
            .Select(x => x?.Value)
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .ToList();

        var items = ComboBoxSource
            .OfType<object>()
            .Where(item => ItemMatchesAllFilters(item, filters))
            .ToList();

        UpdateFilteredItemsSource(items);

        if(FilteredItemsSource.Count == 1) {
            ComboBoxSelected = FilteredItemsSource.First();
        }
    }

    /// <summary>
    /// Приводит коллекцию ComboBox к нужному составу, изменяя только отличия.
    /// Полная очистка коллекции недопустима: ComboBox на время очистки сбрасывает SelectedItem в null и через
    /// двустороннюю привязку записывает null в ViewModel, из-за чего теряется уже сделанный выбор.
    /// </summary>
    private void UpdateFilteredItemsSource(IReadOnlyList<object> items) {
        // Удаляем то, чего больше нет в новом составе
        for(int i = FilteredItemsSource.Count - 1; i >= 0; i--) {
            if(!items.Contains(FilteredItemsSource[i])) {
                FilteredItemsSource.RemoveAt(i);
            }
        }

        // Добавляем недостающее, сохраняя порядок нового состава
        for(int i = 0; i < items.Count; i++) {
            if(i >= FilteredItemsSource.Count || !Equals(FilteredItemsSource[i], items[i])) {
                FilteredItemsSource.Insert(i, items[i]);
            }
        }

        // Удаляем возможный хвост, если элементов стало меньше
        while(FilteredItemsSource.Count > items.Count) {
            FilteredItemsSource.RemoveAt(FilteredItemsSource.Count - 1);
        }
    }

    /// <summary>
    /// Проверяет, что имя элемента содержит все строки фильтра
    /// </summary>
    private bool ItemMatchesAllFilters(object item, IReadOnlyCollection<string> filters) {
        if(filters.Count == 0) {
            return true;
        }
        string name = GetItemName(item);
        return filters.All(name.Contains);
    }

    /// <summary>
    /// Возвращает имя элемента для фильтрации: строка как есть, для элемента Revit - его имя
    /// </summary>
    private static string GetItemName(object item) {
        return item switch {
            string str => str,
            Element element => element.Name ?? string.Empty,
            _ => item?.ToString() ?? string.Empty
        };
    }

    /// <summary>
    /// Показывает/скрывает видимость фильтров
    /// </summary>
    private void Button_Click(object sender, RoutedEventArgs e) {
        flyout.IsOpen = flyout.IsOpen != true;
    }
}
