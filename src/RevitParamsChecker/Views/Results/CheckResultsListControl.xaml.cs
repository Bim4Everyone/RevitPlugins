using System.Windows;

using dosymep.SimpleServices;

namespace RevitParamsChecker.Views.Results;

internal partial class CheckResultsListControl {
    public CheckResultsListControl() {
        InitializeComponent();
    }

    public CheckResultsListControl(
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

    public static readonly DependencyProperty IsListVisibleProperty = DependencyProperty.Register(
        nameof(IsListVisible),
        typeof(bool),
        typeof(CheckResultsListControl),
        new PropertyMetadata(true));

    public bool IsListVisible {
        get => (bool) GetValue(IsListVisibleProperty);
        set => SetValue(IsListVisibleProperty, value);
    }

    private void ListToggleViewButton_Clicked(object sender, RoutedEventArgs e) {
        IsListVisible = !IsListVisible;
    }
}
