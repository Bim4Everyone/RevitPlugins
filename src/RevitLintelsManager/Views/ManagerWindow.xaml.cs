using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

using dosymep.SimpleServices;

using RevitLintelsManager.Models;
using RevitLintelsManager.Services;
using RevitLintelsManager.ViewModels;

namespace RevitLintelsManager.Views;

public partial class ManagerWindow {
    private readonly struct ScrollAnchor(ManagerGroupKey groupKey, ManagerItemKey? itemKey, double offset) {
        public ManagerGroupKey GroupKey { get; } = groupKey;
        public ManagerItemKey? ItemKey { get; } = itemKey;
        public double Offset { get; } = offset;
    }

    private readonly Func<ManagerDetailsWindow> _detailsFactory;
    private readonly WindowPlatformEvents _platformEvents;
    private readonly Dictionary<ManagerScope, ScrollAnchor> _scrollAnchors = new();
    private ManagerViewModel _viewModel;
    private ManagerDetailsWindow _detailsWindow;
    private DispatcherOperation _restoreScroll;

    /// <summary>Создаёт немодальное окно с отсоединяемыми платформенными событиями.</summary>
    public ManagerWindow(ILoggerService loggerService, ISerializationService serializationService,
        WindowPlatformEvents platformEvents, ILocalizationService localizationService,
        IUIThemeUpdaterService themeUpdaterService, Func<ManagerDetailsWindow> detailsFactory)
        : base(loggerService, serializationService, platformEvents, localizationService,
            platformEvents, themeUpdaterService) {
        InitializeComponent();
        _detailsFactory = detailsFactory;
        _platformEvents = platformEvents;
        DataContextChanged += OnDataContextChanged;
        Closing += OnManagerClosing;
        Closed += OnManagerClosed;
        platformEvents.LanguageChanged += OnLanguageChanged;
        platformEvents.UIThemeChanged += OnThemeChanged;
    }

    public override string PluginName => nameof(RevitLintelsManager);
    public override string ProjectConfigName => nameof(ManagerWindow) + "Layout2";

