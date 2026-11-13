using System.Windows;
using System.Windows.Markup;

using dosymep.SimpleServices;

using Wpf.Ui.Controls;

namespace RevitOpeningPlacement.Views.Settings.Controls;
/// <summary>
/// Карточка значения настройки проверки: название слева, поле ввода справа
/// </summary>
[ContentProperty(nameof(EditControl))]
public partial class StatusValueControl {
    public StatusValueControl() : base() {
        InitializeComponent();
    }

    public StatusValueControl(
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

    /// <summary>
    /// Отступ слева для значения, вложенного в экспандер проверки
    /// </summary>
    private const double _nestedIndent = 40;

    public static readonly DependencyProperty IsNestedProperty = DependencyProperty.Register(
        nameof(IsNested), typeof(bool), typeof(StatusValueControl),
        new FrameworkPropertyMetadata(false, OnIsNestedChanged));

    /// <summary>
    /// Карточка вложена в экспандер проверки:
    /// отображается строкой без собственного фона и внешних отступов
    /// </summary>
    public bool IsNested {
        get => (bool) GetValue(IsNestedProperty);
        set => SetValue(IsNestedProperty, value);
    }

    private static void OnIsNestedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
        if(d is StatusValueControl control && (bool) e.NewValue) {
            // Значение - поднастройка проверки, сдвигается вправо, чтобы была видна подчиненность
            control.Margin = new Thickness(_nestedIndent, 0, 0, 0);
        }
    }

    public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
        nameof(Icon), typeof(IconElement), typeof(StatusValueControl));

    public IconElement Icon {
        get => (IconElement) GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
        nameof(Title), typeof(string), typeof(StatusValueControl));

    public string Title {
        get => (string) GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(
        nameof(Description), typeof(string), typeof(StatusValueControl));

    /// <summary>
    /// Короткое описание под названием. Если не задано, скрывается
    /// </summary>
    public string Description {
        get => (string) GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly DependencyProperty EditControlProperty = DependencyProperty.Register(
        nameof(EditControl), typeof(object), typeof(StatusValueControl));

    /// <summary>
    /// Поле ввода значения, отображается справа
    /// </summary>
    public object EditControl {
        get => GetValue(EditControlProperty);
        set => SetValue(EditControlProperty, value);
    }
}
