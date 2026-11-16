using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

using dosymep.SimpleServices;
using dosymep.WpfCore.Behaviors;

using Microsoft.Xaml.Behaviors;

namespace RevitEnumerateBySpline.Views;

/// <summary>Панель подтверждения последовательного выбора без модального диалога.</summary>
internal partial class CurveSelectionWindow {
    private IntPtr _revitWindow;
    private readonly ILocalizationService _localization;
    private bool _isPicking;
    private bool _allowClose;

    /// <summary>Создаёт панель подтверждения выбора с сервисами платформы.</summary>
    /// <param name="loggerService">Сервис журналирования.</param>
    /// <param name="serializationService">Сервис сериализации настроек окна.</param>
    /// <param name="languageService">Сервис текущего языка интерфейса.</param>
    /// <param name="localizationService">Сервис локализованных строк.</param>
    /// <param name="uiThemeService">Сервис текущей темы интерфейса.</param>
    /// <param name="themeUpdaterService">Сервис обновления темы окна.</param>
    public CurveSelectionWindow(
        ILoggerService loggerService,
        ISerializationService serializationService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IUIThemeService uiThemeService,
        IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService, serializationService, languageService,
            localizationService, uiThemeService, themeUpdaterService) {
        InitializeComponent();
        _localization = localizationService;
        Closing += OnClosing;
    }

    public override string PluginName => nameof(RevitEnumerateBySpline);
    public override string ProjectConfigName => nameof(CurveSelectionWindow) + ".Compact";

    /// <summary>Отключает сохранение геометрии и восстанавливает размеры, заданные в XAML.</summary>
    /// <param name="e">Данные события инициализации оконного дескриптора.</param>
    protected override void OnSourceInitialized(EventArgs e) {
        double width = Width;
        double height = Height;
        base.OnSourceInitialized(e);

        // Временная панель использует размеры из XAML, а не сохранённую геометрию.
        var behaviors = Interaction.GetBehaviors(this);
        foreach(var behavior in behaviors.OfType<WpfSavePositionBehavior>().ToArray()) {
            behaviors.Remove(behavior);
        }
        Width = width;
        Height = height;
    }

    /// <summary>Назначает главное окно Revit владельцем панели и адресатом отмены выбора.</summary>
    /// <param name="revitWindow">Дескриптор главного окна Revit.</param>
    public void SetRevitOwner(IntPtr revitWindow) {
        _revitWindow = revitWindow;
        new WindowInteropHelper(this).Owner = revitWindow;
    }

    /// <summary>Результат выбора: подтверждение, отмена или null, пока выбор продолжается.</summary>
    public bool? Accepted { get; private set; }

    /// <summary>Разрешает подтверждение набора после выбора первой линии.</summary>
    public void EnableFinish() {
        _finishButton.IsEnabled = true;
    }

    /// <summary>Отмечает начало ожидания выбора элемента через Revit API.</summary>
    public void BeginPick() => _isPicking = true;

    /// <summary>Отмечает завершение текущего запроса выбора элемента.</summary>
    public void EndPick() => _isPicking = false;

    /// <summary>Закрывает панель после завершения цикла выбора без повторной отмены.</summary>
    public void ClosePanel() {
        _allowClose = true;
        Close();
    }

    /// <summary>Подтверждает выбранный набор по нажатию кнопки «Готово».</summary>
    /// <param name="sender">Кнопка подтверждения.</param>
    /// <param name="e">Данные события нажатия.</param>
    private void Finish_Click(object sender, RoutedEventArgs e) => Complete(true);

    /// <summary>Отменяет текущий набор по нажатию кнопки «Отмена».</summary>
    /// <param name="sender">Кнопка отмены.</param>
    /// <param name="e">Данные события нажатия.</param>
    private void Cancel_Click(object sender, RoutedEventArgs e) => Complete(false);

    /// <summary>Преобразует закрытие панели пользователем в отмену текущего набора.</summary>
    /// <param name="sender">Закрываемая панель.</param>
    /// <param name="e">Данные события с возможностью отменить закрытие.</param>
    private void OnClosing(object? sender, CancelEventArgs e) {
        if(!_allowClose) {
            e.Cancel = true;
            Complete(false);
        }
    }

    /// <summary>Сохраняет результат набора и прерывает активный запрос выбора клавишей Escape.</summary>
    /// <param name="accepted">true для подтверждения набора, false для отмены.</param>
    private void Complete(bool accepted) {
        if(Accepted.HasValue) {
            return;
        }
        Accepted = accepted;
        Hide();
        if(!_isPicking) {
            return;
        }

        // У PickObject нет метода Cancel. Завершаем только текущий запрос выбора
        // одним Escape в очередь окна Revit; следующие PickObject уже не запускаем.
        SetForegroundWindow(_revitWindow);
        IntPtr target = GetFocus();
        if(target == IntPtr.Zero || (target != _revitWindow && !IsChild(_revitWindow, target))) {
            target = _revitWindow;
        }
        if(!PostMessage(target, 0x0100, new IntPtr(0x1B), new IntPtr(0x00010001))) {
            Accepted = null;
            _prompt.Text = _localization.GetLocalizedString("CurveSelectionWindow.StopFailed");
            _prompt.Visibility = Visibility.Visible;
            Show();
            return;
        }
        PostMessage(target, 0x0101, new IntPtr(0x1B), new IntPtr(0xC0010001L));
    }

    /// <summary>Запрашивает перевод указанного окна на передний план.</summary>
    /// <param name="window">Дескриптор окна.</param>
    /// <returns>Признак успешного перевода окна на передний план.</returns>
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetForegroundWindow(IntPtr window);

    /// <summary>Получает окно с фокусом клавиатуры в очереди сообщений текущего потока.</summary>
    /// <returns>Дескриптор окна с фокусом или нулевой дескриптор.</returns>
    [DllImport("user32.dll")]
    private static extern IntPtr GetFocus();

    /// <summary>Проверяет, принадлежит ли окно дереву дочерних окон указанного родителя.</summary>
    /// <param name="parent">Дескриптор родительского окна.</param>
    /// <param name="window">Дескриптор проверяемого окна.</param>
    /// <returns>Признак того, что проверяемое окно является дочерним.</returns>
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsChild(IntPtr parent, IntPtr window);

    /// <summary>Помещает сообщение в очередь потока, которому принадлежит окно.</summary>
    /// <param name="window">Дескриптор окна-получателя.</param>
    /// <param name="message">Код оконного сообщения.</param>
    /// <param name="wParam">Первый параметр сообщения.</param>
    /// <param name="lParam">Второй параметр сообщения.</param>
    /// <returns>Признак успешной постановки сообщения в очередь.</returns>
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool PostMessage(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);
}