    // Подключает запросы представления к текущей VM и отсоединяет предыдущую.
    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
        CloseDetails();
        DetachViewModel();
        _viewModel = e.NewValue as ManagerViewModel;
        if(_viewModel is not null) {
            _viewModel.DetailsRequested += ShowDetails;
            _viewModel.DetailsDismissRequested += CloseDetails;
            _viewModel.ProjectionChanging += SaveScrollAnchor;
            _viewModel.ProjectionChanged += ScheduleScrollRestore;
        }
    }

    // Создаёт только одно временное окно; все данные подготовлены ViewModel.
    private void ShowDetails(ManagerDetailsViewModel details) {
        CloseDetails();
        _detailsWindow = _detailsFactory();
        _detailsWindow.Owner = this;
        _detailsWindow.Resources.MergedDictionaries.Add(Resources);
        _detailsWindow.DataContext = details;
        _detailsWindow.Closed += OnDetailsClosed;
        try {
            _detailsWindow.Show();
        } catch {
            CloseDetails();
            throw;
        }
    }

    // Освобождает ссылки временного представления, не затрагивая API/контейнер менеджера.
    private void OnDetailsClosed(object sender, EventArgs e) {
        var window = (ManagerDetailsWindow) sender;
        window.Closed -= OnDetailsClosed;
        window.DataContext = null;
        window.Resources.MergedDictionaries.Clear();
        if(ReferenceEquals(_detailsWindow, window)) {
            _detailsWindow = null;
        }
    }

    // Закрывает мини-окно также при отменённом штатной сессией Closing главного окна.
    private void OnManagerClosing(object sender, CancelEventArgs e) {
        CloseDetails();
    }

    // Снимает только собственные подписки; native lifecycle остаётся у ManagerSession.
    private void OnManagerClosed(object sender, EventArgs e) {
        CloseDetails();
        _platformEvents.LanguageChanged -= OnLanguageChanged;
        _platformEvents.UIThemeChanged -= OnThemeChanged;
        DataContextChanged -= OnDataContextChanged;
        DetachViewModel();
    }

    // Отсоединяет представление от VM и отменяет отложенную прокрутку.
    private void DetachViewModel() {
        _restoreScroll?.Abort();
        _restoreScroll = null;
        _scrollAnchors.Clear();
        if(_viewModel is not null) {
            _viewModel.DetailsRequested -= ShowDetails;
            _viewModel.DetailsDismissRequested -= CloseDetails;
            _viewModel.ProjectionChanging -= SaveScrollAnchor;
            _viewModel.ProjectionChanged -= ScheduleScrollRestore;
            _viewModel = null;
        }
    }

    // Запоминает первую видимую строку и её смещение в текущем диапазоне.
    private void SaveScrollAnchor(ManagerScope scope) {
        if(_restoreScroll?.Abort() == true) {
            _restoreScroll = null;
            return;
        }
        var scrollViewer = FindVisualChild<ScrollViewer>(RowsList);
        var viewport = scrollViewer is null ? null : FindVisualChild<ScrollContentPresenter>(scrollViewer);
        var panel = viewport is null ? null : FindVisualChild<VirtualizingStackPanel>(viewport);
        if(panel is null || scrollViewer.VerticalOffset == 0) {
            _scrollAnchors.Remove(scope);
            return;
        }
        foreach(var container in panel.Children.OfType<ListBoxItem>()) {
            double offset = container.TranslatePoint(new Point(), viewport).Y;
            if(offset + container.ActualHeight <= 0 || offset >= viewport.ActualHeight) {
                continue;
            }
            if(container.Content is ManagerItemViewModel item) {
                _scrollAnchors[scope] = new ScrollAnchor(item.Snapshot.GroupKey, item.Snapshot.Key, offset);
                return;
            }
            if(container.Content is ManagerGroupViewModel group) {
                _scrollAnchors[scope] = new ScrollAnchor(group.Key, null, offset);
                return;
            }
        }
        _scrollAnchors.Remove(scope);
    }

    // Ожидает применения изменений коллекции к генератору контейнеров.
    private void ScheduleScrollRestore(ManagerScope scope) {
        if(IsLoaded) {
            _restoreScroll = Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => RestoreScroll(scope)));
        }
    }

    // Восстанавливает строку по ключу, при её отсутствии пробует заголовок той же группы.
    private void RestoreScroll(ManagerScope scope) {
        _restoreScroll = null;
        var scrollViewer = FindVisualChild<ScrollViewer>(RowsList);
        if(scrollViewer is null) {
            return;
        }
        if(!_scrollAnchors.TryGetValue(scope, out var anchor)) {
            scrollViewer.ScrollToTop();
            return;
        }
        var rows = _viewModel.VisibleRows;
        ManagerRowViewModel row = anchor.ItemKey is ManagerItemKey key
            ? rows.OfType<ManagerItemViewModel>().FirstOrDefault(item => item.Snapshot.Key.Equals(key))
            : null;
        double offset = anchor.Offset;
        if(row is null) {
            row = rows.OfType<ManagerGroupViewModel>().FirstOrDefault(group => group.Key.Equals(anchor.GroupKey));
            offset = anchor.ItemKey.HasValue ? 0 : anchor.Offset;
        }
        if(row is null) {
            scrollViewer.ScrollToTop();
            return;
        }
        if(RowsList.ItemContainerGenerator.ContainerFromItem(row) is not ListBoxItem) {
            RowsList.ScrollIntoView(row);
            RowsList.UpdateLayout();
        }
        var viewport = FindVisualChild<ScrollContentPresenter>(scrollViewer);
        if(viewport is not null && RowsList.ItemContainerGenerator.ContainerFromItem(row) is ListBoxItem container) {
            double delta = container.TranslatePoint(new Point(), viewport).Y - offset;
            if(Math.Abs(delta) >= 0.5) {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset + delta);
            }
        }
    }

    // Находит визуальную часть существующего шаблона WPF без обращения к данным модели.
    private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject {
        for(int index = 0; index < VisualTreeHelper.GetChildrenCount(parent); index++) {
            var child = VisualTreeHelper.GetChild(parent, index);
            if(child is T match) {
                return match;
            }
            var nested = FindVisualChild<T>(child);
            if(nested is not null) {
                return nested;
            }
        }
        return null;
    }

    // Не удерживает сведения строки после recycling или смены положения списка.
    private void OnRowsScrollChanged(object sender, ScrollChangedEventArgs e) {
        if(e.VerticalChange != 0 || e.HorizontalChange != 0) {
            CloseDetails();
        }
    }

    // Переиспользует существующее событие языка платформы, не создавая второй системы локализации.
    private void OnLanguageChanged(CultureInfo language) {
        Dispatcher.BeginInvoke(new Action(() => _viewModel?.RefreshLocalization()));
    }

    // После обновления темы владельца инвалидирует общий словарь в уже открытом мини-окне.
    private void OnThemeChanged(UIThemes theme) {
        Dispatcher.BeginInvoke(new Action(() => {
            SetResourceReference(BackgroundProperty, "ApplicationBackgroundBrush");
            SetResourceReference(ForegroundProperty, "TextFillColorPrimaryBrush");
            if(_detailsWindow is not null) {
                _detailsWindow.Resources.MergedDictionaries.Clear();
                _detailsWindow.Resources.MergedDictionaries.Add(Resources);
                _detailsWindow.SetResourceReference(ForegroundProperty, "TextFillColorPrimaryBrush");
            }
        }));
    }

    // Разрешает повторные запросы закрытия без хранения окна для каждой строки.
    private void CloseDetails() {
        _detailsWindow?.Close();
    }
}
