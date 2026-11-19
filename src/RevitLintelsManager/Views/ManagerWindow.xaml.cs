using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

using dosymep.SimpleServices;

using RevitLintelsManager.Services;
using RevitLintelsManager.ViewModels;

namespace RevitLintelsManager.Views;

public partial class ManagerWindow {
    private readonly Func<ManagerDetailsWindow> _detailsFactory;
    private readonly WindowPlatformEvents _platformEvents;
    private ManagerViewModel _viewModel;
    private ManagerDetailsWindow _detailsWindow;

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
    public override string ProjectConfigName => nameof(ManagerWindow);

    // Подключает запросы представления к текущей VM и отсоединяет предыдущую.
    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
        CloseDetails();
        if(_viewModel is not null) {
            _viewModel.DetailsRequested -= ShowDetails;
            _viewModel.DetailsDismissRequested -= CloseDetails;
        }
        _viewModel = e.NewValue as ManagerViewModel;
        if(_viewModel is not null) {
            _viewModel.DetailsRequested += ShowDetails;
            _viewModel.DetailsDismissRequested += CloseDetails;
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
        if(_viewModel is not null) {
            _viewModel.DetailsRequested -= ShowDetails;
            _viewModel.DetailsDismissRequested -= CloseDetails;
            _viewModel = null;
        }
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
                _detailsWindow.SetResourceReference(BackgroundProperty, "ApplicationBackgroundBrush");
                _detailsWindow.SetResourceReference(ForegroundProperty, "TextFillColorPrimaryBrush");
            }
        }));
    }

    // Разрешает повторные запросы закрытия без хранения окна для каждой строки.
    private void CloseDetails() {
        _detailsWindow?.Close();
    }
}
