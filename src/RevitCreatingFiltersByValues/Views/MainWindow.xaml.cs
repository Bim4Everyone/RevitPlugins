using System.Windows;
using System.Windows.Media;

using dosymep.SimpleServices;

namespace RevitCreatingFiltersByValues.Views;
public partial class MainWindow {
    public MainWindow(
        ILoggerService loggerService,
        ISerializationService serializationService,
        ILanguageService languageService, ILocalizationService localizationService,
        IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService,
            serializationService,
            languageService, localizationService,
            uiThemeService, themeUpdaterService) {
        InitializeComponent();

        SetExpanderCollapsedBrushes();
        uiThemeService.UIThemeChanged += UiThemeService_UIThemeChanged;
    }

    public override string PluginName => nameof(RevitCreatingFiltersByValues);
    public override string ProjectConfigName => nameof(MainWindow);

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }

    private void UiThemeService_UIThemeChanged(UIThemes obj) {
        if(expander.IsExpanded) {
            SetExpanderExpandedBrushes();
        } else {
            SetExpanderCollapsedBrushes();
        }
    }

    private void ExpanderExpanded(object sender, RoutedEventArgs e) {
        SetExpanderExpandedBrushes();
    }

    private void ExpanderCollapsed(object sender, RoutedEventArgs e) {
        SetExpanderCollapsedBrushes();
    }

    private void SetExpanderExpandedBrushes() {
        var fillBrush = (SolidColorBrush) FindResource("WindowBackground");
        Resources["ExpanderHeaderBackground"] = fillBrush;
        Resources["ExpanderContentBackground"] = fillBrush;

        var borderBrush = (SolidColorBrush) FindResource("AccentButtonBackground");
        Resources["ExpanderHeaderBorderBrush"] = borderBrush;
    }

    private void SetExpanderCollapsedBrushes() {
        var fillBrush = (SolidColorBrush) FindResource("ButtonBackground");
        Resources["ExpanderHeaderBackground"] = fillBrush;
        Resources["ExpanderContentBackground"] = fillBrush;

        var borderBrush = (SolidColorBrush) FindResource("ButtonBorderBrushDisabled");
        Resources["ExpanderHeaderBorderBrush"] = borderBrush;
    }
}
