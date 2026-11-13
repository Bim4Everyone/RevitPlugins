using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;

using dosymep.SimpleServices;

using Wpf.Ui.Controls;

namespace RevitOpeningPlacement.Views.Settings.Controls;
/// <summary>
/// Карточка проверки Навигатора с переключателем справа.
/// <para>Если задано вложенное наполнение, карточка отображается развернутым экспандером.</para>
/// </summary>
[ContentProperty(nameof(NestedContent))]
public partial class StatusToggleControl {
    public StatusToggleControl() : base() {
        InitializeComponent();
    }

    public StatusToggleControl(
        ILoggerService loggerService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IUIThemeService uiThemeService,
        IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService,
            languageService,
            localizationService,
            uiThemeService,
            themeUpdaterService) {
        InitializeComponent();
    }

    public static readonly DependencyProperty IsNestedProperty = DependencyProperty.Register(
        nameof(IsNested), typeof(bool), typeof(StatusToggleControl),
        new FrameworkPropertyMetadata(false, OnIsNestedChanged));

    /// <summary>
    /// Карточка вложена в экспандер другой проверки:
    /// отображается строкой без собственного фона и внешних отступов
    /// </summary>
    public bool IsNested {
        get => (bool) GetValue(IsNestedProperty);
        set => SetValue(IsNestedProperty, value);
    }

    private static void OnIsNestedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if(d is StatusToggleControl control && (bool) e.NewValue) {
            control.Margin = new Thickness(0);
            // Область содержимого экспандера рисуется своим полупрозрачным фоном карточки,
            // который во вложенном экспандере накладывается на фон внешнего
            control._expander.Resources["CardBackground"] = Brushes.Transparent;
        }
    }

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(IconElement), typeof(StatusToggleControl));

    public IconElement Icon {
        get => (IconElement) GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(StatusToggleControl));

    public string Title {
        get => (string) GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description), typeof(string), typeof(StatusToggleControl));

    public string Description {
        get => (string) GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register(
        nameof(IsChecked), typeof(bool), typeof(StatusToggleControl),
        new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public bool IsChecked {
        get => (bool) GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    public static readonly DependencyProperty CostTagProperty = DependencyProperty.Register(
        nameof(CostTag), typeof(string), typeof(StatusToggleControl));

    /// <summary>
    /// Тег стоимости проверки: показывает, экономит ли ее выключение время работы Навигатора
    /// </summary>
    public string CostTag {
        get => (string) GetValue(CostTagProperty);
        set => SetValue(CostTagProperty, value);
    }

    public static readonly DependencyProperty NestedContentProperty = DependencyProperty.Register(
        nameof(NestedContent), typeof(object), typeof(StatusToggleControl),
        new FrameworkPropertyMetadata(null, OnNestedContentChanged));

    /// <summary>
    /// Вложенное наполнение карточки: подчиненные переключатели или поля ввода.
    /// <para>Отображается внутри экспандера под заголовком проверки.</para>
    /// </summary>
    public object NestedContent {
        get => GetValue(NestedContentProperty);
        set => SetValue(NestedContentProperty, value);
    }

    private static readonly DependencyPropertyKey _hasNestedContentPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(HasNestedContent), typeof(bool), typeof(StatusToggleControl),
            new FrameworkPropertyMetadata(false));

    public static readonly DependencyProperty HasNestedContentProperty =
        _hasNestedContentPropertyKey.DependencyProperty;

    /// <summary>
    /// Задано ли вложенное наполнение. Без него область наполнения скрывается
    /// </summary>
    public bool HasNestedContent {
        get => (bool) GetValue(HasNestedContentProperty);
        private set => SetValue(_hasNestedContentPropertyKey, value);
    }

    private static void OnNestedContentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if(d is StatusToggleControl control) {
            control.HasNestedContent = e.NewValue != null;
        }
    }
}
