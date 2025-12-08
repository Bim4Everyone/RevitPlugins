using System.Windows;
using System.Windows.Input;

using dosymep.SimpleServices;

namespace RevitParamsChecker.Views.Controls;

internal partial class PageMenuControl {
    public PageMenuControl()
        : base() {
        InitializeComponent();
    }

    public PageMenuControl(
        ILoggerService loggerService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IUIThemeService uiThemeService,
        IUIThemeUpdaterService themeUpdaterService)
        : base(
            loggerService,
            languageService,
            localizationService,
            uiThemeService,
            themeUpdaterService) {
        InitializeComponent();
    }

    public static readonly DependencyProperty LoadButtonCommandProperty = DependencyProperty.Register(
        nameof(LoadButtonCommand),
        typeof(ICommand),
        typeof(PageMenuControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty SaveButtonCommandProperty = DependencyProperty.Register(
        nameof(SaveButtonCommand),
        typeof(ICommand),
        typeof(PageMenuControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty ExportButtonCommandProperty = DependencyProperty.Register(
        nameof(ExportButtonCommand),
        typeof(ICommand),
        typeof(PageMenuControl),
        new PropertyMetadata(null));

    public ICommand LoadButtonCommand {
        get => (ICommand) GetValue(LoadButtonCommandProperty);
        set => SetValue(LoadButtonCommandProperty, value);
    }

    public ICommand SaveButtonCommand {
        get => (ICommand) GetValue(SaveButtonCommandProperty);
        set => SetValue(SaveButtonCommandProperty, value);
    }

    public ICommand ExportButtonCommand {
        get => (ICommand) GetValue(ExportButtonCommandProperty);
        set => SetValue(ExportButtonCommandProperty, value);
    }
}
